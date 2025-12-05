using UnityEngine;
using System;

public class PlayerProjectile : MonoBehaviour
{

    public static event Action OnEnemyDestroyed;

    public GameObject gameManagerPrefab;

    Vector3 target;
    public float speed = 6f;
    public float explosionRadius = 1.5f;
    public GameObject explosionEffect;
    bool exploded = false;

    public void SetTarget(Vector3 t)
    {
        target = t;
    }

    void Update()
    {
        if (exploded) return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
            Explode();
    }

    void Explode()
    {
        exploded = true;

        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy"))
            {
                GameManager.Instance.score += 10;

                OnEnemyDestroyed?.Invoke();

                Destroy(h.gameObject);
            }
                
                
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
