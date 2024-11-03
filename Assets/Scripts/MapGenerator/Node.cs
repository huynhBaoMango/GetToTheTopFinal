using FishNet.Serializing;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public abstract class Node
{
    private List<Node> childrenNodeList;

    public List<Node> ChildrenNodeList { get => childrenNodeList;}

    public bool Visted { get; set; }
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }

    public Node Parent { get; set; }

    public int TreeLayerIndex { get; set; }

    public NodeType thisMeshType { get; set; }

    public Node(Node parentNode)
    {
        childrenNodeList = new List<Node>();
        this.Parent = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node node)
    {
        childrenNodeList.Add(node);

    }

    public void RemoveChild(Node node)
    {
        childrenNodeList.Remove(node);
    }

    public enum NodeType
    { 
        Room,
        Corridor
    }

    public void Serialize(System.IO.BinaryWriter writer)
    {
        writer.Write(BottomLeftAreaCorner.x);
        writer.Write(BottomLeftAreaCorner.y);
        writer.Write(BottomRightAreaCorner.x);
        writer.Write(BottomRightAreaCorner.y);
        writer.Write(TopRightAreaCorner.x);
        writer.Write(TopRightAreaCorner.y);
        writer.Write(TopLeftAreaCorner.x);
        writer.Write(TopLeftAreaCorner.y);
        writer.Write(TreeLayerIndex);
        writer.Write(Visted);
        writer.Write((byte)thisMeshType);

        // Serialize children
        writer.Write(ChildrenNodeList.Count);
        foreach (Node child in ChildrenNodeList)
        {
            child.Serialize(writer);
        }
    }

    // Deserialize the Node
    //public static Node Deserialize(System.IO.BinaryReader reader, Node parent = null)
    //{
    //    // Create the node without initializing properties
    //    Node node = new Node(parent);

    //    // Now set the properties individually
    //    node.BottomLeftAreaCorner = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
    //    node.BottomRightAreaCorner = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
    //    node.TopRightAreaCorner = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
    //    node.TopLeftAreaCorner = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
    //    node.TreeLayerIndex = reader.ReadInt32();
    //    node.Visted = reader.ReadBoolean();
    //    node.thisMeshType = (NodeType)reader.ReadByte();

    //    // Deserialize children
    //    int childrenCount = reader.ReadInt32();
    //    for (int i = 0; i < childrenCount; i++)
    //    {
    //        Node childNode = Deserialize(reader, node);
    //        node.AddChild(childNode);
    //    }

    //    return node;
    //}

}