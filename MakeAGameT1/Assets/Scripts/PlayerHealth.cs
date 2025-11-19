using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private float maxHealth = 15;
    private float currentHealth; 

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private Transform player;
    private Transform spawnpoint; 
    private float damageFlash = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnpoint = GameObject.FindGameObjectWithTag("Spawn").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddHealth(float amt)
    {
        maxHealth += amt;
        currentHealth += amt;
    }

    public void Damage(float dmg)
    {

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            StartCoroutine(DamageFlash());
        
        Debug.Log("Player took damage - " + dmg + " damage - " + currentHealth + " health remaining");
        if (IsDead())
        {
            Die();
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

    public void Die()
    {
        player.transform.position = new Vector2(spawnpoint.position.x, spawnpoint.position.y);
        currentHealth = maxHealth;
        Debug.Log("Dead");
    }

    public IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlash);
        spriteRenderer.color = Color.white;
    }

}
