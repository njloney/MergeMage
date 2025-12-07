using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private MergeMode mergeController;
    [SerializeField] private ConsumableManager consumable;

    [Header("UI References")]
    [SerializeField] private RectTransform crystalBackground;
    [SerializeField] private GameObject crystalPanel;
    [SerializeField] private GameObject mergePanel;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private float normalWidth = 220f;
    [SerializeField] private float mergeWidth = 125f;


    void Start()
    {

        if (mergeController != null)
        {
            mergeController.OnMergeModeChanged += UpdateInventoryView;
        }

        if (consumable != null)
        {
            consumable.CooldownUpdate += UpdateCoolDown;
        }

    }

    private void UpdateInventoryView(bool mergeMode)
    {
        float updatedWidth = mergeMode ? mergeWidth : normalWidth;
        crystalPanel.SetActive(!mergeMode);
        mergePanel.SetActive(mergeMode);  
        crystalBackground.sizeDelta = new Vector2(updatedWidth, crystalBackground.sizeDelta.y);
       
    }

    private void UpdateCoolDown(float cooldown)
    {
        bool shouldBeVisible = cooldown > 0;

        if (cooldownText.gameObject.activeSelf != shouldBeVisible)
        {
            cooldownText.gameObject.SetActive(shouldBeVisible);
        }

        if (shouldBeVisible)
        {
            cooldownText.text = cooldown.ToString("F0");
        }
    }

    // Update is called once per frame
}
