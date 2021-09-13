using UnityEngine;

public static class Bezier {
	
	//Quadratic bezier position
	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * p0 +
			2f * oneMinusT * t * p1 +
			t * t * p2;
	}

	//Quadratic bezier direction
	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
		return
			2f * (1f - t) * (p1 - p0) +
			2f * t * (p2 - p1);
	}

	//Cubic bezier position
	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float OneMinusT = 1f - t;
		return
			OneMinusT * OneMinusT * OneMinusT * p0 +
			3f * OneMinusT * OneMinusT * t * p1 +
			3f * OneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	//Cubic bezier direction
	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
			6f * oneMinusT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}

	//same thing as derivative, only using Lerp
	/*public static Vector3 GetTangent (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
		Vector3 a = Vector3.Lerp(p0, p1, t);
		Vector3 b = Vector3.Lerp(p1, p2, t);
		Vector3 c = Vector3.Lerp(p2, p3, t);

		Vector3 d = Vector3.Lerp(a, b, t);
		Vector3 e = Vector3.Lerp(b, c, t);

		return (e - d).normalized;
	}*/
}