using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoadGenTester_RadialOrthoBasis : MonoBehaviour
{
    public float Importance = 1.0f;


    public Generators.Road.RadialOrthoBasis OrthoBasis
    {
        get
        {
            Generators.Road.RadialOrthoBasis r = new Generators.Road.RadialOrthoBasis();
            r.Center = transform.position;
            r.Importance = Importance;
            return r;
        }
    }


    void OnDrawGizmos()
    {
        Generators.Road.RadialOrthoBasis r = OrthoBasis;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(r.Center, 5.0f * r.Importance);
    }
}