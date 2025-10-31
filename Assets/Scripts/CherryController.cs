using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject cherryPrefab;
    public float spawnDelay = 5f; // 5 seconds after scene start or after previous destroyed
    public float moveSpeed = 3f;

    [Header("Level Bounds")]
    public Vector2 levelMinBounds; // bottom-left corner (e.g. -10, -10)
    public Vector2 levelMaxBounds; // top-right corner (e.g. 10, 10)

    private GameObject currentCherry;
    private float spawnTimer;
    private Vector2 levelCenter;

    void Start()
    {
        levelCenter = (levelMinBounds + levelMaxBounds) / 2f;
        spawnTimer = spawnDelay;
    }

    void Update()
    {
        if (currentCherry == null)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                SpawnCherry();
                spawnTimer = spawnDelay;
            }
        }
    }

    void SpawnCherry()
    {
        // Pick one of 4 edges (0=Left, 1=Right, 2=Top, 3=Bottom)
        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;
        Vector2 exitPos = Vector2.zero;

        switch (side)
        {
            case 0: // Left → Right (through center)
                spawnPos = new Vector2(levelMinBounds.x - 1f, Random.Range(levelMinBounds.y, levelMaxBounds.y));
                exitPos = GetExitPosThroughCenter(spawnPos);
                break;
            case 1: // Right → Left (through center)
                spawnPos = new Vector2(levelMaxBounds.x + 1f, Random.Range(levelMinBounds.y, levelMaxBounds.y));
                exitPos = GetExitPosThroughCenter(spawnPos);
                break;
            case 2: // Top → Bottom (through center)
                spawnPos = new Vector2(Random.Range(levelMinBounds.x, levelMaxBounds.x), levelMaxBounds.y + 1f);
                exitPos = GetExitPosThroughCenter(spawnPos);
                break;
            case 3: // Bottom → Top (through center)
                spawnPos = new Vector2(Random.Range(levelMinBounds.x, levelMaxBounds.x), levelMinBounds.y - 1f);
                exitPos = GetExitPosThroughCenter(spawnPos);
                break;
        }

        currentCherry = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);

        // Add mover and pass target
        CherryMover mover = currentCherry.AddComponent<CherryMover>();
        mover.Initialize(exitPos, moveSpeed, this);

        // Render cherry above everything
        SpriteRenderer sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 20;
    }

    // Finds an opposite-side point so the line from spawnPos → center → exitPos is straight
    Vector2 GetExitPosThroughCenter(Vector2 spawnPos)
    {
        // Direction from spawn → center
        Vector2 dir = (levelCenter - spawnPos).normalized;

        // Extend the line in the same direction beyond the center, past the map bounds
        // until it exits the bounding box.
        Vector2 exit = levelCenter + dir * Mathf.Max(
            Mathf.Abs(levelMaxBounds.x - levelMinBounds.x),
            Mathf.Abs(levelMaxBounds.y - levelMinBounds.y)
        ) * 1.5f;

        return exit;
    }

    public void NotifyCherryDestroyed()
    {
        currentCherry = null;
        spawnTimer = spawnDelay;
    }
}
