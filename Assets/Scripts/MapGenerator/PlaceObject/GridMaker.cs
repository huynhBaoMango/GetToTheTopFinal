using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    public int width, height;

    public tileInfo tilePrefab; // prefab cho tile
    public float tileSize = 1f; // kích thước của tile
    public GameObject[,] tilesMatrix;
    public List<tileInfo> tiles;

    public List<PropInfo> props;
    private Material selectedMaterial;

    void Start()
    {
        tilePrefab = Resources.Load<tileInfo>("TilePrefab");
        selectedMaterial = Resources.Load<Material>("LMaterial");
        props = Resources.LoadAll<PropInfo>("PropPrefab").ToList();
        tiles = new List<tileInfo>();


        if (tilePrefab != null)
        {
            CreateGrid();
            PlaceRandomObject();
        }
        else
        {
            Debug.Log("no tile");
        }
        
    }

    void CreateGrid()
    {
        // Lấy kích thước của GameObject chính
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 objectSize = renderer.bounds.size;

            // Tính toán số lượng tile theo kích thước của GameObject chính
            width = Mathf.CeilToInt(objectSize.x / tileSize);
            height = Mathf.CeilToInt(objectSize.z / tileSize);
            tilesMatrix = new GameObject[width, height];
            // Tính toán vị trí bắt đầu từ góc dưới bên trái của GameObject chính
            Vector3 startPosition = renderer.bounds.min;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    // Tính toán vị trí của từng tile
                    Vector3 tilePosition = startPosition + new Vector3(x * tileSize +0.5f, 0, z * tileSize + 0.5f);
                    tileInfo newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, gameObject.transform);
                    newTile.x = x;
                    newTile.y = z;
                    newTile.isEmpty = true;
                    tiles.Add(newTile);
                    tilesMatrix[x,z] = newTile.gameObject;
                }
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy Renderer trên GameObject chính.");
        }
    }

    void PlaceRandomObject()
    {
        int objectCount = 0;
        while(objectCount < 10)
        {
            Vector2Int randomPos = GetRandomPosOnMatrix();
            if (CheckIfValidToSpawn(randomPos)) objectCount++;
        }
    }

    Vector2Int GetRandomPosOnMatrix()
    {
        Vector2Int tempPos;
        do
        {
            tempPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (!tilesMatrix[tempPos.x, tempPos.y].GetComponent<tileInfo>().isEmpty);

        return tempPos;
    }

    bool CheckIfValidToSpawn(Vector2Int pos)
    {
        PropInfo newProp = props[Random.Range(0, props.Count)];
        bool isOkay = true;
        if (pos.x + newProp.x > width || pos.y + newProp.y > height)
        {
            isOkay = false;
        }
        else
        {
            for (int x = pos.x; x <= pos.x + newProp.x; x++)
            {
                for (int y = pos.y; y <= pos.y + newProp.y; y++)
                {
                    if (!tilesMatrix[x, y].GetComponent<tileInfo>().isEmpty || tilesMatrix[x, y].GetComponent<tileInfo>() == null) isOkay = false;
                }
            }
        }
        

        if (isOkay)
        {
            Transform stile = tilesMatrix[pos.x, pos.y].transform;
            Vector3 cornerPos = new Vector3(stile.position.x - 0.5f, stile.position.y, stile.position.z - 0.5f);
            Instantiate(newProp, cornerPos, Quaternion.identity, gameObject.transform);
            for (int x = pos.x; x <= pos.x + newProp.x; x++)
            {
                for (int y = pos.y; y <= pos.y + newProp.y; y++)
                {
                    tilesMatrix[x,y].GetComponentInParent<tileInfo>().isEmpty = false;
                    //tilesMatrix[x, y].GetComponent<Renderer>().material = selectedMaterial;
                }
            }
            return true;
        }
        return false;
    }

    void PlaceRandomObject1()
    {
        List<Vector2Int> positions = GetPositionsFromOutsideToInside();

        int objectCount = 0;
        foreach (Vector2Int pos in positions)
        {
            if (CheckIfValidToSpawn(pos))
            {
                objectCount++;
                if (objectCount >= 10) break; // Dừng lại khi đã đặt đủ 10 vật thể
            }
        }
    }

    List<Vector2Int> GetPositionsFromOutsideToInside()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int layer = 0; layer < (width + height) / 2; layer++)
        {
            // Duyệt theo các cạnh của hình chữ nhật từ ngoài vào trong
            for (int x = layer; x < width - layer; x++)
            {
                positions.Add(new Vector2Int(x, layer)); // Top edge
                if (layer != height - layer - 1) // tránh lặp lại hàng dưới
                    positions.Add(new Vector2Int(x, height - layer - 1)); // Bottom edge
            }
            for (int y = layer + 1; y < height - layer - 1; y++)
            {
                positions.Add(new Vector2Int(layer, y)); // Left edge
                if (layer != width - layer - 1) // tránh lặp lại cột phải
                    positions.Add(new Vector2Int(width - layer - 1, y)); // Right edge
            }
        }

        return positions;
    }
}



