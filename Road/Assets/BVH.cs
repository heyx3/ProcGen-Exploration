using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Assert = UnityEngine.Assertions.Assert;


namespace BVH
{
    //TODO: Currently nodes are not stored near their children, but as a result ref indexes can be found via binary search and current highest ID is the last element in the node list. See if this trade-off was worth it.


    /// <summary>
    /// A collection of data indexed based on the data's "bounding volume",
    ///     a.k.a. an axis-aligned bounding box.
    /// Provides efficient querying of spacially-organized data.
    /// </summary>
    public abstract class BVH<DataType>
        where DataType : IEquatable<DataType>
    {
        /// <summary>
        /// A node in the tree will split into two child nodes if it has more than "Threshold" elements.
        /// </summary>
        public int Threshold;

        private List<Node<DataType>> nodes = new List<Node<DataType>>();
        private List<int> nodeIDs = new List<int>();


        public int Count { get; private set; }
        internal int CountNodes { get; private set; }

        internal NodeRef<DataType> Root { get { return new NodeRef<DataType>(this, 0, 0); } }

        internal uint versionNum { get; private set; }

        internal int GetIndex(int nodeID)
        {
            //Binary search.

            int start = 0,
                end = nodeIDs.Count - 1;
            while (start <= end)
            {
                int i = (start + end) / 2;

                if (nodeIDs[i] == nodeID)
                    return i;
                else if (nodeIDs[i] < nodeID)
                    start = i + 1;
                else
                    end = i - 1;
            }

            return -1;
        }
        internal Node<DataType> GetNode(int nodeIndex) { return nodes[nodeIndex]; }


        public BVH(int threshold)
        {
            Threshold = threshold;
            Clear();
        }


        public void Clear()
        {
            nodes.Clear();
            nodeIDs.Clear();

            nodes.Add(new Node<DataType>(this, 0, Threshold));
            nodeIDs.Add(0);

            Count = 0;
            CountNodes = 1;

            unchecked { versionNum += 1; }
        }

        public void Add(DataType data)
        {
            Count += 1;

            int nodeRefIndex = Root.Index;
            Rect bnds = GetBounds(data);
            float centerX = bnds.center.x;

            //Go down through the tree to find a leaf to use.
            while (!nodes[nodeRefIndex].IsLeaf)
            {
                nodes[nodeRefIndex] = nodes[nodeRefIndex].GetBoundsUnionWith(bnds);

                //Remember that items are sorted based on X position.
                if (Mathf.Abs(centerX - nodes[nodeRefIndex].Child1.Node.Bounds.center.x) <
                    Mathf.Abs(centerX - nodes[nodeRefIndex].Child2.Node.Bounds.center.x))
                {
                    nodeRefIndex = nodes[nodeRefIndex].Child1.Index;
                }
                else
                {
                    nodeRefIndex = nodes[nodeRefIndex].Child2.Index;
                }
            }

            //Add the data to the leaf.
            nodes[nodeRefIndex].LeafData.Add(data);
            if (nodes[nodeRefIndex].LeafData.Count == 1)
                nodes[nodeRefIndex] = nodes[nodeRefIndex].GetWithNewBounds(bnds);
            else
                nodes[nodeRefIndex] = nodes[nodeRefIndex].GetBoundsUnionWith(bnds);

            //Split the leaf if there are too many elements.
            if (nodes[nodeRefIndex].LeafData.Count > Threshold)
                SplitNode(nodeRefIndex);
        }
        public void Remove(DataType data)
        {
            List<int> pathFromLeaf = new List<int>();
            int nodeRefIndex = FindLeaf(data, GetBounds(data), pathFromLeaf);

            Assert.IsTrue(nodeRefIndex >= 0, "Couldn't find data " + data.ToString() + " in BVH");

            //Remove the data from the leaf.
            nodes[nodeRefIndex].LeafData.Remove(data);

            //Update the bounding volumes of the parent nodes.
            for (int i = 0; i < pathFromLeaf.Count; ++i)
                nodes[pathFromLeaf[i]] = nodes[pathFromLeaf[i]].GetComputedBounds();

            //If that was the last element in the node, clean up this part of the tree.
            if (nodes[nodeRefIndex].LeafData.Count < 1 && nodeRefIndex != Root.Index)
            {
                int parentIndex = pathFromLeaf[1];
                int otherChld = (nodes[parentIndex].Child1.Index == nodeRefIndex ?
                                    nodes[parentIndex].Child2.Index :
                                    nodes[parentIndex].Child1.Index);
				int otherChildID = nodeIDs[otherChld];

                if (nodes[otherChld].IsLeaf)
                {
                    nodes[parentIndex] = nodes[parentIndex].GetAsLeaf(nodes[otherChld].LeafData,
                                                                      nodes[otherChld].Bounds);
                }
                else
                {
                    nodes[parentIndex] = nodes[parentIndex].GetChangedChildren(nodes[otherChld].Child1,
                                                                               nodes[otherChld].Child2);
                }

                RemoveNode(new NodeRef<DataType>(this, nodeIDs[nodeRefIndex], nodeRefIndex));
                RemoveNode(new NodeRef<DataType>(this, otherChildID));
            }

            Count -= 1;
        }

        public bool Contains(DataType data)
        {
            Rect bnds = GetBounds(data);

            if (!Root.Node.Bounds.Overlaps(bnds))
                return false;

            int index = FindLeaf(data, bnds);
            return index >= 0;
        }

        public IEnumerable<DataType> GetAll()
        {
            for (int i = 0; i < nodes.Count; ++i)
                if (nodes[i].IsLeaf)
                    for (int j = 0; j < nodes[i].LeafData.Count; ++j)
                        yield return nodes[i].LeafData[j];
        }
        public IEnumerable<DataType> GetAllNearby(DataType data)
        {
            return GetAllNearbyBnds(GetBounds(data));
        }
        public IEnumerable<DataType> GetAllNearbyBnds(Rect bnds)
        {
            if (!Root.Node.Bounds.Overlaps(bnds))
                yield break;


            //Traverse the tree until the data point is found.

            Stack<int> indicesToSearch = new Stack<int>();
            indicesToSearch.Push(Root.Index);

            while (indicesToSearch.Count > 0)
            {
                int nodeRefIndex = indicesToSearch.Pop();

                //If this is a leaf, see what's inside here.
                if (nodes[nodeRefIndex].IsLeaf)
                {
                    for (int i = 0; i < nodes[nodeRefIndex].LeafData.Count; ++i)
                        if (GetBounds(nodes[nodeRefIndex].LeafData[i]).Overlaps(bnds))
                            yield return nodes[nodeRefIndex].LeafData[i];
                }
                //Otherwise, see if the child nodes are worth searching.
                else
                {
                    int child1 = nodes[nodeRefIndex].Child1.Index,
                        child2 = nodes[nodeRefIndex].Child2.Index;
                    if (nodes[child1].Bounds.Overlaps(bnds))
                        indicesToSearch.Push(child1);
                    if (nodes[child2].Bounds.Overlaps(bnds))
                        indicesToSearch.Push(child2);
                }
            }
        }
        public IEnumerable<DataType> GetAllNearbyPos(Vector2 pos)
        {
            if (!Root.Node.Bounds.Contains(pos))
                yield break;


            //Traverse the tree until the data point is found.

            Stack<int> indicesToSearch = new Stack<int>();
            indicesToSearch.Push(Root.Index);

            while (indicesToSearch.Count > 0)
            {
                int nodeRefIndex = indicesToSearch.Pop();

                //If this is a leaf, see what's inside here.
                if (nodes[nodeRefIndex].IsLeaf)
                {
                    for (int i = 0; i < nodes[nodeRefIndex].LeafData.Count; ++i)
                        if (GetBounds(nodes[nodeRefIndex].LeafData[i]).Contains(pos))
                            yield return nodes[nodeRefIndex].LeafData[i];
                }
                //Otherwise, see if the child nodes are worth searching.
                else
                {
                    int child1 = nodes[nodeRefIndex].Child1.Index,
                        child2 = nodes[nodeRefIndex].Child2.Index;
                    if (nodes[child1].Bounds.Contains(pos))
                        indicesToSearch.Push(child1);
                    if (nodes[child2].Bounds.Contains(pos))
                        indicesToSearch.Push(child2);
                }
            }
        }


        Dictionary<int, int> _childToParent = new Dictionary<int,int>();
        private int FindLeaf(DataType data, Rect dataBnds, List<int> outPathFromLeaf = null)
        {
            int nodeRefIndex = -1;

            if (outPathFromLeaf != null)
            {
                _childToParent.Clear();
                _childToParent.Add(Root.Index, -1);
            }

            Stack<KeyValuePair<int, int>> indicesToSearch = new Stack<KeyValuePair<int, int>>();
            indicesToSearch.Push(new KeyValuePair<int, int>(Root.Index, -1));

            while (indicesToSearch.Count > 0)
            {
                var childAndParent = indicesToSearch.Pop();
                nodeRefIndex = childAndParent.Key;

                //If this is a leaf, see whether the data is contained in here.
                if (nodes[nodeRefIndex].IsLeaf)
                {
                    if (nodes[nodeRefIndex].LeafData.Contains(data))
                    {
                        if (outPathFromLeaf != null)
                        {
                            outPathFromLeaf.Add(nodeRefIndex);
                            int parent = _childToParent[nodeRefIndex];
                            while (parent >= 0)
                            {
                                outPathFromLeaf.Add(parent);
                                parent = _childToParent[parent];
                            }
                        }
                        return nodeRefIndex;
                    }
                }
                //Otherwise, see if the child nodes are worth searching.
                else
                {
                    int child1 = nodes[nodeRefIndex].Child1.Index,
                        child2 = nodes[nodeRefIndex].Child2.Index;
                    if (nodes[child1].Bounds.Overlaps(dataBnds))
                    {
                        indicesToSearch.Push(new KeyValuePair<int, int>(child1, nodeRefIndex));
                        if (outPathFromLeaf != null)
                            _childToParent.Add(child1, nodeRefIndex);
                    }
                    if (nodes[child2].Bounds.Overlaps(dataBnds))
                    {
                        indicesToSearch.Push(new KeyValuePair<int, int>(child2, nodeRefIndex));
                        if (outPathFromLeaf != null)
                            _childToParent.Add(child2, nodeRefIndex);
                    }
                }
            }
            
            return -1;
        }

        private void SplitNode(int nodeIndex)
        {
            //Sort the items by their X position.
            List<float> centerPoses = nodes[nodeIndex].LeafData.Select(d => GetBounds(d).center.x).ToList();
            nodes[nodeIndex].LeafData.Sort((d1, di1, d2, di2) =>
            {
                if (centerPoses[di1] < centerPoses[di2])
                    return -1;
                else if (centerPoses[di1] > centerPoses[di2])
                    return 1;
                else
                    return 0;
            });


            //Put the first half of the items in the first child node.
            //Put the second half of the items in the second child node.
            NodeRef<DataType> child1 = MakeNode(),
                              child2 = MakeNode();
            int i1 = child1.Index,
                i2 = child2.Index;
            for (int i = 0; i < nodes[nodeIndex].LeafData.Count; ++i)
            {
                if (i <= (nodes[nodeIndex].LeafData.Count / 2))
                    nodes[i1].LeafData.Add(nodes[nodeIndex].LeafData[i]);
                else
                    nodes[i2].LeafData.Add(nodes[nodeIndex].LeafData[i]);
            }

            nodes[nodeIndex] = nodes[nodeIndex].GetNonLeafVersion(child1, child2);
            nodes[i1] = nodes[i1].GetComputedBounds();
            nodes[i2] = nodes[i2].GetComputedBounds();
        }

        private NodeRef<DataType> MakeNode()
        {
            CountNodes += 1;

            int newID = nodeIDs[nodeIDs.Count - 1] + 1;
            nodes.Add(new Node<DataType>(this, newID, Threshold));
            nodeIDs.Add(newID);
            return new NodeRef<DataType>(this, newID, nodeIDs.Count - 1);
        }
        private void RemoveNode(NodeRef<DataType> n)
        {
            int i = n.Index;
            if (i >= 0)
            {
                nodes.RemoveAt(i);
                nodeIDs.RemoveAt(i);

                unchecked { versionNum += 1; }
                CountNodes -= 1;
            }
        }

        public abstract Rect GetBounds(DataType data);
    }



