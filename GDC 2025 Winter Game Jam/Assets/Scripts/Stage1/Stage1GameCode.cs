using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MissileCommandController : MonoBehaviour
{
    [Header("References")]
    public GameObject gameManagerPrefab;

    [Header("Stage Start")]
    public GameObject instructionPanel;
    public bool stageStarted = false;

    [Header("UI")]
    public TMP_Text scoreDisplay;

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab;
    public float spawnInterval = 1f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minAngle = -30f;
    public float maxAngle = 30f;

    [Header("Player")]
    public GameObject turretLeft;
    public GameObject turretRight;
    public GameObject playerProjectilePrefab;

    [Header("Defense Targets")]
    public List<GameObject> defendedShips;

    [Header("Timer")]
    public float roundTime = 20f;
    public TMP_Text timerDisplay;

    [Header("Scene Flow")]
    public string nextScene;
    public string gameOverScene;

    float timer;
    bool roundEnded = false;

    public int shipsAlive;

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

        shipsAlive = defendedShips.Count;

        foreach (var ship in defendedShips)
        {
            var ds = ship.GetComponent<DefendedShip>();
            ds.OnShipDestroyed += HandleShipDestroyed;
        }

        // Listen for enemy destruction events
        PlayerProjectile.OnEnemyDestroyed += UpdateScoreDisplay;

        timer = roundTime;

        stageStarted = false;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }
            
    }

    public void BeginStage()
    {
        if (stageStarted) return;

        stageStarted = true;

        // Hide UI card
        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        // Start enemy spawning
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void Update()
    {
        if (!stageStarted || roundEnded)
            return;

        HandleTimer();
        HandlePlayerFire();
    }

    void OnDestroy()
    {
        PlayerProjectile.OnEnemyDestroyed -= UpdateScoreDisplay;
    }

    void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
            scoreDisplay.text = $"Score: {GameManager.Instance.score:F0}";
    }

    

    void SpawnEnemy()
    {
        if (roundEnded) return;

        float x = Random.Range(-8f, 8f);
        Vector3 pos = new Vector3(x, 6f, 0);

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        float angle = Random.Range(minAngle, maxAngle);
        float speed = Random.Range(minSpeed, maxSpeed);

        Quaternion rot = Quaternion.Euler(0, 0, angle);
        Vector3 direction = rot * Vector3.down;

        enemy.GetComponent<Rigidbody2D>().linearVelocity = direction * speed;

        enemy.transform.rotation = Quaternion.Euler(0, 0, angle + 90);

        activeEnemies.Add(enemy);
    }

    void HandlePlayerFire()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        GameObject closer =
            Vector3.Distance(mouse, turretLeft.transform.position) <
            Vector3.Distance(mouse, turretRight.transform.position)
            ? turretLeft
            : turretRight;

        GameObject projectile =
            Instantiate(playerProjectilePrefab, closer.transform.position, Quaternion.identity);

        projectile.GetComponent<PlayerProjectile>().SetTarget(mouse);

        activePlayerProjectiles.Add(projectile);
    }

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

        foreach (var e in activeEnemies)
            if (e) Destroy(e);

        foreach (var p in activePlayerProjectiles)
            if (p) Destroy(p);

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(gameOverScene);
    }

    void EndRound()
    {
        if (roundEnded) return;
        if (shipsAlive <= 0) return;

        roundEnded = true;

        GameManager.Instance.score += 100 * shipsAlive;
        UpdateScoreDisplay();

        GameManager.Instance.ApplySceneResults(shipsAlive);

        foreach (var e in activeEnemies)
            if (e) Destroy(e);

        foreach (var p in activePlayerProjectiles)
            if (p) Destroy(p);

        foreach (var ship in defendedShips)
        {
            if (ship != null)
            {
                Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;

                ship.transform.GetChild(0).gameObject.SetActive(true);

                StartCoroutine(ShipEscape(ship));
            }
        }
    }

    System.Collections.IEnumerator ShipEscape(GameObject ship)
    {
        Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * 3f;

        while (ship.transform.position.y < 7f)
            yield return null;

        SceneManager.LoadScene(nextScene);
    }
}
