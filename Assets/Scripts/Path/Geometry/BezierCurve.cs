using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BezierCurve 
{
    // Start is called before the first frame update


    public static Vector3[] smoothLine(Vector3[] points)
    {
        if(points.Length < 3) { return points; }

        int nSmooth = 12;
        int gradeBezier = 3;

        int division = (int) Mathf.Floor(points.Length / gradeBezier);

        int difference = (int)points.Length - division * gradeBezier;

        Vector3[] smoothPoints = new Vector3[division * nSmooth + difference];

        int index = 0;
        for(int n = 0; n < division; n++)
        {

            for(int t=0; t< nSmooth; t++)
            {
                smoothPoints[index + t] = QuadraticCurve(points[n * gradeBezier], points[n * gradeBezier + 1], points[n * gradeBezier + 2], t/nSmooth);
            }
            index += nSmooth;
        }

        if(difference > 0) { for (int i = 0; i < difference; i++) { smoothPoints[index] = points[ (points.Length-1) - difference + i]; index++; } }
        

        return smoothPoints;
    }

    static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return a + (b - a) * t;
    }


    static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Lerp(a, b, t);
        Vector3 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);

    }


}