    internal struct Node<DataType>
        where DataType : IEquatable<DataType>
    {
        public int ID;
        public NodeRef<DataType> Child1;
        public NodeRef<DataType> Child2;
        public List<DataType> LeafData;
        public Rect Bounds;


        public BVH<DataType> Owner { get { return Child1.Owner; } }

        public bool IsLeaf { get { return LeafData != null; } }


        public Node(BVH<DataType> owner, int id, int threshold)
        {
            ID = id;
            Child1 = new NodeRef<DataType>(owner);
            Child2 = new NodeRef<DataType>(owner);
            LeafData = new List<DataType>(threshold);
            Bounds = new Rect();
        }


        public Node<DataType> GetNonLeafVersion(NodeRef<DataType> child1, NodeRef<DataType> child2)
        {
            Node<DataType> n = this;
            n.LeafData = null;
            n.Child1 = child1;
            n.Child2 = child2;
            return n;
        }
        public Node<DataType> GetComputedBounds()
        {
            Node<DataType> n = this;
            if (n.IsLeaf)
            {
                if (n.LeafData.Count > 0)
                {
                    n.Bounds = Owner.GetBounds(n.LeafData[0]);
                    for (int i = 1; i < n.LeafData.Count; ++i)
                        n.Bounds = n.Bounds.UnionWith(Owner.GetBounds(n.LeafData[i]));
                }
                else
                {
                    n.Bounds = new Rect();
                }
            }
            else
            {
                int c1 = Child1.Index,
                    c2 = Child2.Index;
                bool isEmpty1 = (Owner.GetNode(c1).IsLeaf && Owner.GetNode(c1).LeafData.Count == 0),
                     isEmpty2 = (Owner.GetNode(c2).IsLeaf && Owner.GetNode(c2).LeafData.Count == 0);
                if (isEmpty1)
                    if (isEmpty2)
                        n.Bounds = new Rect();
                    else
                        n.Bounds = Owner.GetNode(c2).Bounds;
                else
                    if (isEmpty2)
                        n.Bounds = Owner.GetNode(c1).Bounds;
                    else
                        n.Bounds = Owner.GetNode(c1).Bounds.UnionWith(Owner.GetNode(c2).Bounds);
            }

            return n;
        }
        public Node<DataType> GetBoundsUnionWith(Rect bnds)
        {
            Node<DataType> n = this;
            n.Bounds = n.Bounds.UnionWith(bnds);
            return n;
        }
        public Node<DataType> GetWithNewBounds(Rect bnds)
        {
            Node<DataType> n = this;
            n.Bounds = bnds;
            return n;
        }
        public Node<DataType> GetChangedChildren(NodeRef<DataType> child1, NodeRef<DataType> child2)
        {
            Node<DataType> n = this;
            n.Child1 = child1;
            n.Child2 = child2;
            return n.GetComputedBounds();
        }
        public Node<DataType> GetAsLeaf(List<DataType> leafData, Rect bnds)
        {
            Node<DataType> n = this;
            n.Child1 = new NodeRef<DataType>(n.Child1.Owner);
            n.Child2 = new NodeRef<DataType>(n.Child2.Owner);
            n.LeafData = leafData;
            n.Bounds = bnds;
            return n;
        }
    }


