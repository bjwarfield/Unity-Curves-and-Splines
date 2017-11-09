using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor {


    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    //curve granularity, # of steps per curve
    private const int lineSteps = 16;
    private const float directionScale = 1f;

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;


        for (int i = 0; i < spline.vectorChain.Length; i++)
        {
            CurveVectors points = ShowPoint(ref spline.vectorChain[i], i);

            Handles.color = Color.gray;
            Handles.DrawLine(points.p0, points.p1);
            Handles.DrawLine(points.p2, points.p3);

            Handles.DrawBezier(points.p0, points.p3, points.p1, points.p2, Color.white, null, 2f);

        }

        ShowDirections();

    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
        for (int i = 1; i <= lineSteps; i++)
        {
            point = spline.GetPoint(i / (float)lineSteps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)lineSteps));
        }


    }


    //draw handles at given points in the curve
    private CurveVectors ShowPoint(ref CurveVectors vector, int index)
    {

        CurveVectors points = new CurveVectors();

        for(int i = 0 ; i < 4; i++)
        {
            points[i] = handleTransform.TransformPoint(vector[i]);
            EditorGUI.BeginChangeCheck();
            if(i == 0 && index > 0)
            {
                points[i] = handleTransform.TransformPoint(spline.vectorChain[index - 1].p3);
                spline.vectorChain[index].p0 = spline.vectorChain[index - 1].p3;
            }
            else
            {
                points[i] = Handles.DoPositionHandle(points[i], handleRotation);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                vector[i] = handleTransform.InverseTransformPoint(points[i]);
                
            }
        }
        return points;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        spline = target as BezierSpline;
        if(GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }
}
