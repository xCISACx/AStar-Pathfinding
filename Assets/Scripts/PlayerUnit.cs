using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    public Grid Grid;
    public int TargetIndex = 0;
    [SerializeField] private int _speed = 10;
    public int DefaultSpeed = 10;
    public Vector3 DefaultPosition;
    public float DefaultY = 1.1f;
    public bool Moving = false;
    public List<Vector3> PathPositions;

    private void Awake()
    {
        Grid = FindObjectOfType<Grid>();
    }

    public IEnumerator FollowPath(List<Node> path)
    {
        foreach (var node in path)
        {
            PathPositions.Add(node.position);
        }

        Vector3 currentWaypoint = path[0].position;
        
        while (Moving) 
        {
            if (transform.position == new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z))
            {
                TargetIndex++;
                
                if (TargetIndex >= path.Count)
                {
                    Moving = false;
                    
                    if (Grid.PathParent.childCount > 0)
                    {
                        for (int i = 0; i < Grid.PathParent.childCount; i++)
                        {
                            var child = Grid.PathParent.GetChild(i);
                            Destroy(child.gameObject);
                        }
                    }
                    
                    path.Clear();
                    yield break;
                }
                
                currentWaypoint = path[TargetIndex].position;
            }

            transform.position = Vector3.MoveTowards(transform.position,new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z),_speed * Time.deltaTime);
            yield return null;

        }
    }

    public void UpdatePosition(Vector3 pos, Grid grid)
    {
        transform.localScale = new Vector3(grid.CellSize, grid.CellSize, grid.CellSize);
        _speed = DefaultSpeed * grid.CellSize;
    }
    
    public void UpdateWorldPosition(Grid grid)
    {
        transform.position = new Vector3(DefaultPosition.x * grid.CellSize, DefaultY * grid.CellSize, DefaultPosition.z * grid.CellSize);
        transform.localScale = new Vector3(grid.CellSize, grid.CellSize, grid.CellSize);
        _speed = DefaultSpeed * grid.CellSize;
    }
}
