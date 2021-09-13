using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeakSpawner : MonoBehaviour
{

	public BezierSpline spline;

	public float duration;

	private float progress=0.01f;

	public bool lookForward;

	private void Update()
	{
		progress += Time.deltaTime / duration;
		if (progress > 1f)
		{
			progress = 1f;
		}
		Vector3 position = spline.GetPoint(progress);
		transform.localPosition = position;
		if (lookForward)
		{
			transform.LookAt(position + spline.GetDirection(progress));
		}
	}
}
