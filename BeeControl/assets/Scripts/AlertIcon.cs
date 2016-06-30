using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the alert icon for when a bee is off the screen.
/// </summary>
public class AlertIcon : MonoBehaviour
{
	public float screenOffset;		// Distance from the edge of the screen.

	BeeController beeController;	// Controller script for the bee we are tracking.
	Transform beeTransform;			// Transform component of the bee we are tracking.

	void Update()
	{
		// Check if the bee is still valid.
		if (beeTransform == null || beeController.IsStunned())
		{
			// Stop tracking if the bee is gone or is stunned.
			Destroy(gameObject);
			return;
		}

		// Check if the bee is visible yet.
		Vector3 viewportPosition = Camera.main.WorldToViewportPoint(beeTransform.position);
		if (viewportPosition.x >= 0f && viewportPosition.x <= 1f &&
			viewportPosition.y >= 0f && viewportPosition.y <= 1f)
		{
			// Bee is in view, we can destroy the icon now.
			Destroy(gameObject);
			return;
		}

		// Update icon screen position.
		Vector3 screenPosition = Camera.main.ViewportToScreenPoint(viewportPosition);
		screenPosition.x = Mathf.Clamp(screenPosition.x, screenOffset, Screen.width - screenOffset);
		screenPosition.y = Mathf.Clamp(screenPosition.y, screenOffset, Screen.height - screenOffset);
		transform.position = screenPosition;
	}

	/// <summary>
	/// Call this to set the bee this icon should be tracking.
	/// </summary>
	/// <param name="bee">Game object for the bee.</param>
	public void TrackBee(GameObject bee)
	{
		beeController = bee.GetComponent<BeeController>();
		beeTransform = bee.transform;
	}
}
