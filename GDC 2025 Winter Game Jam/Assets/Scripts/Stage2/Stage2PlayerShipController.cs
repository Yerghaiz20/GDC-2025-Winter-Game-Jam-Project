using System.Collections.Generic;
using UnityEngine;

public class FormationControllerStage2 : MonoBehaviour
{
    [SerializeField] GameObject shipPrefab;
    [SerializeField] float spacing = 1.2f;
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float topLimit = 4.5f;
    [SerializeField] float bottomLimit = -4.5f;
    [SerializeField] Vector2 formationAnchor = new Vector2(-8f, 0f);
    List<GameObject> ships = new List<GameObject>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        int count = Mathf.Max(0, GameManager.Instance.shipsRemaining);
        SpawnVerticalFormation(count);
    }

    void SpawnVerticalFormation(int count)
    {
        ships.Clear();

        if (count <= 0 || shipPrefab == null) return;

        // Center the formation vertically on formationAnchor.y
        float centerOffset = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float y = formationAnchor.y + (i - centerOffset) * spacing;
            Vector3 pos = new Vector3(formationAnchor.x, y, 0f);
            GameObject s = Instantiate(shipPrefab, pos, Quaternion.identity);
            s.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            ships.Add(s);
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float input = Input.GetAxisRaw("Vertical");
        if (Mathf.Approximately(input, 0f)) return;

        float delta = input * moveSpeed * Time.deltaTime;

        // Check whether any ship would go out of bounds
        foreach (var s in ships)
        {
            if (s == null) continue;
            float nextY = s.transform.position.y + delta;
            if (nextY > topLimit || nextY < bottomLimit)
            {
                // If any ship would be out of bounds, cancel movement
                return;
            }
        }

        // Apply movement to the whole formation
        foreach (var s in ships)
        {
            if (s == null) continue;
            s.transform.position += new Vector3(0f, delta, 0f);
        }
    }
}
