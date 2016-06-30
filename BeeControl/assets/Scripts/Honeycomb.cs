using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script for controlling the honeycomb's current state and track the number of
/// bees entering the honeycomb.
/// </summary>
public class Honeycomb : MonoBehaviour
{
	public GameObject open;				// Game object showed when the honeycomb is opened.
	public GameObject closed;			// Game object showed when the honeycomb is closed.

	public Material normalMaterial;		// Material used when the honeycomb is opened.
	public Material closingMaterial;	// Material used when the honeycomb is almost full.

	public AudioClip landSound;			// Sound when the bee enters the honeycomb.

	List<FlightPath> paths;				// List of paths connected to this honeycomb.
	int beeCapacity = 0;				// The number of bees allowed to enter this honeycomb.

	AudioSource landSource;

	void Start()
	{
		// Create audio sources for sound playback.
		landSource = AudioHelper.CreateAudioSource(gameObject, landSound);
	}

	/// <summary>
	/// Call this when a bee enters the honeycomb.
	/// </summary>
	public void BeeEntered()
	{
		// Decrement the bee capacity.
		beeCapacity--;

		// Play landing sound.
		landSource.Play();

		// Check if the honeycomb is almost fully occupied.
		if (beeCapacity == 1)
		{
			// If so, switch to the closing material so the player can tell that this
			// honeycomb is almost full.
			open.GetComponentInChildren<Renderer>().material = closingMaterial;
		}
		// Check if the honeycomb is full.
		else if (beeCapacity == 0)
		{
			// We need to now close the honeycomb.
			Close();
		}
	}

	/// <summary>
	/// Call this to open the honeycomb. Bees can now enter this honeycomb.
	/// </summary>
	/// <param name="capacity">The number of bees allowed into this honeycomb.</param>
	public void Open(int capacity)
	{
		// Clear our list of registered path.
		paths = new List<FlightPath>();

		// Set the new bee capacity.
		beeCapacity = capacity;

		// Show the open state.
		open.SetActive(true);
		closed.SetActive(false);

		// Use the normal material for rendering.
		open.GetComponentInChildren<Renderer>().material = (beeCapacity == 1) ? closingMaterial : normalMaterial;
	}

	/// <summary>
	/// Call this to close the honeycomb. No more bees can enter after it is closed.
	/// </summary>
	public void Close()
	{
		// Disconnect all registered paths.
		foreach (FlightPath path in paths)
		{
			path.Disconnect();
		}
		paths.Clear();

		// Show the closed state.
		open.SetActive(false);
		closed.SetActive(true);

		// Notify the beehive that this honeycomb is now closed.
		Beehive.Instance.OnClose(this);
	}

	/// <summary>
	/// Keep a reference to the path in our list. This is used to update all connected paths
	/// when the honeycomb closes.
	/// </summary>
	/// <param name="path">The flight path to be tracked.</param>
	public void RegisterPath(FlightPath path)
	{
		paths.Add(path);
	}
}
