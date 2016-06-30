using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The bee manager is responsible for controlling the spawning of the various bee
/// types. The bee manager is a singleton and can be accessed in any script using the
/// BeeManager.Instance syntax.
/// </summary>
public class BeeManager : MonoBehaviour
{
	// The static singleton instance of the bee manager.
	public static BeeManager Instance { get; private set; }

	[System.Serializable]
	public class BeeType
	{
		public Material material;			// The material used for this bee type.
		public float moveSpeed;				// The move speed for this bee type.
		public int spawnWeight;				// Controls the chance of spawning this bee type over other types.
	};

	[System.Serializable]
	public class SpawnSettings
	{
		public float minSpawnDelay;			// Minimum wait time before spawning the next bee.
		public float maxSpawnDelay;			// Maximum wait time before spawning the next bee.
		public int beeCount;				// The number of bees to spawn using this setting.
	};

	public AudioClip buzzSoundLoop;			// Audio clip for the bee buzz sound.

	public BeeType[] beeTypes;				// An array of all bee types.
	public SpawnSettings[] spawnSettings;	// An array of spawn settings.

	float spawnDelay = 0f;					// The wait time before we spawn the next bee.
	float buzzVolumeMax = 0f;				// Bee buzz sound maximum volume.
	float buzzVolumeCurrent = 0f;			// Bee buzz sound current volume.
	int totalSpawnWeight = 0;				// Total spawn weight accumulated from all the bee types.
	int spawnIndex = 0;						// Index to the current spawn setting.
	int beeCount;							// Total number of bees spawned with the current setting.

	AudioSource buzzSoundSource;

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	void Start()
	{
		// Compute the total spawn weight.
		foreach (var type in beeTypes)
		{
			totalSpawnWeight += type.spawnWeight;
		}

		// Create audio sources for sound playback.
		buzzSoundSource = AudioHelper.CreateAudioSource(gameObject, buzzSoundLoop);

		// Kick off the buzz sound loop, but set volume to zero initially.
		buzzVolumeMax = buzzSoundSource.volume;
		buzzSoundSource.volume = buzzVolumeCurrent;
		buzzSoundSource.Play();
	}

	void Update()
	{
		// Check if we can start playing.
		if (GameplayManager.Instance.CanPlay())
		{
			// Update the spawn wait time and check if we are ready to spawn the next bee.
			spawnDelay -= Time.deltaTime;
			if (spawnDelay <= 0f)
			{
				// Get a new bee from the object pool.
				GameObject bee = ObjectPooler.Instance.GetPooledObject("Bee");
				bee.SetActive(true);

				// Randomly pick a type and position for this bee.
				int type = PickRandomBeeType();
				Vector3 position = PickRandomSpawnPosition();

				// Initialize the bee controller with the settings for this bee type.
				BeeController beeController = bee.GetComponent<BeeController>();
				beeController.Initialize(position, beeTypes[type].moveSpeed, beeTypes[type].material);

				// Show alert icon.
				UIManager.Instance.ShowAlertIcon(bee);

				// Increment bee count and check if we need to advance to the next spawn setting.
				beeCount++;
				if (beeCount >= spawnSettings[spawnIndex].beeCount)
				{
					// Set to the next spawn setting and reset the bee count.
					spawnIndex = Mathf.Min(spawnIndex + 1, spawnSettings.Length - 1);
					beeCount = 0;
				}

				// Reset the spawn wait time for the next bee.
				spawnDelay = Random.Range(spawnSettings[spawnIndex].minSpawnDelay, spawnSettings[spawnIndex].maxSpawnDelay);
			}
		}

		// Update buzz volume.
		UpdateBuzzVolume();
	}

	/// <summary>
	/// Pick a random bee type based on the weight of each category.
	/// </summary>
	/// <returns>The index to the select bee type.</returns>
	int PickRandomBeeType()
	{
		int random = Random.Range(0, totalSpawnWeight);
		int currentWeight = 0;
		for (int i = 0; i < beeTypes.Length; ++i)
		{
			currentWeight += beeTypes[i].spawnWeight;
			if (random <= currentWeight)
			{
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Pick a random spawn position just outside the screen viewport.
	/// </summary>
	/// <returns>The randomly selected position.</returns>
	Vector3 PickRandomSpawnPosition()
	{
		Vector3 spawnPosition = Vector3.zero;

		// We need to find a position that is suitable (not too close to other bees).
		bool positionFound = false;
		while (!positionFound)
		{
			// Randomly select a position in viewport space.
			Vector3 viewportPoint = Vector3.zero;
			viewportPoint.x = Random.Range(-0.7f, 1.7f);
			viewportPoint.y = Random.Range(-0.7f, 1.7f);

			// If x is within the viewable space, adjust y to the top or bottom of the screen.
			if (viewportPoint.x > 0.05f && viewportPoint.x < 1.05f)
			{
				viewportPoint.y = (viewportPoint.y > 0.5f) ? 1.1f : -0.1f;
			}

			// Transform position to world space with z at 0.
			Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
			Plane plane = new Plane(-Vector3.forward, 0f);
			float distance = 0f;
			plane.Raycast(ray, out distance);
			spawnPosition = ray.GetPoint(distance);

			// Make sure the position is not too close to other active bees.
			bool tooClose = false;
			foreach (BeeController bee in BeeController.ActiveList)
			{
				if (Vector3.Distance(bee.transform.position, spawnPosition) < 5f)
				{
					tooClose = true;
					break;
				}
			}
			positionFound = !tooClose;
		}

		return spawnPosition;
	}

	/// <summary>
	/// Controls the buzz sound loop volume.
	/// </summary>
	void UpdateBuzzVolume()
	{
		// Check if we have any bees active in the game.
		if (BeeController.ActiveList.Count > 0)
		{
			// Set target volume to the max.
			buzzVolumeCurrent = GameplayManager.Instance.CanPlay() ? buzzVolumeMax : buzzVolumeMax * 0.25f;
		}
		else
		{
			// Immediately shutdown the sound.
			buzzVolumeCurrent = 0f;
			buzzSoundSource.volume = 0f;
		}

		// Gradually adjust the volume.
		buzzSoundSource.volume = Mathf.Lerp(buzzSoundSource.volume, buzzVolumeCurrent, Time.deltaTime);
	}
}