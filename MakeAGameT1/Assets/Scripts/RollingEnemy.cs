using System.Collections;
using System.Numerics;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private float speed;
    private Rigidbody2D rb;
    private GameObject player;
    private bool canAttack;
    [SerializeField]
    private float damageBuffer = 2.0f;

    [SerializeField]
    


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
        }
    }
    
    public IEnumerator DamageBuffer()
    {
        yield return new WaitForSeconds(damageBuffer);
        canAttack = false;
    }

}
