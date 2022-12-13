using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.WSA;
using Application = UnityEngine.WSA.Application;

public class Grid : MonoBehaviour
{
    [Range(1, 100)]
    public int width;
    [Range(1, 100)]
    public int height;
    
    private Node[,] _nodes;

    [Range(1, 5)]
    public int CellSize;

    public float GroundY = 0.1f;
    [SerializeField] private float PathY = 0.2f;

    public Color GroundColour = new(0.00000f, 0.39608f, 0.12549f);

    public GameObject Ground;
    public CameraPlacer Camera;
    public PlayerUnit Unit;

    public bool ShowGrid;

    public Transform GridQuadParent;

    public Transform PathParent;
    public Transform ObstacleParent;

    public LayerMask ObstacleMask;

    public Material GroundMaterial;
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    private void Awake()
    {
        CreateGrid();

        /*if (ShowGrid)
        {
            DrawGrid();
        }*/

        DrawGridQuads();

        Camera.UpdatePosition(this, Ground.transform);
        Unit = FindObjectOfType<PlayerUnit>();
        
        Unit.UpdatePosition(NodeFromWorldPoint(Unit.transform.position).position, this);

        Debug.Log(_nodes.Length);
    }

    private void OnValidate()
    {
        SetGroundValues();
        Camera.UpdatePosition(this, Ground.transform);
        Unit.UpdateWorldPosition(this);
    }
    

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void CreateGrid()
    {
        _nodes = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Use check box to mark nodes obstructed by obstacles as un-walkable
                var walkable = !Physics.CheckBox(GetNodeWorldPosition(x, y), Vector3.one * CellSize/2f,
                    Quaternion.identity, ObstacleMask);

                // Create a new Node at the grid position
                _nodes[x, y] = new Node(x, y, GetNodeWorldPosition(x, y), walkable);
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Calculate the coordinates of the nodes to the right, left, up and down of the current node
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // If the adjacent node is inside the grid limits and is walkable, add it to the list of neighbors.
                
                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height && _nodes[checkX, checkY].walkable)
                {
                    neighbors.Add(_nodes[checkX, checkY]);
                }
            }
        }
        
        return neighbors;
    }

    // Get the node's grid position through its world coordinates
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // Calculate the coordinates of the node in the grid by dividing its world position by the cell size

        int x = Mathf.RoundToInt(worldPosition.x / CellSize); // for node(10, 10), cell size of 2 : x = 10 / 2 = 5 
        int y = Mathf.RoundToInt(worldPosition.z / CellSize);

        Debug.Log("x: " + x + " | y: " + y);

        return _nodes[x, y];
    }
    
    // Get the node's world position through its coordinates on the grid
    public Vector3 GetNodeWorldPosition(int x, int y)
    {
        // Calculate the world position of the node by multiplying its grid values by the cell size
        Vector3 worldPosition = new Vector3(x, 0, y) * CellSize;

        return worldPosition;
    }

    private void SetGroundValues()
    {
        Ground.transform.position = new Vector3(transform.position.x - CellSize / 2f, transform.position.y, transform.position.z - CellSize / 2f)
                             + new Vector3((width / 2f)  * CellSize, 0, (height / 2f) * CellSize);
        //Ground.transform.localPosition = new Vector3((width / 2f) - CellSize / 2f + width * 2, transform.position.y, (height / 2f) - CellSize / 2f + height * 2);
        Ground.transform.localScale = new Vector3((width / 10f) * CellSize, 1f, (height / 10f) * CellSize);
        Ground.transform.localScale *= 1.01f;

        var obstacles = ObstacleParent.GetComponentsInChildren<Obstacle>();
        
        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].transform.position = new Vector3(obstacles[i].DefaultPosition.x * CellSize, obstacles[i].DefaultPosition.y,
                obstacles[i].DefaultPosition.z * CellSize);
            obstacles[i].transform.localScale = new Vector3(obstacles[i].DefaultScale.x * CellSize, 1f, obstacles[i].DefaultScale.z * CellSize);
        }
    }

    public void DrawGridQuadAtPosition(int x, int y, float groundY, Color colour, Transform parent, bool useCellSize)
    {
        // Spawn quads for drawing the path
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // Create a new MaterialPropertyBlock and specify the material we want to modify
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material = GroundMaterial;

        // Set the MaterialPropertyBlock properties to what we want
        block.SetColor(Color1, colour);

        // Apply the changes to the material by passing the MaterialPropertyBlock to the renderer's SetPropertyBlock method
        renderer.SetPropertyBlock(block);

        renderer.shadowCastingMode = ShadowCastingMode.Off;

        quad.transform.SetParent(parent);
        
        if (useCellSize)
        {
            quad.transform.position = new Vector3(x * CellSize, groundY, y * CellSize);
        }
        else
        {
            quad.transform.position = new Vector3(x, groundY, y);
        }
        
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(CellSize/1.2f, CellSize/1.2f, CellSize/1.2f);
        quad.layer = LayerMask.NameToLayer("Quad");

    }

    // Draw a quad on each node for displaying its walk-ability in colour
    public void DrawGridQuads()
    {
        foreach (var n in _nodes)
        {
            var colour = n.walkable ? GroundColour : Color.red;

            DrawGridQuadAtPosition(n.gridX, n.gridY, GroundY, colour, GridQuadParent, true);
        }
    }
    
    // Delete all generated grid quads

    public void DeleteGridQuads()
    {
        if (GridQuadParent.childCount > 0)
        {
            for (int i = 0; i < GridQuadParent.childCount; i++)
            {
                var child = GridQuadParent.GetChild(i);
                Destroy(child.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!UnityEngine.Application.isPlaying)
        {
            return;
        }

        if (_nodes.Length > 0)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPosition = new Vector3(x, 0, y) * CellSize;

                    // If the node is walkable, set the gizmo colour to white.
                    
                    if (_nodes[x, y].walkable)
                    {
                        Gizmos.color = Color.white;
                    }
                    
                    // If the node is not walkable, set the gizmo colour to red.
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    
                    // Draw a coloured wire sphere at its world position
                    Gizmos.DrawWireSphere(worldPosition, CellSize / 4f);
                }
            }   
        }
    }

    /*public void DrawGrid()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = transform.right;

        Vector3 origin = new Vector3((CellSize / 2f + CellSize/2f) + transform.position.x, 0, (CellSize / 2f + CellSize/2f) + transform.position.z);

        // Draw the horizontal grid lines
        for (int y = 0; y <= height; y++)
        {
            Vector3 start =
                origin - new Vector3(CellSize, 0, 0); // position of the origin - half a cell on the z
            start.z += (y - CellSize/2f) * CellSize; // gradually increase the Y position taking into the account cell size
            Vector3 end = start + right * width * CellSize; //direction of the lines
            
            Debug.DrawLine(start, end, Color.blue, Mathf.Infinity, false);
        }

        // Draw the vertical grid lines
        for (int x = 0; x <= width; x++)
        {
            Vector3 start =
                origin - new Vector3(0, 0, CellSize); // position of the origin - half a cell on the x
            start.x += (x - CellSize/2f) * CellSize; // gradually increase the Y position taking into the account cell size
            Vector3 end = start + forward * height * CellSize; //direction of the lines
            
            Debug.DrawLine(start, end, Color.blue, Mathf.Infinity, false);
        }
    }*/

    public void DrawPath()
    {
        if (_nodes != null)
        {
            var pathToFollow = GetComponent<Pathfinding>().PathToFollow;
            var unit = GetComponent<Pathfinding>().Unit;

            foreach (Node n in _nodes)
            {
                if (pathToFollow != null)
                {
                    if (pathToFollow.Contains(n))
                    {
                        DrawGridQuadAtPosition(n.gridX, n.gridY, GroundY, Color.yellow, PathParent, true);
                    }
                }
            }
        }
    }
}
