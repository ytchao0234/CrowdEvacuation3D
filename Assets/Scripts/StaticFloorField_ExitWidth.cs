using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticFloorField_ExitWidth : MonoBehaviour
{
    public float[,] sff_e;
    float max_value;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        sff_e = new float[gui.planeRow, gui.planeCol];
        //Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        GUI gui = FindObjectOfType<GUI>();
        Vector2Int[] exitPos = gui.exitPos;
        int[] exitWidth = gui.exitWidth;
        Reset();
        for (int i = 0; i < exitPos.Length; i++)
        {
            sff_e[exitPos[i].x, exitPos[i].y] = 0;
            SetSFFE_OneExit(exitPos[i], exitWidth[i]);
        }
        foreach(float value in sff_e)
        {
            if(value < gui.sff_init_value && value > max_value)
                max_value = value;
        }
        for(int i=0;i<gui.planeRow;i++)
        for(int j=0;j<gui.planeCol;j++)
        {
            if(sff_e[i,j] >= gui.sff_init_value)
                sff_e[i,j] = max_value;
        }
    }

    public void Reset()
    {
        GUI gui = FindObjectOfType<GUI>();
        max_value = 0f;
        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            sff_e[i, j] = gui.sff_init_value;
        }

    }

    void SetSFFE_OneExit(Vector2Int exitPos, int exitWidth)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorField ff = FindObjectOfType<FloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        float offset_hv = Mathf.Exp(-1.0f * exitWidth / gui.totalExitWidth);
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

                if (ff.isValidCell(adjCell) && !fm.isObstacleCell(adjCell))
                {
                    float offset = (i == 0 || j == 0) ? offset_hv : offset_d;

                    if (sff_e[adjCell.x, adjCell.y] > sff_e[curCell.x, curCell.y] + offset)
                    {
                        sff_e[adjCell.x, adjCell.y] = sff_e[curCell.x, curCell.y] + offset;
                        toDoList.Enqueue(adjCell);
                    }
                }
            }
        }
    }
}
