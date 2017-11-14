using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SplineWalker : MonoBehaviour {

    public BezierSpline spline;
    public float duration;
    public float progress;
    public float speed;

	

	

    
	void Update () {
        //progress += Time.deltaTime * 0.2f;
        progress += Time.deltaTime * speed;
        Vector3 x = spline.GetDistancePoint(progress);
        //Vector3 y = spline.GetLerpPoint(progress);
        //float d = Vector3.Distance(transform.localPosition, y);
        transform.localPosition = x;
	}
}
