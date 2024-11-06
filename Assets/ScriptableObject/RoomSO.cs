using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSO : ScriptableObject
{
    public bool Visted { get; set; }
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }
    public enum NodeType
    {
        Room,
        Corridor
    }
}
