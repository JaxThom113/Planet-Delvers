using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxCheck : MonoBehaviour
{
    public bool IsHazard { get; private set; }
    public bool IsEnemy { get; private set; }

    public Vector2 KnockbackDir { get; private set; }
    [SerializeField] public float knockbackForce;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            IsHazard = true;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IsEnemy = true;
            KnockbackDir = (transform.position - collision.transform.position).normalized;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            IsHazard = false;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IsEnemy = false;
        }
    }
}
