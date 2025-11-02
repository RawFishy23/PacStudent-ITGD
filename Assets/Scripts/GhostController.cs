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
        lastGridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(gridPosition);
        transform.position = targetWorldPos;
        lastWorldPos = targetWorldPos;
        UpdateAnimator();
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
                SetState(GhostState.Normal);
                return;
            }
        }

        if (Vector3.Distance(transform.position, pacStudent.position) < 0.45f)
        {
            if (currentState == GhostState.Scared || currentState == GhostState.Recovering)
            {
                GameManager.Instance.AddScore(300);
                SetState(GhostState.Dead);
                UpdateAudioAfterPossibleChange();
            }
            else if (currentState == GhostState.Normal)
            {
                pacStudent.GetComponent<PacStudentController>().Die();
            }
        }

        float speed = GetSpeedByState();

        if (!isMoving) DecideNextMove();

        MoveLerp(speed);
        UpdateAnimator();
    }

    private float GetSpeedByState()
    {
        switch (currentState)
        {
            case GhostState.Normal: return normalSpeed;
            case GhostState.Scared: return scaredSpeed;
            case GhostState.Recovering: return scaredSpeed;
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

            if (inSpawnArea && gridPosition != spawnCell && gridPosition != topExitCell && gridPosition != bottomExitCell)
            {
                inSpawnArea = false;
            }
        }

        if (inSpawnArea)
        {
            Vector3Int exitCell = (ghostNumber == 1 || ghostNumber == 3) ? topExitCell : bottomExitCell;
            Vector3Int outside = exitCell + ((exitCell == topExitCell) ? Vector3Int.up : Vector3Int.down);
            if (Vector3Int.RoundToInt(levelTilemap.WorldToCell(transform.position)) == outside)
                inSpawnArea = false;
        }
    }

    private void DecideNextMove()
    {
        if (inSpawnArea && currentState != GhostState.Dead)
        {
            if (gridPosition == spawnCell)
            {
                Vector3Int exitCell = (ghostNumber == 1 || ghostNumber == 3) ? topExitCell : bottomExitCell;
                SetTargetCell(exitCell);
                return;
            }
            Vector3Int exit = (ghostNumber == 1 || ghostNumber == 3) ? topExitCell : bottomExitCell;
            if (gridPosition == exit)
            {
                Vector3Int outside = exit + ((exit == topExitCell) ? Vector3Int.up : Vector3Int.down);
                SetTargetCell(outside);
                return;
            }
        }

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

        if (possibleMoves.Count == 0) return;

        targetCell = possibleMoves[0];

        int behaviorNumber = (currentState == GhostState.Scared || currentState == GhostState.Recovering) ? 1 : ghostNumber;

        switch (behaviorNumber)
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
        float bestDist = Vector3.Distance(levelTilemap.GetCellCenterWorld(best), pacPos);
        foreach (var m in moves)
        {
            float d = Vector3.Distance(levelTilemap.GetCellCenterWorld(m), pacPos);
            if (d >= bestDist)
            {
                bestDist = d;
                best = m;
            }
        }
        return best;
    }

    private Vector3Int SelectMoveForGhost2(List<Vector3Int> moves)
    {
        Vector3 pacPos = pacStudent.position;
        Vector3Int best = moves[0];
        float bestDist = Vector3.Distance(levelTilemap.GetCellCenterWorld(best), pacPos);
        foreach (var m in moves)
        {
            float d = Vector3.Distance(levelTilemap.GetCellCenterWorld(m), pacPos);
            if (d <= bestDist)
            {
                bestDist = d;
                best = m;
            }
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
        if (cell == leftTunnelCell || cell == rightTunnelCell) return false;

        if (currentState == GhostState.Dead)
        {
            TileBase t = levelTilemap.GetTile(cell);
            if (t == null) return true;
            foreach (var wall in wallTiles)
                if (t == wall)
                    return true;  
            return true;
        }

        if ((cell == spawnCell || cell == topExitCell || cell == bottomExitCell))
        {
            if (inSpawnArea) return true;
            if (currentState == GhostState.Dead) return true;
            return false;
        }

        TileBase tile = levelTilemap.GetTile(cell);
        if (tile == null) return true;
        foreach (var wall in wallTiles)
            if (tile == wall) return false;
        return true;
    }

    private void UpdateAnimator()
    {
        Vector3 delta = targetWorldPos - transform.position;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            animator.SetInteger("Direction", delta.x > 0 ? 3 : 2);
        else
            animator.SetInteger("Direction", delta.y > 0 ? 0 : 1);

        animator.SetBool("Recovering", currentState == GhostState.Recovering);
        if (currentState != GhostState.Recovering)
            animator.SetInteger("State", (int)currentState);
    }

    public void SetState(GhostState state, float duration = 0f)
    {
        if (currentState == GhostState.Scared && state != GhostState.Scared)
        {
            GameManager.Instance.GhostReturnedToNormal();
        }

        if (state == GhostState.Scared && currentState != GhostState.Scared)
        {
            GameManager.Instance.GhostBecameScared();
        }

        currentState = state;

        switch(state)
        {
            case GhostState.Scared:
                scaredTimer = duration;
                isRecovering = false;
                animator.SetBool("Recovering", false);
                animator.SetInteger("State", (int)GhostState.Scared);
                canMove = true;
                break;

            case GhostState.Recovering:
                isRecovering = true;
                animator.SetBool("Recovering", true);
                animator.SetInteger("State", (int)GhostState.Recovering);
                break;

            case GhostState.Normal:
                isRecovering = false;
                animator.SetBool("Recovering", false);
                animator.SetInteger("State", (int)GhostState.Normal);
                break;

            case GhostState.Dead:
                isRecovering = false;
                animator.SetBool("Recovering", false);
                animator.SetInteger("State", (int)GhostState.Dead);
                lastGridPosition = spawnCell;
                SetTargetCell(spawnCell);
                canMove = true;
                break;
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
        gridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        transform.position = targetWorldPos;
        lastGridPosition = spawnCell;
        inSpawnArea = true;
        isRecovering = false;

        float remaining = 0f;
        if (GameManager.Instance != null) remaining = GameManager.Instance.GetRemainingGhostTime();
        if (remaining > 0f)
        {
            SetState(GhostState.Scared, remaining);
        }
        else
        {
            SetState(GhostState.Normal);
        }
    }

    public void TeleportToSpawn()
    {
        currentState = GhostState.Normal;
        inSpawnArea = true;
        isMoving = false;
        gridPosition = spawnCell;
        lastGridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        transform.position = targetWorldPos;
        animator.SetInteger("State", (int)currentState);
        UpdateAudioAfterPossibleChange();
    }

    public void StopAndTeleportToSpawn()
    {
        canMove = false;
        isMoving = false;
        gridPosition = spawnCell;
        lastGridPosition = spawnCell;
        targetCell = spawnCell;
        targetWorldPos = levelTilemap.GetCellCenterWorld(spawnCell);
        transform.position = targetWorldPos;
        currentState = GhostState.Normal;
        animator.SetInteger("State", (int)currentState);
        UpdateAudioAfterPossibleChange();
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    private void UpdateAudioAfterPossibleChange()
    {
        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        bool anyDead = false;
        bool anyScared = false;
        foreach (var g in ghosts)
        {
            if (g.currentState == GhostState.Dead) { anyDead = true; break; }
            if (g.currentState == GhostState.Scared || g.currentState == GhostState.Recovering) anyScared = true;
        }

        if (AudioPlayer.Instance == null) return;

        if (anyDead)
        {
            AudioPlayer.Instance.SwitchState(BGMState.Dead);
            return;
        }

        if (anyScared)
        {
            AudioPlayer.Instance.SwitchState(BGMState.Scared);
            return;
        }

        AudioPlayer.Instance.SwitchState(BGMState.Normal);
    }
}
