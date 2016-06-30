using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script to control the bee's movement and collision. Each bee has a flight path that controls
/// its velocity. If the flight path is empty, the bee will keep its course until it hits the
/// edge of the screen. This script will update the velocity when that happens to keep the bee
/// within the screen border. A bee can also be stunned when it collides with another bee.
/// </summary>
public class BeeController : MonoBehaviour
{
	// Global list of active bees.
	public static List<BeeController> ActiveList = new List<BeeController>();

	public AudioClip stunnedSound;	// Audio clip when the bee is stunned.

	FlightPath flightPath;			// Reference to the bee's flight path.

	Collider[] colliders;			// An array of colliders for the bee.
	Vector3 velocity;				// The bee's current velocity.
	float moveSpeed;				// The bee's move speed.
	float stunnedLerp;				// A lerp value to control the bee's stun animation.
	bool stunned;					// Is the bee currently stunned?

	AudioSource stunnedSource;

	void Awake()
	{
		// Keep references to the bee's flight path and colliders.
		flightPath = GetComponentInChildren<FlightPath>();
		colliders = GetComponentsInChildren<Collider>();

		// Create audio sources for sound playback.
		stunnedSource = AudioHelper.CreateAudioSource(gameObject, stunnedSound);
	}

	void OnDestroy()
	{
		// Remove ourself from the global list.
		ActiveList.Remove(this);
	}

	void Update()
	{
		// Check if the bee is currently stunned.
		if (stunned)
		{
			// A stunned bee will shrink and spiral down.
			stunnedLerp = Mathf.Lerp(stunnedLerp, 0f, Time.deltaTime);
			transform.Rotate(new Vector3(0f, 0f, 360f * Time.deltaTime));
			transform.localScale = Vector3.one * stunnedLerp;

			// Check if we are done with the stunned animation.
			if (stunnedLerp < 0.1f)
			{
				// Deactivate the bee.
				Terminate();
			}
		}
		else
		{
			// *** Add your source code here ***
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// We are only concerned with collision during gameplay.
		if (GameplayManager.Instance.CanPlay() && !stunned)
		{
			// Check if we have collided with another bee.
			Rigidbody rb = other.attachedRigidbody;
			if (rb != null && rb.CompareTag("Bee"))
			{
				// Stun both bees.
				Stun();
				rb.GetComponent<BeeController>().Stun();

				// Play the stunned sound.
				stunnedSource.Play();

				// Notify the gameplay manager.
				GameplayManager.Instance.OnBeeCollision();
			}
		}
	}

	/// <summary>
	/// Call this method each update to make sure the bee doesn't fly off the screen borders.
	/// </summary>
	void KeepBeeWithinViewport()
	{
		// Get the bee's current and future positions (we are projecting two update frames ahead).
		Vector3 currentPosition = Camera.main.WorldToViewportPoint(transform.position);
		Vector3 nextPosition = Camera.main.WorldToViewportPoint(transform.position + (velocity * Time.deltaTime * 2));

		// Check if the bee is heading off screen in the next few frames.
		Rect viewport = new Rect(0f, 0f, 1f, 1f);
		if (viewport.Contains(currentPosition) && !viewport.Contains(nextPosition))
		{
			// If the bee has reached the edge of the screen, we need to flip its velocity.
			if (nextPosition.x <= 0f || nextPosition.x >= 1f)
			{
				velocity.x *= -1f;
			}
			if (nextPosition.y <= 0f || nextPosition.y >= 1f)
			{
				velocity.y *= -1f;
			}

			// Stop following the flight path.
			flightPath.Clear();
		}
	}

	/// <summary>
	/// Since we are re-using bee game objects in our object pool, we can call
	/// this method to re-initial the bee.
	/// </summary>
	/// <param name="position">The bee's start position.</param>
	/// <param name="speed">The bee's move speed.</param>
	/// <param name="material">The bee's render material.</param>
	public void Initialize(Vector3 position, float speed, Material material)
	{
		// Update the bee's position and orientation.
		transform.position = position;
		transform.rotation = Quaternion.LookRotation(Vector3.forward, -position);
		transform.localScale = Vector3.one;

		// Set the bee's velocity.
		moveSpeed = speed;
		velocity = -position.normalized * moveSpeed;

		// Reset stun.
		stunned = false;

		// Apply the material to the bee's renderer.
		GetComponentInChildren<MeshRenderer>().material = material;

		// Enable all colliders.
		foreach (Collider c in colliders)
		{
			c.enabled = true;
		}

		// Add self to the global list.
		ActiveList.Add(this);
	}

	/// <summary>
	/// Call this to deactivate the bee and return it back to the object pool.
	/// </summary>
	public void Terminate()
	{
		// Clear our path.
		flightPath.Clear();

		// Remove ourself from the global list.
		ActiveList.Remove(this);

		// Return the bee back to the object pool.
		ObjectPooler.Instance.ReturnPooledObject(gameObject);
	}

	/// <summary>
	/// Call this to put the bee in the stunned state.
	/// </summary>
	public void Stun()
	{
		// Check if the bee is already stunned.
		if (!stunned)
		{
			// Clear the bee's flight path.
			flightPath.Clear();

			// Initialize stun parameters.
			stunnedLerp = 1f;
			stunned = true;

			// Disable all colliders.
			foreach (Collider c in colliders)
			{
				c.enabled = false;
			}
		}
	}

	/// <summary>
	/// Is the bee currently stunned?
	/// </summary>
	/// <returns><c>true</c> if the bee is stunned; otherwise, <c>false</c>.</returns>
	public bool IsStunned()
	{
		return stunned;
	}
}
