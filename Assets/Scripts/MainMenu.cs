using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The big text that says 'MERGE MAGE', 'YOU DIED', or 'YOU WON'")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Tooltip("The text inside the Start button (to change it to 'Restart')")]
    [SerializeField] private TextMeshProUGUI startButtonText;

    [Header("Scene Configuration")]
    [SerializeField] private string gameSceneName = "New Zone";

    public static string CurrentTitle = "MERGE MAGE";
    public static string CurrentButtonText = "START GAME";

    private void Start()
    {
        // 1. Unlock Cursor so we can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Update the text based on what happened
        if (titleText != null) titleText.text = CurrentTitle;
        if (startButtonText != null) startButtonText.text = CurrentButtonText;
    }

    public void OnStartButton()
    {
        // Reset defaults for next time
        CurrentTitle = "MERGE MAGE";
        CurrentButtonText = "START GAME";

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitButton()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public static void LoadMenu(string title, string buttonLabel, string menuSceneName = "MenuScene")
    {
        CurrentTitle = title;
        CurrentButtonText = buttonLabel;
        SceneManager.LoadScene(menuSceneName);
    }
}