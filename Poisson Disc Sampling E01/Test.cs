using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Vector2 RadiusRange = new Vector2(1, 5);
    public bool IsUsingStaticRadius = true;

    /// <summary>
    /// The range we'll actually be testing against
    /// </summary>
    public float radius = 1;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSamples = 30;

    /// <summary>
    /// The radius that will be displayed in the final graph
    /// </summary>
    public float displayRadius = 1;

    List<Vector2> points;
    List<Vector3> pointsPlusRadius;

    void OnValidate()
    {
        if(IsUsingStaticRadius)
        {
            points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
        }
        else
        {
            pointsPlusRadius = PoissonDiscSampling.GeneratePointsOfDifferentSize(RadiusRange, regionSize, rejectionSamples);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        if(IsUsingStaticRadius)
        {
            if (points != null)
            {
                foreach (Vector2 point in points)
                {
                    Gizmos.DrawWireSphere(point, displayRadius);
                }
            }
        }
        else
        {
            if(pointsPlusRadius != null)
            {
                foreach(var point in pointsPlusRadius)
                {
                    Gizmos.DrawWireSphere(new Vector2(point.x, point.y), point.z * displayRadius);
                }
            }
        }
    }
}
