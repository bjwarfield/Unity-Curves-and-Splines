using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//struct to hold the vector set for each segment in a given curve
[System.Serializable]
public struct CurveVectors
{
    public Vector3 p0, p1, p2, p3;

    public CurveVectors(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }

    public CurveVectors(CurveVectors vectors)
    {
        this.p0 = vectors.p0;
        this.p1 = vectors.p1;
        this.p2 = vectors.p2;
        this.p3 = vectors.p3;
    }

    public static bool operator ==(CurveVectors c1, CurveVectors c2)
    {
        return (c1.p0.Equals(c2.p0) &&
                c1.p1.Equals(c2.p1) &&
                c1.p2.Equals(c2.p2) &&
                c1.p3.Equals(c2.p3));
    }

    public static bool operator !=(CurveVectors c1, CurveVectors c2)
    {
        return !(c1.p0.Equals(c2.p0) &&
                c1.p1.Equals(c2.p1) &&
                c1.p2.Equals(c2.p2) &&
                c1.p3.Equals(c2.p3));
    }


    // override object.Equals
    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return this == (CurveVectors)obj;
    }



    // override object.GetHashCode
    public override int GetHashCode()
    {
        //return base.GetHashCode();
        return 0;
    }

    //index operator override
    public Vector3 this[int key]
    {
        
        get
        {
            switch(key)
            {
                case 0:
                    return this.p0;
                case 1:
                    return this.p1;
                case 2:
                    return this.p2;
                case 3:
                    return this.p3;
                //default: return Vector3.zero;
                default: throw new System.ArgumentOutOfRangeException("Index for curve vector out of range.");


            }
        }
        set
        {
            switch (key)
            {
                case 0:
                    this.p0 = value; break;
                case 1:
                    this.p1 = value; break;
                case 2:
                    this.p2 = value; break;
                case 3:
                    this.p3 = value; break;
                default: throw new System.ArgumentOutOfRangeException("Index for curve vector out of range.");

            }

        }
    }

}
public static class Bezier
{


    public static Vector3 CalculatePoint(CurveVectors vectors, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        
        //return cubic bezier formula
        return
            oneMinusT * oneMinusT * oneMinusT * vectors.p0 +
            3f * oneMinusT * oneMinusT * t * vectors.p1 +
            3f * oneMinusT * t * t * vectors.p2 +
            t * t * t * vectors.p3;
    }

    public static Vector3 GetFirstDerivative (CurveVectors vectors, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        return
            3f * oneMinusT * oneMinusT * (vectors.p1 - vectors.p0) +
            6f * oneMinusT * t * (vectors.p2 - vectors.p1) +
            3f * t * t * (vectors.p3 - vectors.p2);
    }

    public static Vector3 CalculatePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

}

public class BezierCurve : MonoBehaviour {

    public CurveVectors vectors;

    public void Reset()
    {
        vectors = new CurveVectors
        (
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        );
    }

    public Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(Bezier.CalculatePoint(vectors, t));
    }

    public Vector3 GetVelocity(float t)
    {
        return transform.TransformPoint(Bezier.GetFirstDerivative(vectors, t)) -     transform.position;
    }

    public Vector3 GetDirection (float t)
    {
        return GetVelocity(t).normalized;
    }
}
