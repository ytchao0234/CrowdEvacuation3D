using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticFloorField : MonoBehaviour
{
    public float[,] sff;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        Vector2Int[] exitPos = gui.exitPos;
        sff = new float[gui.planeRow, gui.planeCol];

        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            sff[i, j] = gui.sff_init_value;
        }

        foreach (Vector2Int exit in exitPos)
        {
            sff[exit.x, exit.y] = 0;
            SetSFF_OneExit(exit);
        }

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            Debug.Log(sff[i, j]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetSFF_OneExit(Vector2Int exitPos)
    {
        GUI gui = FindObjectOfType<GUI>();
        Queue<Vector2Int> toDoList = new Queue<Vector2Int>();
        toDoList.Enqueue(exitPos);

        while (toDoList.Count > 0)
        {
            Vector2Int curCell = toDoList.Dequeue();
            Vector2Int adjCell = curCell;

            for (int i = -1; i <= 1; i++)
			for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                adjCell = curCell + new Vector2Int(i, j);

                if (isValidCell(adjCell))
                {
                    float offset = (i == 0 || j == 0) ? gui.sff_offset_hv : gui.sff_offset_d;

                    if (sff[adjCell.x, adjCell.y] > sff[curCell.x, curCell.y] + offset)
                    {
                        sff[adjCell.x, adjCell.y] = sff[curCell.x, curCell.y] + offset;
                        toDoList.Enqueue(adjCell);
                    }
                }
            }
        }
    }

    bool isValidCell(Vector2Int cell)
    {
        GUI gui = FindObjectOfType<GUI>();

        bool flg = true;
        flg &= cell.x >= 0 && cell.x < gui.planeRow;
        flg &= cell.y >= 0 && cell.y < gui.planeCol;

        return flg;
    }
}
