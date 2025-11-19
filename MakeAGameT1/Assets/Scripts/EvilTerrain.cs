using System.Collections;
using UnityEngine;

public class EvilTerrain : MonoBehaviour
{

    [SerializeField]
    private GameObject terrain;
    [SerializeField]
    private Transform player;
    [SerializeField]
    private float attackDamage = 5f;
    [SerializeField]
    private float damageTimer = 1.5f;

    private float timer = 0f;
    private bool inHitbox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfPlayer();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inHitbox = true;
            timer = 0;

        }

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inHitbox = false;
            timer = 0;
        }

    }

    void CheckIfPlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (inHitbox)
        {
            
            StartCoroutine(DamagePlayer());
            
        }
    }

    public IEnumerator DamagePlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        playerHealth.Damage(attackDamage);
        yield return new WaitForSeconds(damageTimer);
        timer = damageTimer; 
    }
}
