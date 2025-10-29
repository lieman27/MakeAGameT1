using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/StatChangeItem")]
public class StatChangesItem : Item
{

    public float healthChange;
    public float speedChange;
    public override void Apply(GameObject target)
    {
        target.GetComponent<PlayerHealth>().AddHealth(healthChange);
        target.GetComponent<PlayerController>().maxRunSpeed += speedChange;
        Debug.Log("Health changed by " + healthChange + ", speed changed by " + speedChange);
    }
}
