using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Tilemap Reference")]
    public Tilemap levelTilemap;
    public TileBase[] wallTiles;

    [Header("Particles")]
    public ParticleSystem dustParticle;

    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    private Vector3Int gridPosition;
    public Vector3 targetWorldPos;
    public bool isMoving = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
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
            UpdateDustRotation();

            if (!dustParticle.isPlaying)
                dustParticle.Play();
        }
        else
        {
            TryMove();

            if (!isMoving)
            {
                animator.SetBool("isMoving", false);
                if (dustParticle.isPlaying)
                    dustParticle.Stop();
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
        if (lastInput == Vector2Int.zero && currentInput == Vector2Int.zero)
            return;

        Vector3Int nextPos = gridPosition + new Vector3Int(lastInput.x, lastInput.y, 0);
        if (IsWalkable(nextPos))
        {
            currentInput = lastInput;
            MoveTo(nextPos);
            return;
        }

        nextPos = gridPosition + new Vector3Int(currentInput.x, currentInput.y, 0);
        if (IsWalkable(nextPos))
        {
            MoveTo(nextPos);
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
        }
    }

    void MoveTo(Vector3Int nextPos)
    {
        gridPosition = nextPos;
        targetWorldPos = levelTilemap.GetCellCenterWorld(nextPos);
        isMoving = true;
    }

    void UpdateFacingDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up) animator.SetInteger("Direction", 0);
        else if (direction == Vector2Int.down) animator.SetInteger("Direction", 1);
        else if (direction == Vector2Int.left) animator.SetInteger("Direction", 2);
        else if (direction == Vector2Int.right) animator.SetInteger("Direction", 3);
    }

    void UpdateDustRotation()
    {
        if (currentInput == Vector2Int.up)
            dustParticle.transform.rotation = Quaternion.Euler(90, 0, 0);
        else if (currentInput == Vector2Int.down)
            dustParticle.transform.rotation = Quaternion.Euler(-90, 0, 0);
        else if (currentInput == Vector2Int.left)
            dustParticle.transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (currentInput == Vector2Int.right)
            dustParticle.transform.rotation = Quaternion.Euler(0, -90, 0);
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
}
