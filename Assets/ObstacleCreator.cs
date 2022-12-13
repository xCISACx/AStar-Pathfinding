using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCreator : MonoBehaviour
{
    public LayerMask QuadLayer;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            bool canDestroy = true;

            if (Physics.Raycast(ray, out hit, 100, QuadLayer))
            {
                Debug.Log(hit.transform.name);
                Debug.Log("hit");
                Grid grid = GetComponent<Grid>();
                Node node = GetComponent<Grid>().NodeFromWorldPoint(hit.point);

                if (node.walkable)
                {
                    node.walkable = false;
                    
                    // delete clicked plane and redraw only that plane as new

                    if (canDestroy)
                    {
                        Destroy(hit.transform.gameObject);
                        canDestroy = false;
                    }

                    var nodeX = node.position.x;
                    var nodeY = node.position.z;
                
                    grid.DrawGridQuadAtPosition((int) nodeX, (int) nodeY, grid.GroundY, Color.red, grid.GridQuadParent, false);

                    canDestroy = true;
                }
            }
        }
        
        if (Input.GetMouseButton(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            bool canDestroy = true;

            if (Physics.Raycast(ray, out hit, 100, QuadLayer))
            {
                Debug.Log(hit.transform.name);
                Debug.Log("hit");
                Grid grid = GetComponent<Grid>();
                Node node = GetComponent<Grid>().NodeFromWorldPoint(hit.point);
                
                if (!node.walkable)
                {
                    node.walkable = true;
                    
                    // delete clicked plane and redraw only that plane as new
                
                    if (canDestroy)
                    {
                        Destroy(hit.transform.gameObject);
                        canDestroy = false;
                    }

                    var nodeX = node.position.x;
                    var nodeY = node.position.z;
                
                    grid.DrawGridQuadAtPosition((int) nodeX, (int) nodeY, grid.GroundY, grid.GroundColour, grid.GridQuadParent, false);

                    canDestroy = true;
                }
            }
        }
    }
}
