using UnityEngine;
using System.Collections;

/// <summary>
/// The level manager is responsible for defining the player ranking. The level manager is a
/// singleton and can be accessed in any script using the LevelManager.Instance syntax.
/// </summary>
public class LevelManager : MonoBehaviour
{
	// The static singleton instance of the level manager.
	public static LevelManager Instance { get; private set; }

	public int goldRequirement;		// Requirement for Gold rank.
	public int silverRequirement;	// Requirement for Silver rank.
	public int bronzeRequirement;	// Requirement for Bronze rank.

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	/// <summary>
	/// Gets the rank for the player given the score.
	/// </summary>
	/// <returns>The player rank.</returns>
	/// <param name="score">The player's score.</param>
	public PlayerRank GetRank(int score)
	{
		// Set default to unranked.
		PlayerRank rank = PlayerRank.Unranked;

		// Check requirement for Gold rank.
		if (score >= goldRequirement)
		{
			rank = PlayerRank.Gold;
		}
		// Check requirement for Silver rank.
		else if (score >= silverRequirement)
		{
			rank = PlayerRank.Silver;
		}
		// Check requirement for Bronze rank.
		else if (score >= bronzeRequirement)
		{
			rank = PlayerRank.Bronze;
		}
		return rank;
	}
}