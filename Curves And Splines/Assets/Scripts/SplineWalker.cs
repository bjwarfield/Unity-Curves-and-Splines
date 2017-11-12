using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SplineWalker : MonoBehaviour {

    public BezierSpline spline;
    public float duration;
    private float progress;
    public float speed;

	

	

    
	void Update () {
        progress += Time.deltaTime * speed;

        transform.localPosition = spline.GetDistancePoint(progress);
	}
}
