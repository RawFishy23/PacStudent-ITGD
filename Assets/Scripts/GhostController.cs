using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public enum GhostState { Normal, Scared, Dead, Recovering }

[RequireComponent(typeof(Animator))]
public class GhostController : MonoBehaviour
{
    [Header("References")]
    public Tilemap levelTilemap;
    public TileBase[] wallTiles;
    public Transform pacStudent;
    public Vector3Int spawnCell; 
    public Vector3Int leftTunnelCell;
    public Vector3Int rightTunnelCell;

    [Header("Spawn/Exit Settings")]
    public Vector3Int topExitCell;             
    public Vector3Int bottomExitCell;          

    [Header("Settings")]
    public float normalSpeed = 4.5f;  
    public float scaredSpeed = 2.25f; 
    public float deadSpeed = 2.25f;   
    public int ghostNumber = 1;
    public float scaredTimer = 10f;
    public bool canMove = false; 
    public bool isRecovering = false;

    private bool inSpawnArea = true;
    private GhostState currentState = GhostState.Normal;

    private Vector3Int gridPosition;
    private Vector3Int lastGridPosition;
    private Vector3Int targetCell;
    private Vector3 targetWorldPos;
    private bool isMoving = false;
    private Vector3 lastWorldPos;

    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        gridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
        transform.position = targetWorldPos;
        lastWorldPos = targetWorldPos;
    }

    void Update()
    {
        if (!canMove) return; 

        if (currentState == GhostState.Dead)
        {
            HandleDeadMovement();
            UpdateAnimator();
            return;
        }

        if (currentState == GhostState.Scared)
        {
            scaredTimer -= Time.deltaTime;

            if (!isRecovering && scaredTimer <= 3f)
            {
                isRecovering = true;
                animator.SetBool("Recovering", true);
            }

            if (scaredTimer <= 0f)
            {
                currentState = GhostState.Normal;
                isRecovering = false;
                animator.SetBool("Recovering", false);
            }
        }


        if (Vector3.Distance(transform.position, pacStudent.position) < 0.45f)
        {
            if (currentState == GhostState.Scared)
            {
                GameManager.Instance.AddScore(300);
                SetState(GhostState.Dead);
            }
            else if (currentState == GhostState.Normal)
            {
                pacStudent.GetComponent<PacStudentController>().Die();
            }
        }

        float speed = GetSpeedByState();

        if (!isMoving)
            DecideNextMove();

        MoveLerp(speed);
        UpdateAnimator();
    }

    private float GetSpeedByState()
    {
        switch (currentState)
        {
            case GhostState.Normal: return normalSpeed;
            case GhostState.Scared: return scaredSpeed;
            case GhostState.Dead: return deadSpeed;
            default: return normalSpeed;
        }
    }

    private void MoveLerp(float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
            transform.position = targetWorldPos;
            gridPosition = Vector3Int.RoundToInt(targetCell);
            isMoving = false;
            lastWorldPos = transform.position;
        }

        if (inSpawnArea && (gridPosition == topExitCell || gridPosition == bottomExitCell))
        {
            inSpawnArea = false;
        }
    }

    private void DecideNextMove()
    {
        if (!inSpawnArea && (gridPosition == topExitCell || gridPosition == bottomExitCell))
        {
            Vector3Int outDir = (gridPosition == topExitCell) ? Vector3Int.up : Vector3Int.down;
            Vector3Int next = gridPosition + outDir;
            if (IsWalkable(next))
            {
                SetTargetCell(next);
                return;
            }
        }

        if (inSpawnArea && currentState != GhostState.Dead)
        {
            Vector3Int exitCell = (ghostNumber == 1 || ghostNumber == 3) ? topExitCell : bottomExitCell;
            SetTargetCell(exitCell);
            return;
        }

        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in directions)
        {
            Vector3Int nextCell = gridPosition + dir;
            if (IsWalkable(nextCell) && nextCell != lastGridPosition)
                possibleMoves.Add(nextCell);
        }

        if (possibleMoves.Count == 0)
        {
            foreach (var dir in directions)
            {
                Vector3Int nextCell = gridPosition + dir;
                if (IsWalkable(nextCell))
                    possibleMoves.Add(nextCell);
            }
        }

        targetCell = possibleMoves[0];
        switch (ghostNumber)
        {
            case 1: targetCell = SelectMoveForGhost1(possibleMoves); break;
            case 2: targetCell = SelectMoveForGhost2(possibleMoves); break;
            case 3: targetCell = SelectMoveForGhost3(possibleMoves); break;
            case 4: targetCell = SelectMoveForGhost4(possibleMoves); break;
        }

        targetWorldPos = levelTilemap.GetCellCenterWorld(targetCell);
        isMoving = true;
        lastGridPosition = gridPosition;
    }

    private void SetTargetCell(Vector3Int cell)
    {
        targetCell = cell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(cell);
        isMoving = true;
        lastGridPosition = gridPosition;
    }

    private Vector3Int SelectMoveForGhost1(List<Vector3Int> moves)
    {
        Vector3 pacPos = pacStudent.position;
        Vector3Int best = moves[0];
        float maxDist = Vector3.Distance(levelTilemap.GetCellCenterWorld(best), pacPos);
        foreach (var m in moves)
        {
            float d = Vector3.Distance(levelTilemap.GetCellCenterWorld(m), pacPos);
            if (d >= maxDist) { maxDist = d; best = m; }
        }
        return best;
    }

    private Vector3Int SelectMoveForGhost2(List<Vector3Int> moves)
    {
        Vector3 pacPos = pacStudent.position;
        Vector3Int best = moves[0];
        float minDist = Vector3.Distance(levelTilemap.GetCellCenterWorld(best), pacPos);
        foreach (var m in moves)
        {
            float d = Vector3.Distance(levelTilemap.GetCellCenterWorld(m), pacPos);
            if (d <= minDist) { minDist = d; best = m; }
        }
        return best;
    }

    private Vector3Int SelectMoveForGhost3(List<Vector3Int> moves)
    {
        return moves[Random.Range(0, moves.Count)];
    }

    private Vector3Int SelectMoveForGhost4(List<Vector3Int> moves)
    {
        Vector3Int[] clockwise = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
        Vector3Int candidate = gridPosition + clockwise[Random.Range(0, 4)];
        if (IsWalkable(candidate)) return candidate;
        return moves[Random.Range(0, moves.Count)];
    }

