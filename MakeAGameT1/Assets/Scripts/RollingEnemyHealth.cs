using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    [SerializeField] private float maxHealth = 10;
    private float currentHealth;

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private GameObject enemy;
    private float damageFlash = 0.1f;
    private float wallDmgInvincibility = 2.5f;
    private bool wallDmgPossible;
    private float wallDmgCooldown = 1.0f;
    private GameObject cvc; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cvc = GameObject.Find("CinemachineCamera");

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DamagingWall"))
        {
            if (!wallDmgPossible)
            {
                return;
            }

            CameraController cameraController = cvc.GetComponent<CameraController>();
            cameraController.CameraShake(4.46f);
            Damage(5);

            wallDmgPossible = false;
            wallDmgCooldown = wallDmgInvincibility;
            

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
        Destroy(gameObject);
    }

    public IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlash);
        spriteRenderer.color = Color.hotPink;
    }
}
