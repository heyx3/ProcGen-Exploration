using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A component that can render watercolor blotches.
/// It can generate a "base" blotch, then generate "instance" blotches that use it as a starting point.
/// </summary>
[Serializable]
public class WaterColorBrush
{
    public int RenderSize = 512;

    /// <summary>
    /// The number of initial points on the shape that will become a blotch.
    /// </summary>
    public int InitialBlotchPoints = 10;
    /// <summary>
    /// The amount of randomness in the initial points generated for a blotch.
    /// Should be between 0 and 0.5 for best results.
    /// </summary>
    public float InitialBlotchVariation = 0.1f;
    /// <summary>
    /// Affects how different each edge is in the initial shape.
    /// </summary>
    public float InitialVarianceSpread = 0.5f;

    /// <summary>
    /// The number of times to iterate the base shape.
    /// </summary>
    public int InitialBlotchIterations = 3;
    /// <summary>
    /// The number of times to iterate the "instance" shape beyond the base shape.
    /// </summary>
    public int ExtraBlotchIterations = 3;
    /// <summary>
    /// The scale of the randomization done to a blotch when it's iterated,
    ///     indexed by the iteration number.
    /// </summary>
    public AnimationCurve BlotchVarianceScale = new AnimationCurve(new Keyframe(0.0f, 0.25f),
                                                                   new Keyframe(6.0f, 0.00625f));

    /// <summary>
    /// This scale is applied to the variance of the new line segments
    ///     during each iteration of the shape.
    /// </summary>
    public float IterationVarianceDecreaseScale = 0.95f;
    /// <summary>
    /// A random value based on this field is also applied to the new line segments
    ///     during each iteration of the shape.
    /// For example, a value of 1.1 means that a scale value between 
    ///     1.1 and 1/1.1 is randomly applied to each new line segment's variance.
    /// </summary>
    public float IterationVarianceDecreaseScaleVariance = 1.1f;


    /// <summary>
    /// The basic blotch shape being used.
    /// </summary>
    public PolyShape BaseShape
    {
        get { return baseShape; }
        set { baseShape = value; InstanceShape = baseShape; }
    }
    /// <summary>
    /// A more detailed and unique version of the blotch shape.
    /// Usually a watercolor render will take a single basic blotch shape and randomize it.
    /// </summary>
    public PolyShape InstanceShape { get; private set; }

    public RenderTexture InstanceShapeRender { get { return shapeRender; } }

    private PolyShape baseShape;
    private RenderTexture shapeRender;
    private Mesh shapeMesh;

    private static Material shapeRenderMat;
    private static Shader stencilPass, renderPass;

    
    /// <summary>
    /// Generates a new BaseShape value.
    /// The InstanceShape will be replaced with this value as well.
    /// </summary>
    public void GenerateBaseBlotch()
    {
        Update();

        //Generate an initial shape by perturbing a circle.
        BaseShape = new PolyShape(1.0f - InitialBlotchVariation, InitialBlotchVariation,
                                  InitialBlotchPoints, InitialVarianceSpread);

        Subdivide(BaseShape, 0, InitialBlotchIterations - 1);
    }
    /// <summary>
    /// Generates a new InstanceShape value based off the BaseShape.
    /// Also rasterizes it into "InstanceShapeRender".
    /// </summary>
    public void GenerateInstanceBlotch()
    {
        Update();

        UnityEngine.Assertions.Assert.IsNotNull(BaseShape, "Didn't generate a base shape first!");

        //Make a copy of the base shape.
        var points = new PolyShape.Point[BaseShape.NPoints];
        for (int i = 0; i < points.Length; ++i)
            points[i] = new PolyShape.Point(BaseShape.GetPoint(i), baseShape.GetVariance(i));
        InstanceShape = new PolyShape(points);

        Subdivide(InstanceShape,
                  InitialBlotchIterations,
                  InitialBlotchIterations + ExtraBlotchIterations - 1);


        //Render the instance blotch to the texture.

        //Set up the mesh, using a simple line strip.
        shapeMesh.Clear();
        shapeMesh.vertices = InstanceShape.Points.Select(v => new Vector3(v.x, v.y, 0.01f)).ToArray();
        shapeMesh.SetIndices(InstanceShape.NPoints.CountSequence(1).ToArray(),
                             MeshTopology.LineStrip, 0);
        shapeMesh.UploadMeshData(false);

        //Render the shape as white on a black transparent background.
        var oldRendTex = RenderTexture.active;
        RenderTexture.active = InstanceShapeRender;
        GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        shapeRenderMat.shader = stencilPass;
        shapeRenderMat.SetVector("_ShapeMin", InstanceShape.Min);
        shapeRenderMat.SetVector("_ShapeMax", InstanceShape.Max);
        shapeRenderMat.SetVector("_PointOnShape",
                                 MathF.Lerp(-1.0f, 1.0f,
                                            MathF.InverseLerp(InstanceShape.Min, InstanceShape.Max,
                                                              InstanceShape.GetPoint(0))));
        shapeRenderMat.SetPass(0);
        Graphics.DrawMeshNow(shapeMesh, Matrix4x4.identity);
        shapeRenderMat.shader = renderPass;
        shapeRenderMat.color = Color.white;
        shapeRenderMat.SetPass(0);
        Graphics.DrawMeshNow(ScreenQuadRenderer.QuadMesh, Matrix4x4.identity);
        RenderTexture.active = oldRendTex;
    }

