using System.Collections.Generic;
using UnityEngine;

public class Stage2Controller : MonoBehaviour
{
    [SerializeField] GameObject enemyShipPrefab1;
    [SerializeField] GameObject enemyShipPrefab2;
    [SerializeField] GameObject projectilePrefab;  // Projectile to spawn
    [SerializeField] float spacing = 1.2f;
    [SerializeField] Vector2 formationAnchor = new Vector2(-8f, 1f);
    [SerializeField] float sineAmplitude = 1f;      // Vertical sine movement amplitude
    [SerializeField] float sineFrequency = 1f;      // Sine movement speed
    [SerializeField] float projectileSpeed = 5f;    // Base projectile speed
    [SerializeField] float projectileInterval = 2f; // Seconds between shots

    List<GameObject> enemyShips = new List<GameObject>();
    float startY;
    float lastProjectileTime;

    void Start()
    {
        int count = 6;
        startY = formationAnchor.y;
        SpawnEnemyVerticalFormation(count);
    }

    void Update()
    {
        MoveFormationSine();
        HandleProjectileSpawning();
    }

    void MoveFormationSine()
    {
        float newY = startY + Mathf.Sin(Time.time * sineFrequency) * sineAmplitude;

        // Move each ship relative to its initial X offset
        for (int i = 0; i < enemyShips.Count; i++)
        {
            if (enemyShips[i] != null)
            {
                Vector3 pos = enemyShips[i].transform.position;
                pos.y = newY + (i % 6 - 3) * spacing; // same vertical spacing as initial
                enemyShips[i].transform.position = pos;
            }
        }
    }

    void HandleProjectileSpawning()
    {
        if (Time.time - lastProjectileTime < projectileInterval) return;
        lastProjectileTime = Time.time;

        if (enemyShips.Count == 0) return;

        // Pick a random ship
        GameObject ship = enemyShips[Random.Range(0, enemyShips.Count)];
        if (ship == null) return;

        // Spawn projectile at ship position
        GameObject proj = Instantiate(projectilePrefab, ship.transform.position, Quaternion.identity);

        // Determine velocity
        float speedMultiplier = (ship.transform.position.x > 7.5f) ? 2f : 1f; // right column = x > 0
        proj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * projectileSpeed * speedMultiplier;
    }

    void SpawnEnemyVerticalFormation(int count)
    {
        enemyShips.Clear();

        if (count <= 0) return;

        float centerOffset = (count - 1) / 2f;

        // Column offsets on X
        float[] columnOffsets = new float[]
        {
            -1.5f,   // left column 1
            -0.5f,   // left column 2
             0.5f    // right column
        };

        for (int col = 0; col < columnOffsets.Length; col++)
        {
            for (int i = 0; i < count; i++)
            {
                float y = formationAnchor.y + (i - centerOffset) * spacing;
                float x = formationAnchor.x + columnOffsets[col];
                Vector3 pos = new Vector3(x, y, 0f);

                GameObject prefabToUse =
                    (col < 2) ? enemyShipPrefab1 : enemyShipPrefab2;

                GameObject s = Instantiate(prefabToUse, pos, Quaternion.identity);
                s.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                enemyShips.Add(s);
            }
        }
    }
}
