using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// The user interface (UI) manager is responsible for controlling which screen to display
/// as well as updating the current game heads up display (HUD). The UI manager is a singleton
/// and can be accessed in any script using the UIManager.Instance syntax.
/// </summary>
public class UIManager : MonoBehaviour
{
	// The static singleton instance of the UI manager.
	public static UIManager Instance { get; private set; }

	public GameObject hud;			// GameObject for the HUD.
	public Text scoreText;			// HUD text for the player score.
	public Text livesText;			// HUD text for the player lives.
	public Text rankText;			// HUD text for the player rank.
	public GameObject alertIcon;	// Prefab for the HUD alert icon.
	public GameObject[] screens;	// GameObject array for all the screens.
	public AudioClip buttonClick;	// Sound when a button is clicked.

	AudioSource buttonClickSource;

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	void Start()
	{
		// Create audio sources for sound playback.
		buttonClickSource = AudioHelper.CreateAudioSource(gameObject, buttonClick);
	}

	/// <summary>
	/// Shows the screen with the given name and hide everything else.
	/// </summary>
	/// <param name="name">Name of the screen to be shown.</param>
	public void ShowScreen(string name)
	{
		// Loop through all the screens in the array.
		foreach (GameObject screen in screens)
		{
			// Activate the screen with the matching name, and deactivate
			// any screen that doesn't match.
			screen.SetActive(screen.name == name);
		}
	}

	/// <summary>
	/// Shows/hides the HUD.
	/// </summary>
	/// <param name="show">Do we show the HUD?</param>
	public void ShowHUD(bool show)
	{
		hud.SetActive(show);
	}

	/// <summary>
	/// Updates the game HUD.
	/// </summary>
	/// <param name="score">The player's score.</param>
	/// <param name="lives">The player's lives remaining.</param>
	/// <param name="rank">The player's rank.</param>
	public void UpdateHUD(int score, int lives, PlayerRank rank)
	{
		scoreText.text = string.Format(LocalizationManager.Instance.GetString("HUD Score"), score.ToString());
		livesText.text = string.Format(LocalizationManager.Instance.GetString("HUD Lives"), lives.ToString());
		string rankKey = "Rank " + rank.ToString();
		rankText.text = string.Format(LocalizationManager.Instance.GetString("HUD Rank"), LocalizationManager.Instance.GetString(rankKey));
	}

	/// <summary>
	/// Call this to spawn a new alert icon tracking the given bee.
	/// </summary>
	/// <param name="bee">Game object for the bee to be tracked.</param>
	public void ShowAlertIcon(GameObject bee)
	{
		GameObject icon = Instantiate(alertIcon) as GameObject;
		icon.GetComponent<AlertIcon>().TrackBee(bee);
		icon.transform.SetParent(hud.transform, true);
	}

	/// <summary>
	/// Call this function to play the button click sound.
	/// </summary>
	public void OnButton()
	{
		buttonClickSource.Play();
	}

	/// <summary>
	/// Call this when a new language is selected.
	/// </summary>
	public void OnLanguageChanged()
	{
		// Refresh all the static text fields.
		var objs = FindObjectsOfType<StaticTextManager>();
		foreach (StaticTextManager staticText in objs)
		{
			staticText.OnLanguageChanged();
		}
	}
}