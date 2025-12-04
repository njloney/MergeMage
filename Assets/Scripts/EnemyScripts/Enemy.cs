using System.Collections;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(Health))]
public abstract class Enemy : MonoBehaviour
{
    private void OnDisable(){}

    private void HandleDamageTaken(float amount, DamageType type, Object source){}

    private IEnumerator FlashRed(){yield return null;}
    private void ResetCombo(){}

    private void die(){}

    private void dropItem(GameObject itemDrop){}
    private void Move(){}
}