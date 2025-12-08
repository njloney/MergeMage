using UnityEngine;
using System.Collections.Generic;

public class SpawnRandomConsumables : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<ItemData> AllConsumables = new List<ItemData>();
    void Start()
    {
        if (AllConsumables.Count == 0) return;

        Vector3 pos = transform.position;
        int randomValue = Random.Range(0, AllConsumables.Count);
        ItemData selectedItem = AllConsumables[randomValue];
        if (selectedItem != null && selectedItem.pickupPrefab != null)
        {
            Instantiate(selectedItem.pickupPrefab, pos, Quaternion.identity);
        }
    }
}
