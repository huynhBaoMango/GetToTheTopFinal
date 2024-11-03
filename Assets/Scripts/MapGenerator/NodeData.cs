using System.Collections.Generic;
using System;
using UnityEngine;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Object.Synchronizing;


public sealed class NodeData : SyncBase
{
    private readonly Vector2Int BottomLeftAreaCorner;
    public Vector2Int BottomRightAreaCorner;
    public Vector2Int TopRightAreaCorner;
    public Vector2Int TopLeftAreaCorner;
    public Node.NodeType thisMeshType; // Hoặc tên khác nếu cần
    public List<NodeData> Children; // Danh sách các Node con

    public NodeData()
    {
        Children = new List<NodeData>();
    }
}