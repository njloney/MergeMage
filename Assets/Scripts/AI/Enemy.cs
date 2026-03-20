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
        Benjathemaker.SimpleGemsAnim animScript = retItem.GetComponent<Benjathemaker.SimpleGemsAnim>();
        animScript.isRotating = false;
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
            script.rotateItem = false;
        }
        return retItem;
    }

    public void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null)
        {
            Debug.Log("No pickup prefab assigned!");
            return;
        }

        Vector3 dropPosition = transform.position;
        Vector3 rayStart = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f))
        {
            dropPosition = hit.point;
        }
        Benjathemaker.SimpleGemsAnim animScript = itemDrop.GetComponent<Benjathemaker.SimpleGemsAnim>();
        if (animScript != null) animScript.isRotating = true;
        PassiveItemPickup script = itemDrop.GetComponent<PassiveItemPickup>();
        if (script != null) script.rotateItem = true;
        itemDrop = Instantiate(itemDrop, dropPosition, Quaternion.identity);
        itemDrop.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }
}