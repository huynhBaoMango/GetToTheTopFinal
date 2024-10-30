using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    int width, height;

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
        for(int x = pos.x; x <= pos.x + newProp.x -1; x++)
        {
            for (int y = pos.y; y <= pos.y + newProp.y - 1; y++)
            {
                if (!tilesMatrix[x,y].GetComponent<tileInfo>().isEmpty || tilesMatrix[x, y].GetComponent<tileInfo>() == null) isOkay = false;
            }
        }

        if (isOkay)
        {
            Transform stile = tilesMatrix[pos.x, pos.y].transform;
            Vector3 cornerPos = new Vector3(stile.position.x - 0.5f, stile.position.y, stile.position.z - 0.5f);
            Instantiate(newProp, cornerPos, Quaternion.identity, gameObject.transform);
            for (int x = pos.x; x <= pos.x + newProp.x - 1; x++)
            {
                for (int y = pos.y; y <= pos.y + newProp.y - 1; y++)
                {
                    tilesMatrix[x,y].GetComponentInParent<tileInfo>().isEmpty = false;
                    tilesMatrix[x, y].GetComponent<Renderer>().material = selectedMaterial;
                }
            }
            return true;
        }
        return false;
    }
}
