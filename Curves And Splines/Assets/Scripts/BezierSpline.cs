using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored
}
public class BezierSpline : MonoBehaviour {

    //vector point array
    [SerializeField]
    private Vector3[] points;
    //parallel point mode array
    [SerializeField]
    private BezierControlPointMode[] modes;



    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }


    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        points[index] = point;
    }

    //number of curve segment in spline
    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public void Reset()
    {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
    }

    //return vector of curve at interpolation point 0 < t < 1
    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.CalculatePoint(
            points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }


    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(
            points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }



    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        //resise pointmose array
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
    }
}
