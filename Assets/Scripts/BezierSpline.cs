using UnityEngine;
using System;
using System.Collections.Generic;

//Only for mesh testing in editor
public class BezierCurveData
{
	public Vector3[] points;
	public bool generated;
}

[RequireComponent(typeof(MeshFilter))]
public class BezierSpline : MonoBehaviour
{

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	//Only for mesh testing in editor
	[SerializeField]
	public int resolution = 12;

	//Only for mesh testing in editor
	[SerializeField]
	public float thickness = 0.5f;

	//Only for mesh testing in editor
	public List<BezierCurveData> curveDatas = new List<BezierCurveData>();

	//If loop is selected, close the spine
	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			loop = value;
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

	public Vector3 GetControlPoint(int index)
	{
		return points[index];
	}

	public void SetControlPoint(int index, Vector3 point)
	{
		if (index % 3 == 0)
		{
			Vector3 delta = point - points[index];
			if (loop)
			{
				if (index == 0)
				{
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1)
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

	public BezierControlPointMode GetControlPointMode(int index)
	{
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode(int index, BezierControlPointMode mode)
	{
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop)
		{
			if (modeIndex == 0)
			{
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1)
			{
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode(int index)
	{
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
		{
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex)
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

	public int CurveCount
	{
		get
		{
			return (points.Length - 1) / 3;
		}
	}

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
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
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
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}

	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}

	public Vector3[] getPointList()
	{
		return points;
	}

	//For texture application in future update
	public float GetApproximateLength(int granularity, int curveIndex)
	{
		var length = 0f;
		var lastPoint = curveDatas[curveIndex].points[0];
		for (int i = 1; i <= granularity; i++)
		{
			var currentPoint = GetPoint(1f / granularity * i);
			length += Vector3.Distance(lastPoint, currentPoint);
			lastPoint = currentPoint;
		}
		return length;
	}

	//Add new curve in editor mode
	public void AddCurve()
	{
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 15f;
		point.y += 2f;
		point.z += 3f;
		points[points.Length - 3] = point;
		point.x += 15f;
		point.y += 20f;
		point.z += -10f;
		points[points.Length - 2] = point;
		point.x += 15f;
		point.y += -10f;
		point.z += 10f;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

	//Add new curve at runtime
	public void AddCurve(float pruned1, float pruned2, float pruned3)
	{
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 15f;
		point.y += 200f * pruned1;
		point.z = 200f * pruned1;
		points[points.Length - 3] = point;
		point.x += 15f;
		point.y += 200f * pruned2;
		point.z = 200f * pruned2;
		points[points.Length - 2] = point;
		point.x += 15f;
		point.y += 200f * pruned3;
		point.z = 200f * pruned3;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

	//Generate the first curve and force the mirrored mode
	public void Reset()
	{
		points = new Vector3[] {
			new Vector3(0f, 0f, 0f),
			new Vector3(10f, 0f, 0f),
			new Vector3(20f, 0f, 0f),
			new Vector3(30f, 0f, 0f)
		};

		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Mirrored,
			BezierControlPointMode.Mirrored
		};
	}
}