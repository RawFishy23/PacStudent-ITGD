using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public Transform levelContainer;

    public int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    void Start()
    {
        GenerateFullLevel();
    }

    int[,] BuildFullMap(int[,] quadrant)
    {
        int rows = quadrant.GetLength(0);
        int cols = quadrant.GetLength(1);
        int fullRows = rows * 2 - 1;
        int fullCols = cols * 2;

        int[,] fullMap = new int[fullRows, fullCols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int val = quadrant[y, x];
                fullMap[y, x] = val;
                fullMap[y, fullCols - 1 - x] = val;
                if (y < rows - 1)
                    fullMap[fullRows - 1 - y, x] = val;
                if (y < rows - 1)
                    fullMap[fullRows - 1 - y, fullCols - 1 - x] = val;
            }
        }

        return fullMap;
    }

    void GenerateFullLevel()
    {
        foreach (Transform child in levelContainer)
            Destroy(child.gameObject);

        int[,] fullMap = BuildFullMap(levelMap);
        int rows = fullMap.GetLength(0);
        int cols = fullMap.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileID = fullMap[y, x];
                if (tileID < 0 || tileID >= tilePrefabs.Length) continue;

                Vector3 pos = new Vector3(x, -y, 0);
                GameObject tile = Instantiate(tilePrefabs[tileID], pos, Quaternion.identity, levelContainer);

                int rotation = GetRotationForTile(fullMap, x, y, tileID);
                Vector3 scale = Vector3.one;

                int fullCols = fullMap.GetLength(1);
                if (tileID == 7 && x >= fullCols / 2)
                {
                    scale.x = -1f;
                }

                tile.transform.localScale = scale;
                tile.transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
        }
    }

    int GetRotationForTile(int[,] map, int x, int y, int tileID)
    {
        bool up = IsWall(map, y - 1, x);
        bool down = IsWall(map, y + 1, x);
        bool left = IsWall(map, y, x - 1);
        bool right = IsWall(map, y, x + 1);

        if (tileID == 1)
        {
            if (down && right) return 0;
            if (down && left) return 270;
            if (up && left) return 180;
            if (up && right) return 90;
        }

        if (tileID == 2)
        {
            if (up && down) return 90;
            if (left && right) return 0;
        }

        if (tileID == 3)
        {
            if (down && right) return 0;
            if (up && right) return 90;
            if (up && left) return 180;
            if (down && left) return 270;
        }

        if (tileID == 4)
        {
            if (up && down) return 90;
            if (left && right) return 0;
        }

        if (tileID == 7)
        {
            if (!up) return 0;
            if (!right) return 90;
            if (!down) return 180;
            if (!left) return 270;
        }

        if (tileID == 8)
        {
            if (left && right) return 0;
            if (up && down) return 90;
        }

        return 0;
    }

    bool IsWall(int[,] map, int y, int x)
    {
        if (y < 0 || y >= map.GetLength(0) || x < 0 || x >= map.GetLength(1))
            return false;

        int id = map[y, x];
        return (id == 1 || id == 2 || id == 3 || id == 4 || id == 7 || id == 8);
    }
}
