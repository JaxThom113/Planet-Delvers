using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Class")]
    [SerializeField] private EnemySO enemySO;
    [SerializeField] private SpriteRenderer sprite; 

    private int health; // current health, up to max
    private int damage; // current damage output, can be modified

    void Awake()
    {
        health = enemySO.baseHealth;
        damage = enemySO.baseDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // take damage according to Bullet.cs
            health -= collision.gameObject.GetComponent<Bullet>().damage;
            StartCoroutine(DamageFlash());
            
            if (health <= 0)
            {
                Die();
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator DamageFlash()
    {
        float flashTime = 0.2f;
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            
            currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / flashTime));
            sprite.material.SetFloat("_FlashAmount", currentFlashAmount);

            yield return null;
        }
    }

    void Die()
    {
       SpawnHealthCells();
       SpawnEnergyCells();
       SpawnSpecialItems();
    }

    private void SpawnHealthCells()
    {
        if (enemySO.healthCell != null && enemySO.healthCellCount > 0)
        {
            if (enemySO.healthCellCountMaxRandom)
            {
                // spawn a random count of health cells, up to the cell count
                int randCount = Random.Range(0, enemySO.healthCellCount + 1);
                for (int i = 0; i < randCount; i++)
                    CreateHealthCell();
            }
            else
            {
                // spawn all the health cells
                for (int i = 0; i < enemySO.healthCellCount; i++)
                    CreateHealthCell();
            }
        }
    }

    private void CreateHealthCell()
    {
        GameObject healthCell = Instantiate(enemySO.healthCell, gameObject.transform.position, Quaternion.identity);
        Rigidbody2D rb = healthCell.GetComponent<Rigidbody2D>();

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(randomDirection * 2f, ForceMode2D.Impulse);
    }

    private void SpawnEnergyCells()
    {
        if (enemySO.energyCell != null && enemySO.energyCellCount > 0)
        {
            if (enemySO.energyCellCountMaxRandom)
            {
                // spawn a random count of energy cells, up to the cell count
                int randCount = Random.Range(0, enemySO.energyCellCount + 1);
                for (int i = 0; i < randCount; i++)
                    CreateEnergyCell();
            }
            else
            {
                // spawn all the energy cells
                for (int i = 0; i < enemySO.energyCellCount; i++)
                    CreateEnergyCell();
            }
        }
    }

    private void CreateEnergyCell()
    {
        GameObject energyCell = Instantiate(enemySO.energyCell, gameObject.transform.position, Quaternion.identity);
        Rigidbody2D rb = energyCell.GetComponent<Rigidbody2D>();

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(randomDirection * 2f, ForceMode2D.Impulse);
    }

    private void SpawnSpecialItems()
    {
        if (enemySO.specialDrops.Count > 0)
        {
            if (enemySO.pickRandomSpecial)
            {
                // spawn one random
                int randIndex = Random.Range(0, enemySO.specialDrops.Count);
                Instantiate(enemySO.specialDrops[randIndex], gameObject.transform.position, Quaternion.identity);
            }
            else
            {
                // spawn all of the listed special items
                for (int i = 0; i < enemySO.specialDrops.Count; i++)
                    Instantiate(enemySO.specialDrops[i], gameObject.transform.position, Quaternion.identity);
            }
        }
    }
}
