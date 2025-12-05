using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class Stage2Manager : MonoBehaviour
{
    [Header("References")]
    public GameObject gameManagerPrefab;

    [Header("Stage Start")]
    public GameObject instructionPanel;
    public bool stageStarted = false;

    [Header("UI")]
    public TMP_Text scoreDisplay;

    [Header("Defense Targets")]
    public GameObject shipPrefab;
    public float formationSpacing = 1.2f;
    public float formationMoveSpeed = 4f;
    public float topLimit = 4.5f;
    public float bottomLimit = -4.5f;
    public Vector2 formationAnchor = new Vector2(-8f, 0f);

    [Header("Enemy")]
    public GameObject enemyShipPrefab1;
    public GameObject enemyShipPrefab2;
    public GameObject projectilePrefab;  // Projectile to spawn
    public int EnemiesPerColumn = 6;
    public float spacing = 1.2f;
    public Vector2 enemyFormationAnchor = new Vector2(-8f, 1f);
    public float sineAmplitude = 1f;      // Vertical sine movement amplitude
    public float sineFrequency = 1f;      // Sine movement speed
    public float projectileSpeed = 5f;    // Base projectile speed
    public float projectileInterval = 2f; // Seconds between shots

    List<GameObject> enemyShips = new List<GameObject>();
    float startY;
    float lastProjectileTime;

    [Header("Timer")]
    public float roundTime = 20f;
    public TMP_Text timerDisplay;

    [Header("Scene Flow")]
    public string nextScene;
    public string gameOverScene;

    float timer;
    bool roundEnded = false;

    public List<GameObject> defendedShips = new List<GameObject>();
    public int shipsAlive = 0;

    List<GameObject> activeEnemies = new List<GameObject>();
    List<GameObject> activePlayerProjectiles = new List<GameObject>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            else
            {
                Debug.LogError("GameManager prefab not assigned!");
            }
        }

        if (scoreDisplay != null)
        {
            scoreDisplay.text = $"Score: {GameManager.Instance.score:F0}";
        }

        stageStarted = false;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }

        
        startY = formationAnchor.y;

        timer = roundTime;
      
    }

    void Update()
    {
        if (!stageStarted || roundEnded)
            return;
        
        HandleTimer();
        HandleFormationMovement();
        MoveFormationSine();
        HandleProjectileSpawning();
    }

    void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
            scoreDisplay.text = $"Score: {GameManager.Instance.score:F0}";
    }

    public void BeginStage()
    {
        if (stageStarted) return;

        stageStarted = true;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        SpawnFormation(GameManager.Instance.shipsRemaining);
        SpawnEnemyVerticalFormation(EnemiesPerColumn);
    }

    #region Formation Spawning & Movement

    List<GameObject> formationShips = new List<GameObject>();

    void SpawnFormation(int count)
    {
        formationShips.Clear();

        if (count <= 0 || shipPrefab == null) return;

        float centerOffset = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float y = formationAnchor.y + (i - centerOffset) * formationSpacing;
            Vector3 pos = new Vector3(formationAnchor.x, y, 0f);

            GameObject s = Instantiate(shipPrefab, pos, Quaternion.identity);
            s.transform.rotation = Quaternion.Euler(0f, 0f, 270f);

            // Ensure the ship has a DefendedShip component
            var ds = s.GetComponent<DefendedShip>();
            if (ds == null) ds = s.AddComponent<DefendedShip>();

            ds.OnShipDestroyed += HandleShipDestroyed;

            defendedShips.Add(s);
            formationShips.Add(s);
        }

        shipsAlive = defendedShips.Count;
    }

    void HandleFormationMovement()
    {
        float input = Input.GetAxisRaw("Vertical");
        if (Mathf.Approximately(input, 0f)) return;

        float delta = input * formationMoveSpeed * Time.deltaTime;

        // Cancel movement if any ship would go out of bounds
        foreach (var s in formationShips)
        {
            if (s == null) continue;
            float nextY = s.transform.position.y + delta;
            if (nextY > topLimit || nextY < bottomLimit) return;
        }

        // Apply movement
        foreach (var s in formationShips)
        {
            if (s == null) continue;
            s.transform.position += new Vector3(0f, delta, 0f);
        }
    }

    void SpawnEnemyVerticalFormation(int EnemiesPerColumn)
    {
        enemyShips.Clear();

        if (EnemiesPerColumn <= 0) return;

        float centerOffset = (EnemiesPerColumn - 1) / 2f;

        // Column offsets on X
        float[] columnOffsets = new float[]
        {
            -1.5f,   // left column 1
            -0.5f,   // left column 2
             0.5f    // right column
        };

        for (int col = 0; col < columnOffsets.Length; col++)
        {
            for (int i = 0; i < EnemiesPerColumn; i++)
            {
                float y = enemyFormationAnchor.y + (i - centerOffset) * spacing;
                float x = enemyFormationAnchor.x + columnOffsets[col];
                Vector3 pos = new Vector3(x, y, 0f);

                GameObject prefabToUse =
                    (col < 2) ? enemyShipPrefab1 : enemyShipPrefab2;

                GameObject s = Instantiate(prefabToUse, pos, Quaternion.identity);
                s.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                enemyShips.Add(s);
            }
        }
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

    #endregion

    

    #region Timer & Round Handling

    void HandleTimer()
    {
        timer -= Time.deltaTime;

        if (timerDisplay)
            timerDisplay.text = timer.ToString("F2");

        if (timer <= 0f)
        {
            timer = 0.00f;
            EndRound();
        }
    }

    void HandleShipDestroyed(DefendedShip ship)
    {
        shipsAlive--;

        if (shipsAlive <= 0)
        {
            StartCoroutine(GameOverSequence());
        }
    }

    System.Collections.IEnumerator GameOverSequence()
    {
        if (roundEnded) yield break;
        roundEnded = true;

        // Cleanup
        foreach (var e in activeEnemies)
            if (e) Destroy(e);

        foreach (var p in activePlayerProjectiles)
            if (p) Destroy(p);

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(gameOverScene);
    }

    void CleanupEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    void EndRound()
    {
        if (roundEnded) return;
        if (shipsAlive <= 0) return;

        roundEnded = true;

        GameManager.Instance.score += 200 * shipsAlive;
        UpdateScoreDisplay();

        GameManager.Instance.ApplySceneResults(shipsAlive);

        CleanupEnemies();

        foreach (var ship in defendedShips)
        {
            if (ship != null)
            {
                Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;

                ship.transform.GetChild(0).gameObject.SetActive(true);

                StartCoroutine(ShipEscape(ship));
            }
        }
    }

    System.Collections.IEnumerator ShipEscape(GameObject ship)
    {
        Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.right * 3f;

        while (ship.transform.position.x < 8f)
            yield return null;

        SceneManager.LoadScene(nextScene);
    }

    #endregion
}
