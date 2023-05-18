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
        sff = new float[gui.planeRow, gui.planeCol];

        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        Reset();
    }

    public void Reset()
    {
        GUI gui = FindObjectOfType<GUI>();
        Vector2Int[] exitPos = gui.exitPos;
        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            sff[i, j] = gui.sff_init_value;
        }

        for (int i = 0; i < exitPos.Length; i++)
        {
            sff[exitPos[i].x, exitPos[i].y] = 0;
            SetSFF_OneExit(exitPos[i]);
        }
    }

    void SetSFF_OneExit(Vector2Int exitPos)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorField ff = FindObjectOfType<FloorField>();

        float offset_hv = gui.sff_offset_hv;
        float offset_d = offset_hv * gui.sff_offset_lambda;

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

                if (ff.isValidCell(adjCell))
                {
                    float offset = (i == 0 || j == 0) ? offset_hv : offset_d;

                    if (sff[adjCell.x, adjCell.y] > sff[curCell.x, curCell.y] + offset)
                    {
                        sff[adjCell.x, adjCell.y] = sff[curCell.x, curCell.y] + offset;
                        toDoList.Enqueue(adjCell);
                    }
                }
            }
        }
    }
}
