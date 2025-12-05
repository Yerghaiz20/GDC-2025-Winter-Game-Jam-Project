using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    public float offscreenY = -6f;

    void Update()
    {
        if (transform.position.y < offscreenY)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for a defended ship
        if (other.CompareTag("Player"))
        {
            // Notify the ship to handle its own destruction
            var ship = other.GetComponent<DefendedShip>();
            if (ship != null)
                ship.DestroyShip();

            // Destroy the missile
            Destroy(gameObject);
        }
    }
}
