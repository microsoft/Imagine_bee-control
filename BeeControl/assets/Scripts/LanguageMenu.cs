using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script for the language selection menu.
/// </summary>
public class LanguageMenu : MonoBehaviour
{
	List<string> languages;		// List of supported languages.

	[SerializeField] GameObject menuPanel;		// Menu panel game object.
	[SerializeField] Button parentButton;		// Main menu button.
	[SerializeField] GameObject menuItemPrefab;	// Menu item prefab.

	bool open;	// Is the menu current opened?

	void Start()
	{
		// Get the list of available languages from the localization manager.
		languages = LocalizationManager.Instance.GetLanguages();

		// Create a menu button for each language.
		foreach (string l in languages)
		{
			GameObject button = Instantiate(menuItemPrefab) as GameObject;
			button.GetComponentInChildren<Text>().text = LocalizationManager.Instance.GetLanguageString(l);
			button.transform.SetParent(menuPanel.transform);
			string lang = l;
			button.GetComponent<Button>().onClick.AddListener(() =>
			{
				OnLanguageSelected(lang);
			});
		}

		// Close the menu by default.
		menuPanel.SetActive(false);
		open = false;

		// Set the button text to the currently selected language.
		parentButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.GetLanguageString(LocalizationManager.Instance.GetCurrentLanguage());
	}

	/// <summary>
	/// Call this when a new language is selected.
	/// </summary>
	/// <param name="lang">The selected language.</param>
	void OnLanguageSelected(string lang)
	{
		LocalizationManager.Instance.SetLanguage(lang);
		parentButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.GetLanguageString(lang);
		CloseMenu();
	}

	/// <summary>
	/// Call this to toggle the language menu.
	/// </summary>
	public void OnMenuPressed()
	{
		open = !open;
		menuPanel.SetActive(open);
	}

	/// <summary>
	/// Call this to close the menu.
	/// </summary>
	public void CloseMenu()
	{
		open = false;
		menuPanel.SetActive(false);
	}
}