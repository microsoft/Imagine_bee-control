using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script for the flight path of a bee. The script keeps an array of waypoints along the bee's
/// path as well as the index of the bee's current waypoint. When the bee is close enough to the
/// current waypoint, the current index will be incremented so the bee will start heading to the
/// next waypoint. The script also tracks if the end of the path is connected to an open
/// honeycomb. Finally, the path is visualized using a line renderer component.
/// </summary>
public class FlightPath : MonoBehaviour
{
	public List<Vector3> Waypoints { get; set; }	// List of waypoint positions along the path.

	public float scrollSpeed = 1f;					// The scroll speed for the path texture.
	public Color connectedColor;					// The path color when connected to a honeycomb.
	public Color unconnectedColor;					// The path color when unconnected.

	Honeycomb destinationHoneycomb;					// The honeycomb at our destination.

	LineRenderer lineRenderer;						// The line renderer component for our path.
	Material lineMaterial;							// The material used by the line renderer.
	int currentIndex;								// The current waypoint index.

	void Start()
	{
		// Allocate a new list for our waypoints.
		Waypoints = new List<Vector3>();

		// Keep reference to our line renderer and the material.
		lineRenderer = GetComponent<LineRenderer>();
		lineMaterial = lineRenderer.material;
	}

	void Update()
	{
		// Check if we have two or more waypoints for rendering, we only want to show the path
		// from the bee to the end of the path.
		int numWaypointsToRender = Waypoints.Count - currentIndex;
		if (numWaypointsToRender >= 2)
		{
			// Enable the line renderer now that we have at least one point to draw.
			lineRenderer.enabled = true;

			// Set the vertices for the remaining path.
			lineRenderer.SetVertexCount(numWaypointsToRender);
			for (int i = 0; i < numWaypointsToRender; ++i)
			{
				lineRenderer.SetPosition(i, Waypoints[i + currentIndex]);
			}

			// Update the material scale and offset so the texture animates along the path direction.
			lineMaterial.mainTextureScale = new Vector2((float)numWaypointsToRender, 1f);
			lineMaterial.mainTextureOffset = new Vector2(-Time.time * scrollSpeed, 0f);
		}
	}

	/// <summary>
	/// Call this method to reset the path.
	/// </summary>
	public void Clear()
	{
		// Remove all the waypoints.
		Waypoints.Clear();

		// Disable the line renderer.
		lineRenderer.SetVertexCount(0);
		lineRenderer.enabled = false;

		// Reset the waypoint index to 0.
		currentIndex = 0;

		// Disconnect path from any destination honeycomb.
		Disconnect();
	}

	/// <summary>
	/// Appends a new waypoint to our path.
	/// </summary>
	/// <param name="position">Position for the new waypoint.</param>
	public void Append(Vector3 position)
	{
		Waypoints.Add(position);
	}

	/// <summary>
	/// Method to return the last waypoint of the path.
	/// </summary>
	/// <returns>The last position of the path.</returns>
	public Vector3 GetLastPosition()
	{
		// If our path is empty, just return (0,0,0).
		if (Waypoints.Count <= 0)
		{
			return Vector3.zero;
		}

		// Return position at index equal to Count - 1.
		return Waypoints[Waypoints.Count - 1];
	}

	/// <summary>
	/// Call this to connect the end of the path to the given honeycomb.
	/// </summary>
	/// <param name="honeycomb">The honeycomb to connect the path to.</param>
	public void ConnectToHoneycomb(Honeycomb honeycomb)
	{
		// Make sure we have a valid path.
		if (Waypoints.Count > 0)
		{
			// Performing a cast to Vector2 as we are only interested in the (x, y)
			// coordinates of the honeycomb.
			Waypoints[Waypoints.Count - 1] = (Vector2)honeycomb.transform.position;

			// Keep a reference to our destination.
			destinationHoneycomb = honeycomb;

			// Update the line renderer colors to connected.
			lineRenderer.SetColors(connectedColor, connectedColor);
		}
	}

	/// <summary>
	/// Call this to clear the path's destination and reset the path color.
	/// </summary>
	public void Disconnect()
	{
		destinationHoneycomb = null;
		lineRenderer.SetColors(unconnectedColor, unconnectedColor);
	}

	/// <summary>
	/// Retrieve the position of our next waypoint. If we are out of waypoints (i.e. the
	/// bee has reached the end of the path), this function returns false.
	/// </summary>
	/// <param name="waypoint">out Param for the next waypoint position.</param>
	/// <returns><c>true</c> if we have a valid waypoint; otherwise, <c>false</c>.</returns>
	public bool GetWaypoint(out Vector3 waypoint)
	{
		// Automatically advance our index up to the next waypoint.
		while (currentIndex < Waypoints.Count)
		{
			// Check if we have arrived at this waypoint.
			if (Vector3.Distance(transform.position, Waypoints[currentIndex]) > 0.1f)
			{
				// If waypoint is still up ahead, set position to this waypoint and return true.
				waypoint = Waypoints[currentIndex];
				return true;
			}

			// The current waypoint is too close, increment the index and try the next one ahead.
			++currentIndex;
		}

		// If we get here, we have no more waypoints ahead. We can safely clear our path.
		if (EndReached())
		{
			Clear();
		}

		// Set out waypoint position to (0,0,0) and return false.
		waypoint = Vector3.zero;
		return false;
	}

	/// <summary>
	/// Have we reached the end of our path?
	/// </summary>
	/// <returns><c>true</c> if we have reached the end; otherwise, <c>false</c>.</returns>
	public bool EndReached()
	{
		// We are at the end of our path only if we have a valid path and that we have
		// advanced our index up to the last waypoint. Note that reaching the end of the path
		// does not mean we have reached a honeycomb as the path might not be connected. In
		// other words, the end of a path is different than a destination.
		return (Waypoints.Count >= 2) && (Waypoints.Count == currentIndex);
	}

	/// <summary>
	/// Check if the bee has arrived at its destination.
	/// </summary>
	/// <returns><c>true</c> if we have reached the destination; otherwise, <c>false</c>.</returns>
	public bool HasArrived()
	{
		// We only need to check for arrival during gameplay.
		if (GameplayManager.Instance.CanPlay())
		{
			// Do we have a destination?
			if (destinationHoneycomb != null)
			{
				Vector2 current = transform.position;
				Vector2 goal = GetLastPosition();

				// A bee arrives if it reaches its goal.
				if (Vector2.Distance(current, goal) < 0.75f)
				{
					destinationHoneycomb.BeeEntered();

					// Notify the gameplay manager.
					GameplayManager.Instance.OnBeeArrived();
					return true;
				}
			}
		}
		return false;
	}
}
