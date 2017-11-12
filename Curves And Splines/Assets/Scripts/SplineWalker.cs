using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour {

    public BezierSpline spline;
    public float duration;
    private float progress;

	

	
	// Update is called once per frame
	void Update () {
        progress += Time.deltaTime / duration;
        if(progress > 1f)
        {
            progress = 1f;
        }
        transform.localPosition = spline.GetLerpPoint(progress);
	}
}
