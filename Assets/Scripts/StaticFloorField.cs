using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticFloorField : MonoBehaviour
{
    public float[,] sff;
    float max_value;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        sff = new float[gui.planeRow, gui.planeCol];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        Reset();
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        
        Vector2Int[] exitPos = gui.exitPos;
        for (int i = 0; i < exitPos.Length; i++)
        {
            sff[exitPos[i].x, exitPos[i].y] = 0;
            SetSFF_OneExit(exitPos[i]);
        }
        foreach(float value in sff)
        {
            if(value < gui.sff_init_value && value > max_value)
                max_value = value;
        }
        for(int i=0;i<gui.planeRow;i++)
        for(int j=0;j<gui.planeCol;j++)
        {
            if(fm.isObstacleCell(new Vector2Int(i,j)))
                sff[i,j] = max_value;
            if(sff[i,j] >= gui.sff_init_value)
                sff[i,j] = max_value;
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
            sff[i, j] = gui.sff_init_value;
        }
    }

    void SetSFF_OneExit(Vector2Int exitPos)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorField ff = FindObjectOfType<FloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();

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

                if (fm.isValidCell(adjCell))
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
