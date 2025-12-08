using UnityEngine;

[RequireComponent(typeof(Health))]
public class GameEndTrigger : MonoBehaviour
{
    public enum EndType { Loss, Win }

    [Header("Settings")]
    public EndType endCondition;
    [Tooltip("Name of your Menu Scene")]
    public string menuSceneName = "MenuScene";

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += TriggerEnd;
    }

    private void OnDisable()
    {
        health.OnDied -= TriggerEnd;
    }

    private void TriggerEnd()
    {
        if (endCondition == EndType.Loss)
        {
            // Player Died
            MainMenu.LoadMenu("YOU DIED", "TRY AGAIN", menuSceneName);
        }
        else
        {
            // Boss Died
            MainMenu.LoadMenu("VICTORY!", "PLAY AGAIN", menuSceneName);
        }
    }
}