using System.Collections;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    [Header("Health References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health bossHealth;

    [Header("Menu Settings")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private float bossWinDelaySeconds = 5f;

    [Header("Hotkey Disable")]
    [SerializeField] private KeyCode disableKey = KeyCode.K;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Boss Animator")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string walkingBoolName = "IsWalking";

    private bool IsDisabled = false;

    private bool endingTriggered;

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnDied += OnPlayerDied;
        if (bossHealth != null) bossHealth.OnDied += OnBossDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnDied -= OnPlayerDied;
        if (bossHealth != null) bossHealth.OnDied -= OnBossDied;
    }

    private void Update()
    {
        if (Input.GetKeyDown(disableKey))
            DisableEverything();
    }

    private void DisableEverything()
    {
        // Disable scripts
        if (scriptsToDisable != null && IsDisabled == false)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }
            IsDisabled = true;
        } else if (scriptsToDisable != null && IsDisabled == true)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = true;
            }
            IsDisabled = false;
        }


        if (bossAnimator != null)
        {
            bossAnimator.SetBool(walkingBoolName, false);
        }
    }

    private void OnPlayerDied()
    {
        if (endingTriggered) return;
        endingTriggered = true;

        MainMenu.LoadMenu("YOU DIED", "TRY AGAIN", menuSceneName);
    }

    private void OnBossDied()
    {
        if (endingTriggered) return;
        endingTriggered = true;

        StartCoroutine(WinAfterDelay());
    }

    private IEnumerator WinAfterDelay()
    {
        if (bossWinDelaySeconds > 0f)
            yield return new WaitForSeconds(bossWinDelaySeconds);

        MainMenu.LoadMenu("VICTORY!", "PLAY AGAIN", menuSceneName);
    }
}
