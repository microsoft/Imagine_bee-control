using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script to control the beehive. The beehive is constructed using a two-dimensional array
/// of honeycomb objects. The honeycombs are initially hidden and slowly being revealed as
/// the player progresses in game.
/// </summary>
public class Beehive : MonoBehaviour
{
	// The static singleton instance of the beehive.
	public static Beehive Instance { get; private set; }
	
	[System.Serializable]
	public class Settings
	{
		public int hiveGrowSize;		// How much is the beehive growing by?
		public int openCount;			// How many honeycombs should be opened for bees to enter?
	};

	public GameObject honeycomb;		// The honeycomb prefab.

	public int numColumns;				// Number of columns for the beehive.
	public int numRows;					// Number of rows for the beehive.
	public float honeycombSize;			// How big is each honeycomb (we assume the honeycomb to have the same width and height)?

	public int startRadius;				// The initial size of the beehive.
	public int beesPerHoneycomb;		// How many bees can a honeycomb fit?

	public AudioClip hiveGrowSound;		// Audio clip when the beehive grows.

	public Settings[] settings;			// Settings that control the growth of the beehive.

	GameObject[,] honeycombGrid;								// A 2D array of honeycombs.
	List<GameObject> openHoneycombs = new List<GameObject>();	// A list of honeycombs that are currently opened.

	int settingIndex = 0;				// Index to the current setting.
	int numHoneycombsActive;			// Number of honeycombs revealed.

	AudioSource hiveGrowSource;

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;

		// Create audio sources for sound playback.
		hiveGrowSource = AudioHelper.CreateAudioSource(gameObject, hiveGrowSound);
	}

	void Start()
	{
		// *** Add your source code here ***
	}

	void Update()
	{
		// Check if we can start playing.
		if (GameplayManager.Instance.CanPlay())
		{
			// *** Add your source code here ***
		}
	}

	/// <summary>
	/// Call this to expand the beehive randomly by a certain number of honeycombs.
	/// </summary>
	/// <param name="count">Number of honeycombs to expand by.</param>
	void ExpandHive(int count)
	{
		// Make sure we don't try to reveal more than we actually have.
		count = Mathf.Min(count, (numRows * numColumns) - numHoneycombsActive);

		int activated = 0;
		while (activated < count)
		{
			// Randomly pick an inactive honeycomb.
			int column = Random.Range(0, numColumns);
			int row = Random.Range(0, numRows);

			// We need to make sure we pick honeycombs that have active neighbors or
			// else the beehive will look disconnected.
			if (!IsHoneycombActive(row, column) && HasActiveNeighbors(row, column))
			{
				honeycombGrid[row, column].SetActive(true);
				numHoneycombsActive++;
				activated++;
			}
		}

		// Play expand sound.
		hiveGrowSource.Play();
	}

	/// <summary>
	/// Call this to open up some honeycombs for the bees to enter.
	/// </summary>
	/// <param name="count">Number of honeycombs to open.</param>
	void Open(int count)
	{
		// Make sure we don't try to open more than we have revealed.
		count = Mathf.Min(count, numHoneycombsActive - openHoneycombs.Count);
		
		int opened = 0;
		while (opened < count)
		{
			// Randomly pick a revealed honeycomb.
			int column = Random.Range(0, numColumns);
			int row = Random.Range(0, numRows);

			// Make sure the selected honeycomb is active and hasn't been opened yet.
			if (IsHoneycombActive(row, column) && !IsHoneycombOpened(row, column))
			{
				honeycombGrid[row, column].GetComponent<Honeycomb>().Open(beesPerHoneycomb);
				openHoneycombs.Add(honeycombGrid[row, column]);
				opened++;
			}
		}
	}

	/// <summary>
	/// Is this honeycomb active/revealed?
	/// </summary>
	/// <param name="row">The row of the honeycomb.</param>
	/// <param name="column">The column of the honeycomb.</param>
	/// <returns><c>true</c> if the honeycomb is active; otherwise, <c>false</c>.</returns>
	bool IsHoneycombActive(int row, int column)
	{
		return column >= 0 &&
			   column < numColumns &&
			   row >= 0 &&
			   row < numRows &&
			   honeycombGrid[row, column].activeInHierarchy;
	}

	/// <summary>
	/// Is this honeycomb opened?
	/// </summary>
	/// <param name="row">The row of the honeycomb.</param>
	/// <param name="column">The column of the honeycomb.</param>
	/// <returns><c>true</c> if the honeycomb is opened; otherwise, <c>false</c>.</returns>
	bool IsHoneycombOpened(int row, int column)
	{
		return column >= 0 &&
			   column < numColumns &&
			   row >= 0 &&
			   row < numRows &&
			   openHoneycombs.Contains(honeycombGrid[row, column]);
	}

	/// <summary>
	/// Does the honeycomb at location (row, column) have any active neighbors?
	/// </summary>
	/// <param name="row">The row of the honeycomb.</param>
	/// <param name="column">The column of the honeycomb.</param>
	/// <returns><c>true</c> if the honeycomb has active neighbors; otherwise, <c>false</c>.</returns>
	bool HasActiveNeighbors(int row, int column)
	{
		// First check neighbors directly above, below, or to the sides.
		if (IsHoneycombActive(row    , column - 1) ||
			IsHoneycombActive(row    , column + 1) ||
			IsHoneycombActive(row - 1, column    ) ||
			IsHoneycombActive(row + 1, column    ))
		{
			return true;
		}

		// Check if we are on an even row.
		if (row % 2 == 0)
		{
			// We are on an even row (these are shifted to the right by half).
			return IsHoneycombActive(row - 1, column + 1) || IsHoneycombActive(row + 1, column + 1);
		}
		else
		{
			// We are on an odd row (these are not shifted).
			return IsHoneycombActive(row - 1, column - 1) || IsHoneycombActive(row + 1, column - 1);
		}
	}

	/// <summary>
	/// Call this to snap the end of a flight path to an opened honeycomb. This method would
	/// only perform the snapping if there is an opened honeycomb that is close enough.
	/// </summary>
	/// <param name="path">Flight path for a given bee.</param>
	public void SnapToHoneycomb(FlightPath path)
	{
		// Get the end point of the path.
		Vector2 current = path.GetLastPosition();

		// Go through and find an open honeycomb that is close by the end point.
		foreach (GameObject honeycombObject in openHoneycombs)
		{
			Vector2 testPos = honeycombObject.transform.position;
			if (Vector2.Distance(current, testPos) < honeycombSize * 0.55f)
			{
				Honeycomb honeycomb = honeycombObject.GetComponent<Honeycomb>();

				// Snap the path to the honeycomb.
				path.ConnectToHoneycomb(honeycomb);

				// Register this path with the honeycomb, this is so that we can update
				// the path's color if the honeycomb is closed before the bee reaches it.
				honeycomb.RegisterPath(path);
				break;
			}
		}
	}

	/// <summary>
	/// Call this to remove a honeycomb from the opened list.
	/// </summary>
	/// <param name="honeycomb">Honeycomb to be closed.</param>
	public void OnClose(Honeycomb honeycomb)
	{
		openHoneycombs.Remove(honeycomb.gameObject);
	}
}