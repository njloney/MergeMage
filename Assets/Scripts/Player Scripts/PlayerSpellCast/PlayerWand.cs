using UnityEngine;
using System.Collections.Generic;

public class PlayerWand : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Spells/Effects")]
    [SerializeField] private SpellCombinationResolver resolver;

    [Header("Wand Visuals")]
    [Tooltip("Empty GameObject on the wand model for the first crystal")]
    [SerializeField] private Transform socketA;
    [Tooltip("Empty GameObject on the wand model for the second crystal")]
    [SerializeField] private Transform socketB;

    [Tooltip("Visual-only prefab for a Fire crystal")]
    [SerializeField] private GameObject fireCrystalModel;
    [Tooltip("Visual-only prefab for an Ice crystal")]
    [SerializeField] private GameObject iceCrystalModel;
    [Tooltip("Visual-only prefab for a Wind crystal")]
    [SerializeField] private GameObject windCrystalModel;

    [Header("Dropping")]
    [Tooltip("Point where crystals are dropped (e.g., behind the player)")]
    [SerializeField] private Transform dropPoint;
    [Tooltip("The 'CrystalPickup' prefab for Fire")]
    [SerializeField] private GameObject fireCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Ice")]
    [SerializeField] private GameObject iceCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Wind")]
    [SerializeField] private GameObject windCrystalPickupPrefab;

    // --- Private State ---
    public CrystalType? slotA = null;
    public CrystalType? slotB = null;
    private GameObject spawnedModelA = null;
    private GameObject spawnedModelB = null;

    public bool HasA => slotA.HasValue;
    public bool HasB => slotB.HasValue;

    private void Start()
    {
        // Sync visuals on game start, in case we start with crystals
        UpdateWandVisuals();
    }

    public void Collect(CrystalType c)
    {
        if (!slotA.HasValue)
        {
            slotA = c;
            UpdateWandVisuals();
            return;
        }
        if (!slotB.HasValue)
        {
            slotB = c;
            UpdateWandVisuals();
            return;
        }

        // Both full: drop A, shift B to A, new -> B
        CrystalType crystalToDrop = slotA.Value;
        slotA = slotB;
        slotB = c;

        DropCrystal(crystalToDrop);
        UpdateWandVisuals();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryCast();
    }

    private void TryCast()
    {
        if (!slotA.HasValue || !slotB.HasValue || resolver == null || projectilePrefab == null || firePoint == null) return;

        
           
          return;

    }

    private void DropCrystal(CrystalType typeToDrop)
    {
        if (dropPoint == null)
        {
            Debug.LogWarning("No drop point set on wand. Cannot drop crystal.");
            return;
        }

        GameObject prefabToDrop = GetPickupPrefab(typeToDrop);

        if (prefabToDrop != null)
        {
            // Spawn the pickup prefab at the drop point
            Instantiate(prefabToDrop, dropPoint.position, dropPoint.rotation);
        }
    }

    private void UpdateWandVisuals()
    {
        // Clear old models
        if (spawnedModelA != null) Destroy(spawnedModelA);
        if (spawnedModelB != null) Destroy(spawnedModelB);

        // Spawn new model for Slot A
        if (slotA.HasValue && socketA != null)
        {
            GameObject modelPrefab = GetModelPrefab(slotA.Value);
            if (modelPrefab != null)
            {
                spawnedModelA = Instantiate(modelPrefab, socketA.position, socketA.rotation, socketA);
            }
        }

        // Spawn new model for Slot B
        if (slotB.HasValue && socketB != null)
        {
            GameObject modelPrefab = GetModelPrefab(slotB.Value);
            if (modelPrefab != null)
            {
                spawnedModelB = Instantiate(modelPrefab, socketB.position, socketB.rotation, socketB);
            }
        }
    }

    private GameObject GetModelPrefab(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.Fire: return fireCrystalModel;
            case CrystalType.Ice: return iceCrystalModel;
            case CrystalType.Wind: return windCrystalModel;
            default: return null;
        }
    }

    private GameObject GetPickupPrefab(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.Fire: return fireCrystalPickupPrefab;
            case CrystalType.Ice: return iceCrystalPickupPrefab;
            case CrystalType.Wind: return windCrystalPickupPrefab;
            default: return null;
        }
    }
}