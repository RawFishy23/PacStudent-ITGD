using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject cherryPrefab;
    public float spawnDelay = 5f;         
    public float moveSpeed = 3f;          
    public Vector2 levelMinBounds;        
    public Vector2 levelMaxBounds;        

    private GameObject currentCherry;
    private float spawnTimer;

    void Start()
    {
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
        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;
        Vector2 exitPos = Vector2.zero;

        switch (side)
        {
            case 0: 
                spawnPos = new Vector2(levelMinBounds.x - 1f, Random.Range(levelMinBounds.y, levelMaxBounds.y));
                exitPos = new Vector2(levelMaxBounds.x + 1f, spawnPos.y);
                break;
            case 1: 
                spawnPos = new Vector2(levelMaxBounds.x + 1f, Random.Range(levelMinBounds.y, levelMaxBounds.y));
                exitPos = new Vector2(levelMinBounds.x - 1f, spawnPos.y);
                break;
            case 2: 
                spawnPos = new Vector2(Random.Range(levelMinBounds.x, levelMaxBounds.x), levelMaxBounds.y + 1f);
                exitPos = new Vector2(spawnPos.x, levelMinBounds.y - 1f);
                break;
            case 3: 
                spawnPos = new Vector2(Random.Range(levelMinBounds.x, levelMaxBounds.x), levelMinBounds.y - 1f);
                exitPos = new Vector2(spawnPos.x, levelMaxBounds.y + 1f);
                break;
        }

        currentCherry = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);

        CherryMover mover = currentCherry.AddComponent<CherryMover>();
        mover.Initialize(exitPos, moveSpeed, this);

        SpriteRenderer sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 10; 
    }

    public void NotifyCherryDestroyed()
    {
        currentCherry = null;
        spawnTimer = spawnDelay; 
    }
}