private bool IsWalkable(Vector3Int cell)
{
    if (currentState != GhostState.Dead)
    {
        if (cell == leftTunnelCell || cell == rightTunnelCell)
            return false;
        
        if (inSpawnArea == false && IsInsideSpawnDoor(cell))
            return false;
    }

    TileBase tile = levelTilemap.GetTile(cell);
    if (tile == null) return true;

    foreach (var wall in wallTiles)
    {
        if (tile == wall) return false;
    }

    return true;
}

    private bool IsInsideSpawnDoor(Vector3Int cell)
    {
        return cell == topExitCell || cell == bottomExitCell;
    }

    private void UpdateAnimator()
    {
        Vector3 delta = targetWorldPos - transform.position;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            animator.SetInteger("Direction", delta.x > 0 ? 3 : 2);
        else
            animator.SetInteger("Direction", delta.y > 0 ? 0 : 1);

        if (currentState == GhostState.Recovering)
            animator.SetBool("Recovering", true);
        else
            animator.SetBool("Recovering", false);

        if (currentState != GhostState.Recovering)
            animator.SetInteger("State", (int)currentState);
    }

    public void SetState(GhostState state, float duration = 0f)
    {
        currentState = state;

        if (state == GhostState.Scared)
        {
            scaredTimer = duration;
            isRecovering = false;
            animator.SetBool("Recovering", false);
            canMove = true;
        }
        else if (state == GhostState.Recovering)
        {
            isRecovering = true;
            animator.SetBool("Recovering", true);
        }
        else if (state == GhostState.Normal)
        {
            animator.SetBool("Recovering", false);
        }
    }

    private void HandleDeadMovement()
    {
        Vector3 spawnWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        float step = deadSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, spawnWorldPos, step);

        if (Vector3.Distance(transform.position, spawnWorldPos) < 0.01f)
        {
            RespawnGhost();
        }
    }

    private void RespawnGhost()
    {
        currentState = GhostState.Normal;
        inSpawnArea = true;
        gridPosition = spawnCell;
        transform.position = levelTilemap.GetCellCenterWorld(spawnCell);
        AudioPlayer.Instance.SwitchState(BGMState.Normal);
    }

    public void TeleportToSpawn()
    {
        currentState = GhostState.Normal;
        inSpawnArea = true;
        isMoving = false;
        gridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        transform.position = targetWorldPos;
    }

    public void StopAndTeleportToSpawn()
    {
        canMove = false;
        isMoving = false;
        gridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        transform.position = targetWorldPos;
        lastGridPosition = spawnCell;
        currentState = GhostState.Normal;
        animator.SetInteger("State", (int)currentState);
    }

    public void ResumeMovement()
    {
        canMove = true;
    }
}
