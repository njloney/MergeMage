using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private MergeMode mergeController;
    [SerializeField] private ConsumableManager consumable;

    [Header("UI References")]
    [SerializeField] private GameObject crystalActive;
    [SerializeField] private GameObject mergeActive;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [SerializeField] private List<GameObject> crystalSlotHighlights = new List<GameObject>();
    [SerializeField] private List<GameObject> mergeSlotHighlights = new List<GameObject>();


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


        Debug.Log("Updating Inventory View and merge is" + mergeMode.ToString());
        mergeActive.SetActive(!mergeMode);
        crystalActive.SetActive(mergeMode);


        for (int i = 0; i < crystalSlotHighlights.Count; i++)
        {
            crystalSlotHighlights[i].SetActive(!mergeMode);
            mergeSlotHighlights[i].SetActive(mergeMode);
        }
    
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
