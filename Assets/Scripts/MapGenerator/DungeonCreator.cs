﻿using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FishNet.Managing;
using FishNet.CodeGenerating;
using scgFullBodyController;

public class DungeonCreator : NetworkBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public Material material;

    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;
    public GameObject wallVertical, wallHorizontal, ground;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;

    private readonly SyncList<GameObject> floorList = new SyncList<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        CreateDungeonServerRpc();
    }

    [Button]
    public void CreateDungeon()
    {
        CreateDungeonServerRpc();
    }
    [Server]
    public void CreateDungeonServerRpc()
    {
        //DestroyAllChildren();
        DugeonGenerator generator = new DugeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerMidifier,
            roomOffset,
            corridorWidth);
        
        //GameObject wallParent = new GameObject("WallParent", typeof(NetworkObject));
        //wallParent.transform.parent = transform;
        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].thisMeshType);
        }
        CreateWalls(gameObject);
    }

    private void CreateWalls(GameObject wallParent)
    {
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPosition, wallVertical);
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        NetworkObject wall = NetworkManager.GetPooledInstantiated(wallPrefab, wallPosition, Quaternion.identity, true);
        //GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
        ServerManager.Spawn(wall);
       // wall.transform.parent = wallParent.transform;
    }

    IEnumerator waitToChange(NetworkObject dungeonFloor)
    {
        yield return new WaitForSeconds(0.4f);
        if (dungeonFloor.TryGetComponent(out MeshRenderer renderer)) renderer.material = material;
    }
    [Client]
    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, Node.NodeType thistype)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        //GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(NetworkObject), typeof(NetworkTransform));
        NetworkObject dungeonFloor = NetworkManager.GetPooledInstantiated(ground, true);
        ServerManager.Spawn(dungeonFloor);
        floorList.Add(dungeonFloor.gameObject);
        if (dungeonFloor.TryGetComponent(out MeshFilter filter)) filter.mesh = mesh;
        if (dungeonFloor.TryGetComponent(out MeshRenderer renderer)) renderer.material = material;
        UpdateMeshObserver(dungeonFloor, bottomLeftCorner, topRightCorner);
        //if (dungeonFloor.TryGetComponent(out MeshCollider collider)) collider.sharedMesh = mesh;

        //dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        //dungeonFloor.GetComponent<MeshRenderer>().material = material;
        //dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;
        //dungeonFloor.transform.parent = transform;

        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        if (wallList.Contains(point)){
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }

    void UpdateMeshObserver(NetworkObject dungeonFloor, Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Debug.Log(bottomLeftCorner);
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        if (dungeonFloor.TryGetComponent(out MeshFilter filter))
        {
            filter.mesh.Clear();
            filter.mesh = mesh;
        }
        if (dungeonFloor.TryGetComponent(out MeshRenderer renderer)) renderer.material = material;
    }

    private void DestroyAllChildren()
    {
        // Lưu danh sách GameObject con
        List<GameObject> children = new List<GameObject>();

        // Lưu tất cả GameObject con vào danh sách
        if (children != null)
        {
            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
            }
        }

        // Duyệt qua danh sách và xóa từng GameObject
        foreach (GameObject child in children)
        {
            if (child.TryGetComponent<NetworkObject>(out var networkObject))
            {
                ServerManager.Despawn(networkObject);
                Destroy(child);
            }
            else
            {
                Destroy(child); // Nếu không có NetworkObject, có thể dùng Destroy
            }
        }
    }

}
