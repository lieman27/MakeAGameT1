using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
public class DummyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10;
    private float currentHealth;

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private GameObject enemy;
    private float damageFlash = 0.1f;
    private bool wallDmgPossible;
    private float wallDmgCooldown = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

    }
    void Start()
    {
        currentHealth = maxHealth;
        enemy = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!wallDmgPossible)
        {
            wallDmgCooldown -= Time.deltaTime;
            if (wallDmgCooldown < 0)
            {
                wallDmgPossible = true;
            }
        }
    }
    public void Damage(float dmg)
    {

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        StartCoroutine(DamageFlash());

        Debug.Log("Enemy took damage - " + dmg + " damage - " + currentHealth + " health remaining");
        if (IsDead())
        {
            Die();
        }
        wallDmgPossible = false;
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
        Destroy(gameObject);
    }

    public IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlash);
        spriteRenderer.color = Color.skyBlue;
    }
}
