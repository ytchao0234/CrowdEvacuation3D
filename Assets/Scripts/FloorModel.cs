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

        floor = new GameObject[planeRow, planeCol];

        for (int i = 0; i < planeRow; i ++)
        {
            for (int j = 0; j < planeCol; j ++)
            {
                Vector3 pos = transform.position + (Vector3.back * (i - planeRow / 2) + Vector3.right * (j - planeCol / 2)) * planeSize;
                GameObject obj = GameObject.Instantiate(plane, pos, transform.rotation);
                obj.transform.parent = transform;
                floor[i, j] = obj;
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup()
    {
        GUI gui = FindObjectOfType<GUI>();
        foreach (Vector2Int exit in gui.exitPos)
        {
            floor[exit.x, exit.y].transform.tag = "Exit";
        }
    }
    public void Reset()
    {
        GUI gui = FindObjectOfType<GUI>();

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            floor[i,j].GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public bool isValidCell(Vector2Int cell)
    {
        GUI gui = FindObjectOfType<GUI>();

        bool flg = true;
        flg &= cell.x >= 0 && cell.x < gui.planeRow;
        flg &= cell.y >= 0 && cell.y < gui.planeCol;

        return flg;
    }

    public bool isEmptyCell(Vector2Int cell)
    {
        return floor[cell.x, cell.y].transform.childCount == 0;
    }

    public bool isExitCell(Vector2Int cell)
    {
        return floor[cell.x, cell.y].transform.tag == "Exit";
    }

    public bool isAgentCell(Vector2Int cell)
    {
        bool flg = isEmptyCell(cell);
        if (flg) return !flg;

        return floor[cell.x, cell.y].transform.GetChild(0).tag == "ActiveAgent";
    }

    public bool isObstacleCell(Vector2Int cell)
    {
        bool flg = isEmptyCell(cell);
        if (flg) return !flg;

        flg  = floor[cell.x, cell.y].transform.GetChild(0).tag == "MovableObstacle";
        flg |= floor[cell.x, cell.y].transform.GetChild(0).tag == "ImmovableObstacle";
        flg |= floor[cell.x, cell.y].transform.GetChild(0).tag == "AssignedObstacle";

        return flg;
    }

    public bool isNotImmovableExitCell(Vector2Int cell)
    {
        bool flg;

        flg  = floor[cell.x, cell.y].transform.tag == "Exit";
        flg |= !isEmptyCell(cell) && floor[cell.x, cell.y].transform.GetChild(0).tag == "ImmovableObstacle";

        return !flg;
    }
}
