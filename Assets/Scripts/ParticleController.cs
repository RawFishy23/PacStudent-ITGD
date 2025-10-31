using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [Header("Movement Particles")]
    public ParticleSystem dustParticle;

    [Header("Wall Hit Prefab")]
    public GameObject wallHitPrefab;

    private Vector3Int? lastWallHitTile = null;

    // Play/stop movement particles
    public void PlayMove(bool isMoving, Vector2Int moveDir)
    {
        if (dustParticle == null) return;

        if (isMoving)
        {
            // Rotate the particle according to move direction
            Quaternion rot = Quaternion.identity;
            if (moveDir == Vector2Int.up) rot = Quaternion.Euler(90, 0, 0);
            else if (moveDir == Vector2Int.down) rot = Quaternion.Euler(-90, 0, 0);
            else if (moveDir == Vector2Int.left) rot = Quaternion.Euler(0, 90, 0);
            else if (moveDir == Vector2Int.right) rot = Quaternion.Euler(0, -90, 0);

            dustParticle.transform.rotation = rot;

            if (!dustParticle.isPlaying) dustParticle.Play();
        }
        else
        {
            if (dustParticle.isPlaying) dustParticle.Stop();
        }
    }

    // Spawn wall hit prefab once per tile
    public void PlayWallHit(Vector3Int wallTilePos, Vector3 spawnPos)
    {
        if (lastWallHitTile != null && lastWallHitTile == wallTilePos) return;

        lastWallHitTile = wallTilePos;

        if (wallHitPrefab != null)
        {
            GameObject obj = Instantiate(wallHitPrefab, spawnPos, Quaternion.identity);
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(obj, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(obj, 1f);
        }
    }

    public void ResetWallHit()
    {
        lastWallHitTile = null;
    }
}
