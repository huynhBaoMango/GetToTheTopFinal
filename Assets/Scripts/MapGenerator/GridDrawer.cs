using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class GridDrawer : MonoBehaviour
{
    public float cellSize = 1f; // Kích thước mỗi ô của grid
    public GameObject[] prefabsToPlace; // Mảng các prefab để đặt vào grid
    public int maxAttempts = 100; // Số lần thử để tìm vị trí đặt prefab

    private List<Vector3> occupiedCells = new List<Vector3>(); // Danh sách các ô đã được đặt

    void OnDrawGizmos()
    {
        // Lấy Renderer từ GameObject này
        Renderer meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            // Lấy kích thước của mesh
            Vector3 size = meshRenderer.bounds.size;

            // Tính số ô trong grid
            int gridLength = Mathf.CeilToInt(size.z / cellSize);
            int gridWidth = Mathf.CeilToInt(size.x / cellSize);

            // Tính toán vị trí bắt đầu của grid
            Vector3 startPosition = meshRenderer.bounds.min;

            // Vẽ grid
            Gizmos.color = Color.green;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridLength; z++)
                {
                    Vector3 cellPosition = startPosition + new Vector3(x * cellSize, 0, z * cellSize);
                    Gizmos.DrawWireCube(cellPosition + new Vector3(cellSize / 2, 0, cellSize / 2), new Vector3(cellSize, 0.1f, cellSize));

                    // Kiểm tra xem ô có bị chiếm chưa
                    if (occupiedCells.Contains(cellPosition))
                    {
                        Gizmos.color = Color.red; // Đánh dấu ô đã bị chiếm
                        Gizmos.DrawWireCube(cellPosition + new Vector3(cellSize / 2, 0, cellSize / 2), new Vector3(cellSize, 0.1f, cellSize));
                    }
                }
            }
        }
    }

    [Button]
    public void PlaceRandomPrefabs()
    {
        // Chọn ngẫu nhiên một prefab
        GameObject prefab = prefabsToPlace[Random.Range(0, prefabsToPlace.Length)];
        Vector2 prefabSize = new Vector2(prefab.GetComponent<Renderer>().bounds.size.x, prefab.GetComponent<Renderer>().bounds.size.z) / cellSize;

        int width = Mathf.RoundToInt(prefabSize.x);
        int length = Mathf.RoundToInt(prefabSize.y);

        // Thử đặt prefab tối đa maxAttempts lần
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomX = Random.Range(0, Mathf.CeilToInt(width));
            int randomZ = Random.Range(0, Mathf.CeilToInt(length));

            // Tính toán vị trí đặt
            Vector3 positionToPlace = new Vector3(randomX * cellSize, 0, randomZ * cellSize) + (Vector3)transform.position;

            // Kiểm tra xem có thể đặt prefab ở vị trí này không
            if (CanPlacePrefab(positionToPlace, width, length))
            {
                // Đặt prefab
                GameObject instance = Instantiate(prefab, positionToPlace + new Vector3(cellSize / 2, 0, cellSize / 2), Quaternion.identity);

                // Đánh dấu các ô đã bị chiếm
                MarkOccupiedCells(positionToPlace, width, length);

                break; // Thoát khỏi vòng lặp nếu đã đặt thành công
            }
        }
    }

    // Kiểm tra xem prefab có thể đặt vào ô không
    private bool CanPlacePrefab(Vector3 position, int width, int length)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                Vector3 cellPosition = position + new Vector3(x * cellSize, 0, z * cellSize);
                if (occupiedCells.Contains(cellPosition))
                {
                    return false; // Nếu có ô bị chiếm, không thể đặt
                }
            }
        }
        return true; // Có thể đặt
    }

    // Đánh dấu các ô đã chiếm
    private void MarkOccupiedCells(Vector3 position, int width, int length)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                Vector3 cellPosition = position + new Vector3(x * cellSize, 0, z * cellSize);
                occupiedCells.Add(cellPosition); // Thêm ô vào danh sách đã chiếm
            }
        }
    }
}
