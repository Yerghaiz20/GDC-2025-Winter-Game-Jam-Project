using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class Stage3Manager : MonoBehaviour
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
    public float bottomLimit = -4.5f;
    public float topLimit = 4.5f;
    public float rightLimit = 7.5f;
    public float leftLimit = -4.5f;
    public Vector2 formationAnchor = new Vector2(-8f, 0f);

    public enum ControlRotation { Deg0, Deg90, Deg180, Deg270 }
    public ControlRotation controlRotation;

    [Header("Obstacle Spawning")]
    public List<Sprite> obstacleSprites;
    public GameObject obstaclePrefab;
    public float spawnInterval = 1f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minAngle = -30f;
    public float maxAngle = 30f;

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

    List<GameObject> formationShips = new List<GameObject>();
    List<GameObject> activeEnemies = new List<GameObject>();
    List<GameObject> activePlayerProjectiles = new List<GameObject>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            if (gameManagerPrefab != null)
                Instantiate(gameManagerPrefab);
            else
                Debug.LogError("GameManager prefab not assigned!");
        }

        if (scoreDisplay)
            scoreDisplay.text = $"Score: {GameManager.Instance.score:F0}";

        timer = roundTime;

        // Randomize control rotation (including 0°)
        controlRotation = (ControlRotation)Random.Range(0, 4);

        SpawnFormation(GameManager.Instance.shipsRemaining);

        stageStarted = false;

        if (instructionPanel)
            instructionPanel.SetActive(true);
    }

    void Update()
    {
        if (!stageStarted || roundEnded)
            return;

        HandleTimer();
        HandleFormationMovement();
    }

    void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
            scoreDisplay.text = $"Score: {GameManager.Instance.score:F0}";
    }

    #region Formation Spawning

    void SpawnFormation(int count)
    {
        formationShips.Clear();

        if (count <= 0 || shipPrefab == null) return;

        float centerOffset = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = formationAnchor.x + (i - centerOffset) * formationSpacing;
            Vector3 pos = new Vector3(x, formationAnchor.y, 0f);

            GameObject s = Instantiate(shipPrefab, pos, Quaternion.identity);

            Rigidbody2D rb = s.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            }

            var ds = s.GetComponent<DefendedShip>();
            if (ds == null) ds = s.AddComponent<DefendedShip>();

            ds.OnShipDestroyed += HandleShipDestroyed;

            defendedShips.Add(s);
            formationShips.Add(s);
        }

        shipsAlive = defendedShips.Count;
    }

    #endregion

    #region Rotated Input + Movement

    void HandleFormationMovement()
    {
        float rawX = Input.GetAxisRaw("Horizontal");
        float rawY = Input.GetAxisRaw("Vertical");

        Vector2 rotated = RotateInput(new Vector2(rawX, rawY));

        if (Mathf.Approximately(rotated.x, 0f) && Mathf.Approximately(rotated.y, 0f))
            return;

        float deltaX = rotated.x * formationMoveSpeed * Time.deltaTime;
        float deltaY = rotated.y * formationMoveSpeed * Time.deltaTime;

        // Check vertical bounds
        foreach (var s in formationShips)
        {
            if (!s) continue;
            float nextY = s.transform.position.y + deltaY;
            if (nextY > topLimit || nextY < bottomLimit)
                return;
        }

        // Check horizontal bounds
        foreach (var s in formationShips)
        {
            if (!s) continue;
            float nextX = s.transform.position.x + deltaX;
            if (nextX > rightLimit || nextX < leftLimit)
                return;
        }

        // Apply movement
        foreach (var s in formationShips)
        {
            if (!s) continue;
            s.transform.position += new Vector3(deltaX, deltaY, 0f);
        }
    }

    Vector2 RotateInput(Vector2 v)
    {
        switch (controlRotation)
        {
            case ControlRotation.Deg90: return new Vector2(v.y, -v.x);
            case ControlRotation.Deg180: return new Vector2(-v.x, -v.y);
            case ControlRotation.Deg270: return new Vector2(-v.y, v.x);
            default: return v;
        }
    }

    #endregion

    public void BeginStage()
    {
        if (stageStarted) return;

        stageStarted = true;

        if (instructionPanel)
            instructionPanel.SetActive(false);

        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    void SpawnObstacle()
    {
        if (roundEnded) return;

        float x = Random.Range(-6f, 6f);
        Vector3 pos = new Vector3(x, 6f, 0);

        GameObject obj = Instantiate(obstaclePrefab, pos, Quaternion.identity);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr && obstacleSprites.Count > 0)
            sr.sprite = obstacleSprites[Random.Range(0, obstacleSprites.Count)];

        float angle = Random.Range(minAngle, maxAngle);
        float speed = Random.Range(minSpeed, maxSpeed);

        Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.down;
        obj.GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        obj.GetComponent<Rigidbody2D>().angularVelocity = 15f;

        activeEnemies.Add(obj);
    }

    #region Timer

    void HandleTimer()
    {
        timer -= Time.deltaTime;

        if (timerDisplay)
            timerDisplay.text = timer.ToString("F2");

        if (timer <= 0f)
        {
            timer = 0f;
            EndRound();
        }
    }

    void HandleShipDestroyed(DefendedShip ship)
    {
        shipsAlive--;

        if (shipsAlive <= 0)
            StartCoroutine(GameOverSequence());
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

    void CleanupEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);
    }

    void EndRound()
    {
        if (roundEnded) return;
        if (shipsAlive <= 0) return;

        roundEnded = true;

        GameManager.Instance.score += 300 * shipsAlive;
        UpdateScoreDisplay();

        GameManager.Instance.ApplySceneResults(shipsAlive);

        CleanupEnemies();

        foreach (var ship in defendedShips)
        {
            if (!ship) continue;

            Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;

            ship.transform.GetChild(0).gameObject.SetActive(true);

            StartCoroutine(ShipEscape(ship));
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

    #endregion
}