    internal struct NodeRef<DataType>
        where DataType : IEquatable<DataType>
    {
        public BVH<DataType> Owner;

        private uint versionNum;
        private int id, index;


        public int Index { get { UpdateIndex(); return index; } }
        
        public bool IsValid { get { return Index >= 0; } }
        public Node<DataType> Node { get { UpdateIndex(); return Owner.GetNode(index); } }


        public NodeRef(BVH<DataType> owner, int _id = -1)
        {
            Owner = owner;
            versionNum = Owner.versionNum;
            id = _id;
            index = (id == -1 ? -1 : owner.GetIndex(id));
        }
        public NodeRef(BVH<DataType> owner, int _id, int _index)
        {
            Owner = owner;
            versionNum = owner.versionNum;
            id = _id;
            index = _index;
        }

        
        private void UpdateIndex()
        {
            if (Owner.versionNum != versionNum)
            {
                versionNum = Owner.versionNum;
                index = (id < 0 ? -1 : Owner.GetIndex(id));
            }
        }
    }

    
    /// <summary>
    /// Exposes internal information about a BVH for debugging.
    /// </summary>
    public static class BVHTester
    {
        /// <summary>
        /// A bounding volume and the "Depth" in the tree it exists at.
        /// </summary>
        public struct RectAndDepth
        {
            public Rect R;
            public int Depth;
            public RectAndDepth(Rect r, int depth) { R = r; Depth = depth; }
        }
        /// <summary>
        /// A node (stored as an index into the BVH's "nodes" list) and the "depth" in the tree it exists at.
        /// </summary>
        private struct NodeAndDepth
        {
            public int NodeIndex;
            public int Depth;
            public NodeAndDepth(int nodeIndex, int depth) { NodeIndex = nodeIndex; Depth = depth; }
        }


