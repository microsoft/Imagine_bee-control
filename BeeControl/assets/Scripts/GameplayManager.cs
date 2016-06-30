using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// The gameplay manager is responsible for controlling the overall flow of the game. The
/// game is divided into three main states: Tutorial, InGame, and GameOver. The user interface
/// and input controls are different depending on the current game state. The gameplay
/// manager tracks the player progress and switches between the game states based on
/// the results as well as the user input. The gameplay manager is a singleton and can be
/// accessed in any script using the GameplayManager.Instance syntax.
/// </summary>
public class GameplayManager : MonoBehaviour
{
	// The static singleton instance of the gameplay manager.
	public static GameplayManager Instance { get; private set; }

	public int maxLives;				// Total number of collisions before the game end.

	// Enumeration for the different game states. The default starting
	// state is the tutorial.
	enum GameState
	{
		Tutorial,						// Show player the game instructions.
		InGame,							// Player can start controlling the robot.
		GameOver,						// Game ended, player input is blocked.
	};
	GameState state = GameState.Tutorial;
	
	PlayerRank rank;					// The rank of the player for the current level.
	int score;							// The player's score.
	int lives;							// The player's lives.

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	void Start()
	{
		// Reset the player's progress.
		lives = maxLives;
		score = 0;
		rank = LevelManager.Instance.GetRank(score);

		// Refresh the HUD and show the tutorial screen.
		UIManager.Instance.UpdateHUD(score, lives, rank);
		UIManager.Instance.ShowHUD(false);
		UIManager.Instance.ShowScreen("Tutorial");
	}

	/// <summary>
	/// Reloads the current scene.
	/// </summary>
	void ReloadScene()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	/// <summary>
	/// Call this function to start the gameplay.
	/// </summary>
	public void OnStartGame()
	{
		state = GameState.InGame;
		UIManager.Instance.ShowHUD(true);
		UIManager.Instance.ShowScreen("");
	}

	/// <summary>
	/// Call this function to restart the current level.
	/// </summary>
	public void OnRestart()
	{
		// Reload the current scene.
		Invoke("ReloadScene", 0.5f);
	}

	/// <summary>
	/// Call this function when the language has changed.
	/// </summary>
	public void OnLanguageChanged()
	{
		UIManager.Instance.OnLanguageChanged();
		UIManager.Instance.UpdateHUD(score, lives, rank);
	}

	/// <summary>
	/// Call this function when a bee returns to the beehive.
	/// </summary>
	public void OnBeeArrived()
	{
		// We only count bee arrivals during gameplay.
		if (CanPlay())
		{
			// Update the player score and rank and refresh the HUD.
			score++;
			rank = LevelManager.Instance.GetRank(score);
			UIManager.Instance.UpdateHUD(score, lives, rank);
		}
	}

	/// <summary>
	/// Call this function when a bee collides with another bee.
	/// </summary>
	public void OnBeeCollision()
	{
		// Make sure we have lives left, we don't want to go into negative numbers.
		if (lives > 0)
		{
			// Deduct one live an refresh the HUD.
			lives--;
			UIManager.Instance.UpdateHUD(score, lives, rank);

			// If the player has no more lives, the game is over.
			if (lives == 0)
			{
				state = GameState.GameOver;
				UIManager.Instance.ShowScreen("GameOver");
			}
		}
	}

	/// <summary>
	/// Determines whether the player can start playing.
	/// </summary>
	/// <returns><c>true</c> if the player can play; otherwise, <c>false</c>.</returns>
	public bool CanPlay()
	{
		// The player can move only during the InGame state.
		return (state == GameState.InGame);
	}
}