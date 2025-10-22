using System.Numerics;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private float speed;
    private Rigidbody2D rb;
    private GameObject player; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Vector2 lookDir = new UnityEngine.Vector2(player.transform.position.x - rb.transform.position.x, 0).normalized;
        rb.AddForce(lookDir * speed);
    }
}
