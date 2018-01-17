using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoadGenTester_GridOrthoBasis : MonoBehaviour
{
    public float Rotation = 0.0f,
                 Importance = 1.0f;


    public Generators.Road.GridOrthoBasis OrthoBasis
    {
        get
        {
            Generators.Road.GridOrthoBasis g = new Generators.Road.GridOrthoBasis(Rotation);
            g.Center = transform.position;
            g.Importance = Importance;
            return g;
        }
    }


    void OnDrawGizmos()
    {
        Generators.Road.GridOrthoBasis g = OrthoBasis;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(g.Center, g.Center + (g.MajorAxis * g.Importance * 5.0f));
        Gizmos.color = Color.grey;
        Gizmos.DrawLine(g.Center, g.Center + (g.MinorAxis * g.Importance * 5.0f));
    }
}