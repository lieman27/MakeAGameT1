using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField]
    private GameObject player; 
    [SerializeField] private float maxHealth = 10;
    private float currentHealth;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private GameObject damagingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        CheckDamage();
    }

    public void CheckDamage()
    {
        
    }

    public void Damage(float dmg)
    {
        maxHealth -= dmg;
        if (IsDead())
        {
            player.transform.position = new Vector2(0, 0);
        }
    }

    public bool IsDead()
    {
        if (currentHealth <= 0)
        {
            return true;
        }
        return false;
    }
}