    /// <summary>
    /// Looks for any changes in this instance's public fields.
    /// </summary>
    private void Update()
    {
        if (shapeMesh == null)
            shapeMesh = new Mesh();
        
        if (stencilPass == null)
            stencilPass = Shader.Find("PolyShape/StencilPass");
        if (renderPass == null)
            renderPass = Shader.Find("PolyShape/ColorPass");

        if (shapeRenderMat == null)
            shapeRenderMat = new Material(stencilPass);
        
        if (shapeRender == null || shapeRender.width != RenderSize)
        {
            shapeRender = new RenderTexture(RenderSize, RenderSize, 24,
                                            RenderTextureFormat.ARGB32,
                                            RenderTextureReadWrite.Linear);
            if (!shapeRender.Create())
                Debug.LogError("Couldn't create render texture??");
        }
    }
    /// <summary>
    /// Subdivides the given shape for the given set of iterations.
    /// </summary>
    /// <param name="startI">The index of the iteration to start at.</param>
    /// <param name="endI">The index of the iteration to end at.</param>
    private void Subdivide(PolyShape shape, int startI, int endI)
    {
        //Define the algorithm that subdivides each segment.
        float pointVariance = float.NaN;
        float minVarianceScale = IterationVarianceDecreaseScale / IterationVarianceDecreaseScaleVariance,
              maxVarianceScale = IterationVarianceDecreaseScale * IterationVarianceDecreaseScaleVariance;
        Func<Vector2, Vector2, float, PolyShape.SplitResult> subdivider = (start, end, variance) =>
        {
            //Perturb the midpoint using a gaussian distribution.
            Vector2 midpoint = (start + end) * 0.5f;
            float dist = MathF.NextGaussian() * pointVariance * variance;
            float angle = UnityEngine.Random.value * Mathf.PI * 2.0f;
            Vector2 pos = midpoint + (dist * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));

            //The variance in the two new edges is a randomly scaled-down version of the old edge's variance.
            float newVariance1 = variance * Mathf.Lerp(minVarianceScale, maxVarianceScale,
                                                       UnityEngine.Random.value),
                  newVariance2 = variance * Mathf.Lerp(minVarianceScale, maxVarianceScale,
                                                       UnityEngine.Random.value);

            return new PolyShape.SplitResult(pos, newVariance1, newVariance2);
        };

        //Subdivide multiple times.
        for (int i = startI; i <= endI; ++i)
        {
            pointVariance = BlotchVarianceScale.Evaluate((float)i);
            shape.Subdivide(subdivider);
        }
    }
}