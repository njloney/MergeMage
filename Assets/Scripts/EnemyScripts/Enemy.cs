using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(Health))]
public abstract class Enemy : MonoBehaviour
{
    public GameObject getRandomCrystal()
    {
        // get random crystal
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs/CrystalPickUp");
        GameObject retItem = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];

        // stop rotating animation
        Component animScript = retItem.GetComponent("SimpleGemsAnim");
        if (animScript != null) Destroy(animScript);
        return retItem;
    }
    public GameObject getRandomItem()
    {
        // 50% chance it's a crystal
        GameObject retItem = getRandomCrystal();
        if (Random.value < 0.5f)
        {
            // get all passive items
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/ItemPrefabs/PassiveItemPrefabs");
            GameObject[] items;

            // 0.1 chance it's maxed, 0.9 chance otherwise
            items = Random.value < 0.1f ? prefabs.Where(obj => obj.name.StartsWith("Max")).ToArray()
            : prefabs.Where(obj => !obj.name.StartsWith("Max")).ToArray();
            retItem = items[UnityEngine.Random.Range(0, items.Length)];
            // no rotating
            PassiveItemPickup script = retItem.GetComponent<PassiveItemPickup>();
            if (script.rotateItem != null) script.rotateItem = false;
        }
        return retItem;
    }

}