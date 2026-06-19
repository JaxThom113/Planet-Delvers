using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // face left by default
        rb.velocity = -transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // delete the bullet instance
            Destroy(gameObject);
        }

        if (collision.CompareTag("Enemy"))
        {
            // delete the enemy that was hit
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
