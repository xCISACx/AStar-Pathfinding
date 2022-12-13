using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Mesh mesh;
    
    // The x and y grid coordinates of the node.
    public int gridX;
    public int gridY;

    // The node's position in world space.
    public Vector3 position;

    // Whether the node is passable (i.e. can be traversed by the player).
    public bool walkable;

    public float G; // Distance between the current node and the start node
    public float H; // Estimated distance from the current node to the end node
    public float F; // Total cost H + G

    // Constructor
    public Node(int gridX, int gridY, Vector3 position, bool walkable)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.position = position;
        this.walkable = walkable;
    }
}
