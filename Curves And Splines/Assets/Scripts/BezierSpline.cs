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

    [SerializeField]
    private bool closedLoop;

    public bool ClosedLoop
    {
        get
        {
            return closedLoop;
        }

        set
        {
            closedLoop = value;
            if (value == true)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }


    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
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
        //vectors for single curve
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };

        //pointmode for each end
        modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }

    //return vector of curve at interpolation point 0 < t < 1
    public Vector3 GetLerpPoint(float t)
    {
        int i;
        if (ClosedLoop)
        {
            t = t % 1f;
        }

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

    private const int linesPerCurve = 16;
    private float[] GetLengths()
    {
        //create list of lengths for each line segment in spline
        float[] lengths = new float[CurveCount * (linesPerCurve) ];
        //distance trackers
        float totalDistance = 0;
        float distance = 0;

        //start at first segment, proceed to second to last segment
        for(int i = 0; i < CurveCount; i++)
        {
            Vector3 p0 = points[3 * i];
            Vector3 p1 = points[3 * i + 1];
            Vector3 p2 = points[3 * i + 2];
            Vector3 p3 = points[3 * i + 3];
            for (int j = 0; j < linesPerCurve; j++)
            {
                //set coresponding array element to the distance
                lengths[(i * linesPerCurve) + j] = totalDistance;
                
                //calculate the next distance
                distance = Vector3.Distance(Bezier.CalculatePoint(p0, p1, p2, p3, (float)j / (float) linesPerCurve), Bezier.CalculatePoint(p0, p1, p2, p3, (float)(j+1) / (float)linesPerCurve));
                totalDistance += distance;
            }
            
        }

        //put the final total in the last element of the array
        lengths[lengths.Length - 1] = totalDistance;
        return lengths;
    }

    public Vector3 GetDistancePoint(float distance)
    {
        //collect distances of spline segments
        float[] lengths = GetLengths();
        //get the total distace of the spline
        float totalDistance = lengths[lengths.Length - 1];

        //negative distance returns a point on a tangent to the first line segment scales to the negative distance
        if(distance < 0)
        {
            return transform.TransformPoint(Bezier.CalculatePoint(
            points[0], points[1], points[2], points[3], 0)) + GetDirection(0) * (distance - totalDistance);
        }

        //distance greater than the total distance returns a point on a tangent to the last line segment scaled to the overshot distance
        if (distance > totalDistance)
        {
            return transform.TransformPoint(Bezier.CalculatePoint(
            points[points.Length - 4],
            points[points.Length - 3], 
            points[points.Length - 2], 
            points[points.Length - 1], 1)) + GetDirection(1f) * (distance - totalDistance);
        }

        //find the line segment
        int index = 0;
        while (index < lengths.Length - 1 && lengths[index + 1] < distance) index++;

        float lerpScale = Mathf.InverseLerp(lengths[index], lengths[index + 1], distance);
        int curveIndex = index / linesPerCurve;

        Vector3 p0 = points[3 * curveIndex];
        Vector3 p1 = points[3 * curveIndex + 1];
        Vector3 p2 = points[3 * curveIndex + 2];
        Vector3 p3 = points[3 * curveIndex + 3];

        return transform.TransformPoint(
            Vector3.Lerp(Bezier.CalculatePoint(p0, p1, p2, p3, (float)(index % linesPerCurve) / (float)linesPerCurve),
            Bezier.CalculatePoint(p0, p1, p2, p3, (float)((index % linesPerCurve) +1) / (float)linesPerCurve),
            lerpScale)
            );
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
        EnforceMode(points.Length - 4);


        if (closedLoop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public BezierControlPointMode GetControlPointMode (int index)
    {
        return modes[(index + 1) / 3];
        
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];

            if (closedLoop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length -1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) /3;
        modes[modeIndex] = mode;

        if(closedLoop)
        {
            if(modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if(modeIndex == modes.Length -1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;

        //Debug.Log("Array size: " + modes.Length + " Index: " + modeIndex);
        BezierControlPointMode mode = modes[modeIndex];
        
        //skip if free, first or last node
        if(mode == BezierControlPointMode.Free || !closedLoop && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if(index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
        

    }
}
