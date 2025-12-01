using UnityEngine;
using System;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private MergeMode mergeController;

    [Header("UI References")]
    [SerializeField] private RectTransform crystalBackground;
    [SerializeField] private GameObject crystalPanel;
    [SerializeField] private GameObject mergePanel;

    [SerializeField] private float normalWidth = 220f;
    [SerializeField] private float mergeWidth = 125f;


    void Start()
    {

        if (mergeController != null)
        {
            mergeController.OnMergeModeChanged += UpdateInventoryView;
        }
    }

    private void UpdateInventoryView(bool mergeMode)
    {
        float updatedWidth = mergeMode ? mergeWidth : normalWidth;
        crystalPanel.SetActive(!mergeMode);
        mergePanel.SetActive(mergeMode);  
        crystalBackground.sizeDelta = new Vector2(updatedWidth, crystalBackground.sizeDelta.y);
       
    }

    // Update is called once per frame
}
