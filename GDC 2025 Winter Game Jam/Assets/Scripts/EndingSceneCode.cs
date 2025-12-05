using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    public GameObject gameManagerPrefab;
    public GameObject earthSprite;
    public TimedTypewriter typewriter;

    [Header("Defense Targets")]
    public GameObject shipPrefab;
    public float formationSpacing = 1.2f;
    public Vector2 formationAnchor = new Vector2(-8f, 0f);

    [Header("Timer")]
    public float roundTime = 20f;
    public TMP_Text timerDisplay;

    [Header("Ending Text")]
    public TMP_Text endingText;

    float timer;

    public List<GameObject> defendedShips = new List<GameObject>();
    public int shipsAlive = 0;

    List<GameObject> formationShips = new List<GameObject>();


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

        AssignEndingText(GameManager.Instance.shipsRemaining);

        timer = roundTime;

        StartCoroutine(PlanetMovement(earthSprite));

        int survivors = GameManager.Instance.shipsRemaining;
        SpawnFormation(survivors);
    }


    void Update()
    {

    }

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

            s.transform.GetChild(0).gameObject.SetActive(true);

            formationShips.Add(s);
        }
    }

    void AssignEndingText(int shipsRemaining)
    {
        if (typewriter == null)
        {
            Debug.LogError("EndingManager has no TimedTypewriter reference assigned!");
            return;
        }

        string endingText;

        endingText = "You've made it! Earth 2.0! ";
        switch (shipsRemaining)
        {
            case 0:
            case 1:
                endingText +=
                    "Despite your bravery, only one ship remains. " +
                    "With limited supplies and finite labor, taming this new world proves difficult, " +
                    "and humanity struggles for centuries to become what they once were.";
                break;

            case 2:
                endingText +=
                    "Two ships breach atmosphere intact, carrying enough colonists and equipment to survive, " +
                    "though not without hardship. With resources stretched thin, progress is slow and every " +
                    "setback matters. Still, humanity endures, building a fragile foothold that will take decades to truly stabilize.";
                break;

            case 3:
                endingText +=
                    "Three ships touch down safely, bringing a balanced mix of personnel, tools, and knowledge. " +
                    "Settlements rise within years rather than decades. There are shortages and stumbles along the way, but the colony" +
                    "grows steadily. Within a few generations, humanity stands strong again, cautious but hopeful.";
                break;

            case 4:
                endingText +=
                    "Four ships survive the journey, delivering abundant supplies and skilled colonists. Expansion happens swiftly, " +
                    "and the new societies flourish. Though the scars of the old world are remembered, the colony enters a " +
                    "golden century of innovation and unity—humanity’s brightest rebirth.";
                break;

            case 5:
                endingText +=
                    "Of the five ships prepared for the journey, all five have survived! " +
                    "The colonists have everything they need for a hundred years, and civilization " +
                    "is quickly established.  Society thrives, and humanity enters an era of peace that lasts for millenia.";
                break;

            default:
                endingText =
                    $"{shipsRemaining} ships returned triumphant. " +
                    "Humanity celebrates a victory that will echo through the ages.";
                break;
        }

        typewriter.fullText = endingText;
    }

    System.Collections.IEnumerator PlanetMovement(GameObject planet)
    {
        Rigidbody2D rb = planet.GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.left * 1f;
        rb.angularVelocity = 22.5f;

        while (planet.transform.position.x > 7f)
        {
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        foreach (var ship in formationShips)
        {
            if (ship != null)
                StartCoroutine(MoveShipToPlanet(ship, planet.transform.position));
        }
    }

    System.Collections.IEnumerator MoveShipToPlanet(GameObject ship, Vector3 planetPos)
    {
        Rigidbody2D rb = ship.GetComponent<Rigidbody2D>();
        Transform tr = ship.transform;

        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;

        ship.transform.GetChild(0).gameObject.SetActive(true);

        float speed = 1.5f;
        Vector3 startScale = tr.localScale;
        float shrinkTime = 12f;
        float t = 0f;

        while (Vector3.Distance(tr.position, planetPos) > 0.1f)
        {
            Vector3 dir = (planetPos - tr.position).normalized;
            rb.linearVelocity = dir * speed;

            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / shrinkTime);
            tr.localScale = Vector3.Lerp(startScale, Vector3.zero, lerp);

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // Optionally disable or destroy the ship after arrival
        // Destroy(ship);
    }
}