        /// <summary>
        /// Gets the number of nodes total in the given BVH.
        /// </summary>
        public static int GetNNodes<DataType>(BVH<DataType> bvh) where DataType : IEquatable<DataType> { return bvh.CountNodes; }

        /// <summary>
        /// Gets all nodes in the given BVH between the given levels of depth, inclusive.
        /// Returns nodes in ascending order by depth.
        /// </summary>
        public static IEnumerable<RectAndDepth> GetNodeBounds<DataType>(BVH<DataType> bvh,
                                                                        int minDepth, int maxDepth)
            where DataType : IEquatable<DataType>
        {
            Queue<NodeAndDepth> toSearch = new Queue<NodeAndDepth>();
            toSearch.Enqueue(new NodeAndDepth(bvh.Root.Index, 0));

            while (toSearch.Count > 0)
            {
                NodeAndDepth nAndD = toSearch.Dequeue();

                if (nAndD.Depth > maxDepth)
                    continue;
                
                if (nAndD.Depth >= minDepth)
                    yield return new RectAndDepth(bvh.GetNode(nAndD.NodeIndex).Bounds, nAndD.Depth);

                if (!bvh.GetNode(nAndD.NodeIndex).IsLeaf)
                {
                    toSearch.Enqueue(new NodeAndDepth(bvh.GetNode(nAndD.NodeIndex).Child1.Index, nAndD.Depth + 1));
                    toSearch.Enqueue(new NodeAndDepth(bvh.GetNode(nAndD.NodeIndex).Child2.Index, nAndD.Depth + 1));
                }
            }
        }
    }

