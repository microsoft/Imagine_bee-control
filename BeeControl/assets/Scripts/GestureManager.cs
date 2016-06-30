using UnityEngine;
using System.Collections;

public class GestureManager : MonoBehaviour
{
	public Collider playPlane;	// The collision plane used to convert mouse position to world position.

	FlightPath currentPath;		// The current flight path we are appending waypoints to.
	float segmentLength = 1f;	// The length of each segment along the path.
	bool drag = false;			// Are we currently dragging.

	void Update()
	{
		// Check if we are currently dragging the mouse.
		if (!drag)
		{
			// We only enter drag mode during gameplay and when the player hold the left mouse button.
			if (GameplayManager.Instance.CanPlay() && Input.GetMouseButtonDown(0))
			{
				// *** Add your source code here ***
			}
		}
		else
		{
			// Check if we should stay in drag mode. We will exit if the bee has reached a honeycomb
			// or the end of its path.
			if (currentPath == null || currentPath.EndReached() || currentPath.Waypoints.Count == 0)
			{
				drag = false;
			}
			else
			{
				// Create a 3D ray pointing into the world and test against the play plane, which is
				// set at position with z equals 0.
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				playPlane.Raycast(ray, out hitInfo, 1000f);

				// Check if we have dragged far enough from our last waypoint.
				Vector3 currentPosition = hitInfo.point;
				Vector3 lastPosition = currentPath.GetLastPosition();
				if (Vector3.Distance(currentPosition, lastPosition) > segmentLength)
				{
					// Keep adding fixed-length segments up to the current position. This way the
					// dash-line for the path will be consistent instead of looking stretched.
					float dragLength = Vector3.Distance(currentPosition, lastPosition);
					Vector3 direction = Vector3.Normalize(currentPosition - lastPosition);
					float totalLength = segmentLength;
					while (totalLength < dragLength)
					{
						currentPath.Append(lastPosition + (direction * totalLength));
						totalLength += segmentLength;
					}
				}

				// Check if the player has stopped dragging.
				if (Input.GetMouseButtonUp(0))
				{
					// Try to snap the bee's path to the closest honeycomb.
					Beehive.Instance.SnapToHoneycomb(currentPath);

					// Exit drag mode.
					drag = false;
				}
			}
		}
	}
}