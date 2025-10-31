using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Tilemap Reference")]
    public Tilemap levelTilemap;
    public TileBase[] wallTiles;
    [SerializeField] private TileBase pelletTile;
    [SerializeField] private TileBase powerPelletTile;

    // Input
    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    // Movement
    private Vector3Int gridPosition;
    private Vector3 targetWorldPos;
    public bool isMoving = false;

    // Animation
    private Animator animator;

    // Components
    private ParticleController particleController;
    private PacStudentAudio audioController;

    [Header("Tunnel Teleporters")]
    public float leftTunnelX;   
    public float rightTunnelX;  

    void Start()
    {
        animator = GetComponent<Animator>();
        particleController = GetComponentInChildren<ParticleController>();
        audioController = GetComponentInChildren<PacStudentAudio>();;

        gridPosition = levelTilemap.WorldToCell(transform.position);
        targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
        transform.position = targetWorldPos;

        animator.SetBool("isMoving", false);
    }

    void Update()
    {
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

        foreach (var wall in wallTiles)
        {
            if (tile == wall) return false;
        }
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
            GameManager.Instance.CollectPellet(10);
            audioController?.PlayPelletEat();
        }
        else if (tile == powerPelletTile)
        {
            levelTilemap.SetTile(gridPosition, null);
            GameManager.Instance.CollectPowerPellet(50);
            audioController?.PlayPelletEat();

            GhostManager.Instance.EnterScaredMode();

            GameManager.Instance.StartGhostTimer(10f);
        }
    }

    void CheckTeleporters()
    {
        Vector3 pos = transform.position;

        if (pos.x < leftTunnelX)  
        {
            pos.x = rightTunnelX;
            transform.position = pos;
            gridPosition = levelTilemap.WorldToCell(pos);
            targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
        }
        else if (pos.x > rightTunnelX)  
        {
            pos.x = leftTunnelX;
            transform.position = pos;
            gridPosition = levelTilemap.WorldToCell(pos);
            targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
        }
    }
}
