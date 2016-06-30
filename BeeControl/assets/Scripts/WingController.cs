using UnityEngine;
using System.Collections;

/// <summary>
/// Script to control the wing animations of the bee.
/// </summary>
public class WingController : MonoBehaviour
{
	public float range;		// The wing's rotation range.
	public float speed;		// The wing's rotation speed.

	void Update()
	{
		// Use a sinusoidal function to control the repeating wing animation.
		float angle = Mathf.Sin(Time.time * speed) * range;
		transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
	}
}