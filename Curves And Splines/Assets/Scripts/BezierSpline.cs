using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BezierSpline : MonoBehaviour {

    public CurveVectors[] vectorChain;
  
    public void Reset()
    {
        vectorChain = new CurveVectors[]
        {new CurveVectors( new Vector3(1f,0f,0f), new Vector3(2f,0f,0f),
        new Vector3(3f,0f,0f),new Vector3(4f,0f,0f))};
    }

    public Vector3 GetPoint(float t)
    {
        //return transform.TransformPoint(Bezier.GetPoint(vectorChain[0], t));

        int i;
        if(t >= 1f)
        {
            t = 1f;
            i = vectorChain.Length - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * vectorChain.Length;
            i = (int)t;
            t -= i;
        }

        return transform.TransformPoint(Bezier.GetPoint(vectorChain[i], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = vectorChain.Length - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * vectorChain.Length;
            i = (int)t;
            t -= i;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(vectorChain[i], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve()
    {
        Vector3 point = vectorChain[vectorChain.Length - 1].p3;
        Array.Resize(ref vectorChain, vectorChain.Length + 1);
        vectorChain[vectorChain.Length - 1].p0 = point;
        point.x += 1f;
        vectorChain[vectorChain.Length - 1].p1 = point;
        point.x += 1f;
        vectorChain[vectorChain.Length - 1].p2 = point;
        point.x += 1f;
        vectorChain[vectorChain.Length - 1].p3 = point;
    }
}
