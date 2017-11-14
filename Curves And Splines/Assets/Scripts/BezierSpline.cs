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
    private Vector3[] controlPoints;
    [SerializeField]
    private Vector3[] curvePoints;
    private float[] lengths;
    private bool doRecalc = true;
    private const int linesPerCurve = 16;
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
                SetControlPoint(0, controlPoints[0]);
            }
        }
    }


    public int ControlPointCount
    {
        get
        {
            return controlPoints.Length;
        }
    }




    //number of curve segment in spline
    public int CurveCount
    {
        get
        {
            return (controlPoints.Length - 1) / 3;
        }
    }

    public void Reset()
    {
        //vectors for single curve
        controlPoints = new Vector3[] {
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
        curvePoints = new Vector3[(CurveCount * linesPerCurve)+1];
        lengths = new float[curvePoints.Length];
        Debug.Log(curvePoints.Length);

        doRecalc = true;
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
            i = controlPoints.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }


        return transform.TransformPoint(Bezier.CalculatePoint(
            controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3], t));
    }

    private void RecalculateCurvePoints()
    {
        for(int i = 0; i < curvePoints.Length; i ++)
        {
            curvePoints[i] = GetLerpPoint((float)i / (float)(CurveCount * linesPerCurve));
        }
    }
   
    private void GetLengths()
    {
        RecalculateCurvePoints();
        //create list of lengths for each line segment in spline
        lengths = new float[curvePoints.Length];
        //distance trackers
        float totalDistance = 0;
        float distance = 0;

        //start at first segment, proceed to second to last segment
        for(int i = 0; i < curvePoints.Length -1; i++)
        {
                lengths[i] = totalDistance;
                
                //calculate the next distance
                distance = Vector3.Distance(curvePoints[i], curvePoints[i+1]);
                totalDistance += distance;
        }

        //put the final total in the last element of the array
        lengths[lengths.Length - 1] = totalDistance;

    }

    public Vector3 GetDistancePoint(float distance)
    {

        if (doRecalc)
        { 
            //collect distances of spline segments
            GetLengths();
        }
        //get the total distace of the spline
        float totalDistance = lengths[lengths.Length - 1];

        if(closedLoop)
        {
            distance = distance % totalDistance;
        }

        //negative distance returns a point on a tangent to the first line segment scales to the negative distance
        if (distance < 0)
        {
            return GetLerpPoint(0f) + GetDirection(0f) * (distance);
        }

        
        //distance greater than the total distance returns a point on a tangent to the last line segment scaled to the overshot distance
        if (distance > totalDistance)
        {
            
            return GetLerpPoint(1f) + GetDirection(1f) * (distance - totalDistance);
        }

        //find the line segment
        int index = 0;
        while (index < lengths.Length - 2 && lengths[index + 1] < distance) index++;

        float lerpScale = Mathf.InverseLerp(lengths[index], lengths[index + 1], distance);

        return Vector3.Lerp(curvePoints[index], curvePoints[index + 1], lerpScale);
        //return GetLerpPoint(((float)index / (float)(curvePoints.Length))+ (lerpScale/(float)linesPerCurve));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = controlPoints.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(
            controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }



    public void AddCurve()
    {
        Vector3 point = controlPoints[controlPoints.Length - 1];
        Array.Resize(ref controlPoints, controlPoints.Length + 3);
        point.x += 1f;
        controlPoints[controlPoints.Length - 3] = point;
        point.x += 1f;
        controlPoints[controlPoints.Length - 2] = point;
        point.x += 1f;
        controlPoints[controlPoints.Length - 1] = point;

        //resise pointmode array
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(controlPoints.Length - 4);



        if (closedLoop)
        {
            controlPoints[controlPoints.Length - 1] = controlPoints[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }

        Array.Resize(ref curvePoints, (CurveCount * linesPerCurve) +1);
        Array.Resize(ref lengths, curvePoints.Length);

        doRecalc = true;
    }

    public void RemoveCurve()
    {
        if(CurveCount <= 1)
        {
            return;
        }

        Array.Resize(ref controlPoints, controlPoints.Length - 3);
        //resize pointmode array
        Array.Resize(ref modes, modes.Length - 1);

        if (closedLoop)
        {
            controlPoints[controlPoints.Length - 1] = controlPoints[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
        Array.Resize(ref curvePoints, (CurveCount * linesPerCurve)+1);
        Array.Resize(ref lengths, curvePoints.Length);
        doRecalc = true;
    }


    public BezierControlPointMode GetControlPointMode (int index)
    {
        return modes[(index + 1) / 3];
        
    }

    public Vector3 GetControlPoint(int index)
    {
        return controlPoints[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        doRecalc = true;
        if (index % 3 == 0)
        {
            Vector3 delta = point - controlPoints[index];

            if (closedLoop)
            {
                if (index == 0)
                {
                    controlPoints[1] += delta;
                    controlPoints[controlPoints.Length - 2] += delta;
                    controlPoints[controlPoints.Length - 1] = point;
                }
                else if (index == controlPoints.Length -1)
                {
                    controlPoints[0] = point;
                    controlPoints[1] += delta;
                    controlPoints[index - 1] += delta;
                }
                else
                {
                    controlPoints[index - 1] += delta;
                    controlPoints[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    controlPoints[index - 1] += delta;
                }
                if (index + 1 < controlPoints.Length)
                {
                    controlPoints[index + 1] += delta;
                }
            }
        }

        controlPoints[index] = point;
        EnforceMode(index);


    }
    
    public float GetTotalLength()
    {
        if (doRecalc)
        {
            GetLengths();
        }
        return lengths[lengths.Length - 1];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        doRecalc = true;
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
        doRecalc = true;
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
                fixedIndex = controlPoints.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= controlPoints.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= controlPoints.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = controlPoints.Length - 2;
            }
        }

        Vector3 middle = controlPoints[middleIndex];
        Vector3 enforcedTangent = middle - controlPoints[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, controlPoints[enforcedIndex]);
        }
        controlPoints[enforcedIndex] = middle + enforcedTangent;
        

    }
}
