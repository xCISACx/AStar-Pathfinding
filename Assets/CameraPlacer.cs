using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlacer : MonoBehaviour
{
    public Transform Ground;

    public void UpdatePosition(Grid grid, Transform t)
    {
        transform.position = new Vector3(grid.transform.position.x, transform.position.y, grid.transform.position.z)
                             + new Vector3((grid.width / 2f)  * grid.CellSize, 0, (grid.height / 2f) * grid.CellSize);
        
        if (grid.height > grid.width)
        {
            // if height = 200, size = 105. if height = 50, size = 30
            GetComponent<Camera>().orthographicSize = (grid.height / 2f) * (grid.CellSize) + 5f;
        }
        else if (grid.height < grid.width)
        {
            GetComponent<Camera>().orthographicSize = (grid.width / 2f) * (grid.CellSize) + 5f;
        }
        else
        {
            GetComponent<Camera>().orthographicSize = (grid.width / 2f) * (grid.CellSize) + 5f;
        }
        
        Debug.Log(GetComponent<Camera>().orthographicSize);
        
    }
}
