using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private float attackCooldown;
    public float startAttackCooldown;

    public Transform attackPos;
    public float attackRange;
    public int damage;
    public LayerMask whatIsEnemies;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (attackCooldown <= 0)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(AttackFlash());
                Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for (int i = 0; i < enemiesToDamage.Length; i++)
                {
                    enemiesToDamage[i].gameObject.GetComponent<DummyHealth>().Damage(damage);
                    if (enemiesToDamage[i].gameObject.CompareTag("RollingEnemy"))
                    {
                        enemiesToDamage[i].gameObject.GetComponent<EnemyHealth>().Damage(damage);
                    }
                }
                attackCooldown = startAttackCooldown;

            }
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

        public IEnumerator AttackFlash()
    {
        spriteRenderer.color = Color.lightGreen;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}
 