using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Tilemap Reference")]
    public Tilemap levelTilemap;
    public TileBase[] wallTiles;
    [SerializeField] private TileBase pelletTile;
    [SerializeField] private TileBase powerPelletTile;

    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;
    private Vector3Int gridPosition;
    private Vector3 targetWorldPos;
    public static bool isMoving = false;

    private Animator animator;
    private ParticleController particleController;
    private PacStudentAudio audioController;

    [Header("Tunnel Teleporters")]
    public float leftTunnelX;
    public float rightTunnelX;

    [Header("Spawn Settings")]
    public Vector3Int spawnCell;

    void Start()
    {
        animator = GetComponent<Animator>();
        particleController = GetComponentInChildren<ParticleController>();
        audioController = GetComponentInChildren<PacStudentAudio>();
        StartCoroutine(RespawnPacStudentCoroutine(true));
    }

    void Update()
    {
        if (!GameManager.Instance.allowInput)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        GatherInput();

        if (isMoving)
        {
            MoveLerp();
            animator.SetBool("isMoving", true);
            UpdateFacingDirection(currentInput);
            particleController?.PlayMove(true, currentInput);
            audioController?.HandleWalking(true);
        }
        else
        {
            TryMove();
            if (!isMoving)
            {
                animator.SetBool("isMoving", false);
                particleController?.PlayMove(false, currentInput);
                audioController?.HandleWalking(false);
            }
        }
    }

    void GatherInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2Int.right;
    }

    void TryMove()
    {
        if (lastInput != Vector2Int.zero)
        {
            Vector3Int tryPos = gridPosition + new Vector3Int(lastInput.x, lastInput.y, 0);
            if (IsWalkable(tryPos))
            {
                currentInput = lastInput;
                MoveTo(tryPos);
                return;
            }
        }

        if (currentInput != Vector2Int.zero)
        {
            Vector3Int nextPos = gridPosition + new Vector3Int(currentInput.x, currentInput.y, 0);
            if (IsWalkable(nextPos))
            {
                MoveTo(nextPos);
            }
            else
            {
                Vector3 wallWorldPos = levelTilemap.GetCellCenterWorld(nextPos);
                particleController?.PlayWallHit(nextPos, wallWorldPos);
                audioController?.PlayWallHit();
            }
        }
    }

    void MoveLerp()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetWorldPos) < 0.001f)
        {
            transform.position = targetWorldPos;
            gridPosition = levelTilemap.WorldToCell(transform.position);
            isMoving = false;
            CheckTileInteraction();
            CheckTeleporters();
        }
    }

    void MoveTo(Vector3Int nextPos)
    {
        gridPosition = nextPos;
        targetWorldPos = levelTilemap.GetCellCenterWorld(nextPos);
        isMoving = true;
    }

    bool IsWalkable(Vector3Int pos)
    {
        TileBase tile = levelTilemap.GetTile(pos);
        if (tile == null) return true;
        foreach (var wall in wallTiles) if (tile == wall) return false;
        return true;
    }

    void UpdateFacingDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up) animator.SetInteger("Direction", 0);
        else if (direction == Vector2Int.down) animator.SetInteger("Direction", 1);
        else if (direction == Vector2Int.left) animator.SetInteger("Direction", 2);
        else if (direction == Vector2Int.right) animator.SetInteger("Direction", 3);
    }

    void CheckTileInteraction()
    {
        TileBase tile = levelTilemap.GetTile(gridPosition);
        if (tile == null) return;

        if (tile == pelletTile)
        {
            levelTilemap.SetTile(gridPosition, null);
            GameManager.Instance.AddScore(10);
            audioController?.PlayPelletEat();

        if (GameManager.Instance.AreAllPelletsEaten())
        {
            GameManager.Instance.GameOver(); // or trigger next level
        }

        }
        else if (tile == powerPelletTile)
        {
            levelTilemap.SetTile(gridPosition, null);
            GameManager.Instance.AddScore(50);
            audioController?.PlayPelletEat();
            GhostController[] ghosts = FindObjectsOfType<GhostController>();
            foreach (var ghost in ghosts) ghost.SetState(GhostState.Scared, 10f);
            GameManager.Instance.StartGhostTimer(10f);

            if (GameManager.Instance.AreAllPelletsEaten())
            {
                GameManager.Instance.GameOver(); // or trigger next level
            }
        }
    }

    void CheckTeleporters()
    {
        Vector3 pos = transform.position;
        if (pos.x < leftTunnelX) pos.x = rightTunnelX;
        else if (pos.x > rightTunnelX) pos.x = leftTunnelX;
        transform.position = pos;
        gridPosition = levelTilemap.WorldToCell(pos);
        targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
    }

    public void Die()
    {
        if (!GameManager.Instance.allowInput) return;
        GameManager.Instance.allowInput = false;
        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        foreach (var ghost in ghosts) ghost.canMove = false;
        animator.SetTrigger("Die");
        GameManager.Instance.LoseLife();
        particleController?.PlayDeath();
        bool anyLivesLeft = false;
        foreach (var heart in GameManager.Instance.lifeImages)
            if (heart.enabled) { anyLivesLeft = true; break; }
        if (anyLivesLeft) StartCoroutine(RespawnPacStudentCoroutine(false));
        else StartCoroutine(GameOverCoroutine(ghosts));
    }

    private IEnumerator RespawnPacStudentCoroutine(bool isInitialSpawn)
    {
        if (!isInitialSpawn) yield return new WaitForSeconds(2f);
        Vector3 spawnWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        gridPosition = spawnCell;
        targetWorldPos = spawnWorldPos;
        transform.position = spawnWorldPos;
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;
        isMoving = false;
        animator.ResetTrigger("Die");
        animator.SetBool("isMoving", false);
        animator.SetInteger("Direction", 0);
        particleController?.PlayMove(false, currentInput);
        audioController?.HandleWalking(false);
        if (!isInitialSpawn)
        {
            GhostController[] ghosts = FindObjectsOfType<GhostController>();
            foreach (var ghost in ghosts)
            {
                ghost.StopAndTeleportToSpawn();
                ghost.ResumeMovement();
            }
        }
        GameManager.Instance.allowInput = true;
    }

    private IEnumerator GameOverCoroutine(GhostController[] ghosts)
    {
        foreach (var ghost in ghosts) ghost.canMove = false;
        GameManager.Instance.GameOver();
        yield return new WaitForSeconds(3f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
}
