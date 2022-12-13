using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // The grid is the game map on which the units move.
    private Grid _grid;

    // The start node is the starting position of the unit.
    private Node _startNode;

    // The target node is the destination that the unit is trying to reach.
    private Node _targetNode;

    // The open set is a list of nodes that have been discovered but not yet evaluated.
    private List<Node> _openSet;

    // The closed set is a list of nodes that have been evaluated.
    private List<Node> _closedSet;

    // The came from dictionary is used to store the previous node in the path for each node.
    private Dictionary<Node, Node> _cameFrom;

    // The g score is the cost of getting from the start node to the current node.
    private Dictionary<Node, float> _gScore;

    // The f score is the estimated cost of getting from the start node to the target node through the current node.
    private Dictionary<Node, float> _fScore;

    public Vector3 MousePosition;
    
    public List<Node> PathToFollow;
    
    public List<Vector3> PathPositions;
    public LineRenderer ALineRenderer;

    public PlayerUnit Unit;

    private Coroutine _findPathCoroutine;
    private Coroutine _followPathCoroutine;
    
    void Awake()
    {
        _grid = GetComponent<Grid>();
        PathPositions = new List<Vector3>();
    }

    private void Update()
    {
        // If the unit isn't already moving and we press left mouse button
        if (Input.GetMouseButtonDown(0) && !Unit.Moving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Raycast to the mouse position and save the hit point as the mouse position if we hit

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log("hit");
                MousePosition = hit.point;
            }
            
            // Stop the already running co-routine if one exists and start a new one
            /*StopCoroutine(FindPath());
            StartCoroutine(FindPath());*/
            
            if (_findPathCoroutine != null)
            {
                StopCoroutine(_findPathCoroutine);   
            }
            _findPathCoroutine = StartCoroutine(FindPath());
            
            // If we have a path to follow, reset the Unit's target index and set it to moving

            if (PathToFollow.Count > 0)
            {
                Unit.TargetIndex = 0;
                Unit.Moving = true;
                //StartCoroutine(Unit.FollowPath(PathToFollow));
                if (_followPathCoroutine != null)
                {
                    StopCoroutine(_followPathCoroutine);   
                }
                
                _followPathCoroutine = StartCoroutine(Unit.FollowPath(PathToFollow));
                
                //StopCoroutine(Unit.FollowPath(PathToFollow));
                //StartCoroutine(Unit.FollowPath(PathToFollow));
            }
            else
            {
                Debug.Log("No path found!");
            }
        }
    }
    
    IEnumerator FindPath()
    {
        FindPath(Unit.transform.position, MousePosition);
        yield return null;
    }
    
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        
        _startNode = _grid.NodeFromWorldPoint(startPos);
        _targetNode = _grid.NodeFromWorldPoint(targetPos);
        _openSet = new List<Node> { _startNode };
        _closedSet = new List<Node>();
        
        // Clear any previous positions from our path positions list.
        
        PathPositions.Clear();
        
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // Create a dictionary to keep track of what node came before each node.
        
        _cameFrom = new Dictionary<Node, Node>();
        
        // Set up the start node's costs.
        
        _startNode.G = 0;
        _startNode.F = HCost(_startNode, _targetNode);

        // While the open set is not empty,
        while (_openSet.Count > 0)
        {
            // Get the node in the open set with the lowest f score.
            Node currentNode = _openSet.OrderBy(node => node.F).First();

            // If the current node is the target node, we have reached our destination
            if (currentNode == _targetNode)
            {
                stopwatch.Stop();
                Debug.Log("Pathfinding took: " + stopwatch.ElapsedMilliseconds + "ms");
                return ReconstructPath(_cameFrom, currentNode);
            }

            // Remove the current node from the open set and add it to the closed set.
            _openSet.Remove(currentNode);
            _closedSet.Add(currentNode);
            
            foreach (Node neighbour in _grid.GetNeighbors(currentNode))
            {
                // If the neighbor is in the closed set, skip it since we already scanned it.
                if (_closedSet.Contains(neighbour))
                {
                    continue;
                }

                // Calculate the tentative G score for getting to the neighbor from the current node.
                float tentativeGScore = currentNode.G + HCost(currentNode, neighbour);

                // If the neighbor is not in the open set, or the tentative g score is lower than the current g score,
                if (!_openSet.Contains(neighbour) || tentativeGScore < neighbour.G)
                {
                    // Update the came from dictionary and set the neighbour's values
                    _cameFrom[neighbour] = currentNode;
                    neighbour.G = tentativeGScore;
                    neighbour.F = neighbour.G + HCost(neighbour, _targetNode);

                    // If the neighbor is not in the open set, add it.
                    if (!_openSet.Contains(neighbour))
                    {
                        _openSet.Add(neighbour);
                    }
                }
            }
        }
        
        // If no path is found, return an empty list.
        return new List<Node>();
    }

    // Calculate the estimated cost of getting from the start node to the target node using the Manhattan distance.
    private float HCost(Node startNode, Node targetNode)
    {
        int distanceX = Mathf.Abs(startNode.gridX - targetNode.gridX);
        int distanceY = Mathf.Abs(startNode.gridY - targetNode.gridY);
        return distanceX + distanceY;
    }

    // Reconstruct the path from the start node to the end node.
    
    private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node currentNode)
    {
        List<Node> path = new List<Node>();
        
        // Go through the path starting at the end.
        
        while (currentNode != _startNode)
        {
            // Set the current node to the node the current node came from in order to trace the path from end to start.
            
            path.Add(currentNode);
            currentNode = cameFrom[currentNode];
        }

        // Reverse the path so we can follow it from start to finish instead.
        
        path.Reverse();
        
        PathToFollow = path;
        
        for (var index = 0; index < PathToFollow.Count; index++)
        {
            var node = PathToFollow[index];
            
            PathPositions.Add(new Vector3(node.position.x, 1.2f, node.position.z));
            /*ALineRenderer.positionCount = PathPositions.Count;
            ALineRenderer.SetPositions(PathPositions.ToArray());*/
        }
        
        _grid.DrawPath();
        
        return path;
    }
}