    /*
    /// <summary>
    /// BVH stands for "Bounding Volume Hierarchy".
    /// This class is a node in a binary search tree;
    ///     each leaf node contains a collection of nearby items.
    /// In other words, this is just a collection that can efficiently search through a 2D space of data.
    /// </summary>
    [Serializable]
    public abstract class BNode<DataType>
        where DataType : IEquatable<DataType>
    {
        /// <summary>
        /// Once a leaf node has more than "Threshold" elements,
        ///     it will break itself up into two child leaf nodes
        ///     and efficiently distribute the elements among them.
        /// </summary>
        public int Threshold;

        /// <summary>
        /// The area covered by this node's elements.
        /// </summary>
        public Rect BoundingVolume;


        /// <summary>
        /// Whether this node has any child nodes.
        /// </summary>
        public bool IsLeaf { get { return child1 == null; } }

        /// <summary>
        /// The total number of elements in the tree starting from here.
        /// </summary>
        public int TotalNumberOfElements { get; private set; }

        /// <summary>
        /// The elements contained in this node.
        /// Only valid if this is a leaf.
        /// </summary>
        public IEnumerable<DataType> Elements { get { Assert.IsTrue(IsLeaf); return elements; } }
        /// <summary>
        /// The direct children of this node.
        /// Only valid if this is *not* a leaf.
        /// </summary>
        public IEnumerable<BNode<DataType>> Children { get { Assert.IsTrue(!IsLeaf); yield return child1; yield return child2; } }

        /// <summary>
        /// Gets all elements contained within the tree starting at this node.
        /// </summary>
        public IEnumerable<DataType> AllElements
        {
            get
            {
                if (IsLeaf)
                {
                    foreach (DataType dat in elements)
                        yield return dat;
                }
                else
                {
                    foreach (DataType el in child1.elements)
                        yield return el;
                    foreach (DataType el in child2.elements)
                        yield return el;
                }
            }
        }


        [SerializeField]
        private List<DataType> elements = new List<DataType>();
        [SerializeField]
        private BNode<DataType> child1 = null,
                               child2 = null;


        public BNode(int threshold)
        {
            Threshold = threshold;
            BoundingVolume = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
            TotalNumberOfElements = 0;
        }


        /// <summary>
        /// Adds the given element to this node.
        /// </summary>
        public void AddElement(DataType el)
        {
            Rect r = GetBounds(el);
            if (TotalNumberOfElements == 0)
            {
                BoundingVolume = r;
            }
            else
            {
                BoundingVolume = Rect.MinMaxRect(Mathf.Min(BoundingVolume.xMin, r.xMin),
                                                 Mathf.Min(BoundingVolume.yMin, r.yMin),
                                                 Mathf.Max(BoundingVolume.xMax, r.xMax),
                                                 Mathf.Max(BoundingVolume.yMax, r.yMax));
            }

            TotalNumberOfElements += 1;


            if (IsLeaf)
            {
                elements.Add(el);

                //If we're over the threshold, break up into smaller pieces.
                if (elements.Count > Threshold)
                {
                    //Sort by X position.
                    List<float> centerPoses = elements.Select(d => GetBounds(d).center.x).ToList();
                    elements.Sort((dt1, i1, dt2, i2) =>
                        {
                            if (centerPoses[i1] < centerPoses[i2])
                                return -1;
                            else if (centerPoses[i1] > centerPoses[i2])
                                return 1;
                            else return 0;
                        });

                    child1 = CreateNew();
                    child2 = CreateNew();
                    for (int i = 0; i < elements.Count; ++i)
                    {
                        if (i < (elements.Count / 2))
                        {
                            child1.AddElement(elements[i]);
                        }
                        else
                        {
                            child2.AddElement(elements[i]);
                        }
                    }

                    elements.Clear();
                }
            }
            else
            {
                //Pass it down to the child with the closest X position.
                
                float newPos = r.center.x;
                float dist1 = Mathf.Abs(child1.BoundingVolume.center.x - newPos),
                      dist2 = Mathf.Abs(child2.BoundingVolume.center.x - newPos);

                if (dist1 < dist2)
                {
                    child1.AddElement(el);
                }
                else
                {
                    child2.AddElement(el);
                }
            }
        }
        /// <summary>
        /// Removes the given element from this node or its children.
        /// Returns whether the element was successfully found and removed.
        /// </summary>
        public bool RemoveElement(DataType el)
        {
            if (IsLeaf)
            {
                int index = elements.IndexOf(el);
                if (index > -1)
                {
                    elements.RemoveAt(index);
                    TotalNumberOfElements -= 1;
                    return true;
                }
            }
            else
            {
                Rect r = GetBounds(el);

                if ((r.Overlaps(child1.BoundingVolume) && child1.RemoveElement(el)) ||
                    (r.Overlaps(child2.BoundingVolume) && child2.RemoveElement(el)))
                {
                    TotalNumberOfElements -= 1;
                    
                    //If there are no elements left, turn into a leaf.
                    if (TotalNumberOfElements == 0)
                    {
                        child1 = null;
                        child2 = null;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Finds all elements near the given position.
        /// </summary>
        public IEnumerable<DataType> GetAllElementsNear(Vector2 pos)
        {
            if (IsLeaf)
            {
                for (int i = 0; i < elements.Count; ++i)
                    if (GetBounds(elements[i]).Contains(pos))
                        yield return elements[i];
            }
            else
            {
                if (child1.BoundingVolume.Contains(pos))
                    foreach (DataType t in child1.GetAllElementsNear(pos))
                        yield return t;
                if (child2.BoundingVolume.Contains(pos))
                    foreach (DataType t in child2.GetAllElementsNear(pos))
                        yield return t;
            }
        }
        /// <summary>
        /// Finds all elements near the given region.
        /// </summary>
        public IEnumerable<DataType> GetAllElementsNear(Rect region)
        {
            if (IsLeaf)
            {
                for (int i = 0; i < elements.Count; ++i)
                    if (GetBounds(elements[i]).Overlaps(region))
                        yield return elements[i];
            }
            else
            {
                if (child1.BoundingVolume.Overlaps(region))
                    foreach (DataType t in child1.GetAllElementsNear(region))
                        yield return t;
                if (child2.BoundingVolume.Overlaps(region))
                    foreach (DataType t in child2.GetAllElementsNear(region))
                        yield return t;
            }
        }


        protected abstract Rect GetBounds(DataType d);
        protected abstract BNode<DataType> CreateNew();
    }
    */
}