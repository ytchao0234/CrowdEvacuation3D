using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorModel : MonoBehaviour
{
    public GameObject plane;
    public GameObject[,] floor;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
    
        int planeRow = gui.planeRow;
        int planeCol = gui.planeCol;
        float planeSize = plane.transform.GetComponent<Renderer>().bounds.size.x;
        Debug.Log("planeSize: " + planeSize);

        floor = new GameObject[planeRow, planeCol];

        for (int i = 0; i < planeRow; i ++)
        {
            for (int j = 0; j < planeCol; j ++)
            {
                Vector3 pos = transform.position + (Vector3.right * i + Vector3.forward * j) * planeSize;
                GameObject obj = GameObject.Instantiate(plane, pos, transform.rotation);
                obj.transform.parent = transform;
                floor[i, j] = obj;
            }
        }

        foreach (Vector2Int exit in gui.exitPos)
        {
            floor[exit.x, exit.y].transform.tag = "Exit";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isEmptyCell(Vector2Int cell)
    {
        return floor[cell.x, cell.y].transform.childCount == 0;
    }

    public bool isExitCell(Vector2Int cell)
    {
        return floor[cell.x, cell.y].transform.tag == "Exit";
    }
    public bool isValidCell(Vector2Int cell)
    {
        bool flg = isEmptyCell(cell);
        if (flg) return flg;

        flg  = floor[cell.x, cell.y].transform.GetChild(0).tag != "MovableObstacle";
        flg &= floor[cell.x, cell.y].transform.GetChild(0).tag != "ImmovableObstacle";

        return flg;
    }
}
