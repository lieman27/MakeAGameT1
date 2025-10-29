using System.Collections;
using System.Numerics;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float initialSpeed; 
    [SerializeField]
    private float speed;
    private Rigidbody2D rb;
    private GameObject player;
    [SerializeField]
    private bool canAttack;
    [SerializeField]
    private float damageBuffer = 1.0f;

    [SerializeField]
    private float speedReset = 1.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");
        canAttack = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canAttack)
        {
            attackPlayer();
        }

    }

    void attackPlayer()
    {
        UnityEngine.Vector2 lookDir = new UnityEngine.Vector2(player.transform.position.x - rb.transform.position.x, 0).normalized;
        rb.AddForce(lookDir * speed);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canAttack = true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DamageBuffer());
            StartCoroutine(ResetSpeed());
        }
        
    }

    public IEnumerator DamageBuffer()
    {
        yield return new WaitForSeconds(damageBuffer);
        canAttack = false;
    }
    
    public IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(speedReset);
        speed = initialSpeed;
    }

}
