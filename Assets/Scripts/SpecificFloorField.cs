using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificFloorField : MonoBehaviour
{
    public float[,] sff;
    public Vector2Int destination;
    float max_value;

    void Awake()
    {
        GUI gui = FindObjectOfType<GUI>();
        sff = new float[gui.planeRow, gui.planeCol];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDestination(Vector2Int dest)
    {
        destination = dest;
    }

    public void Setup()
    {
        Reset();
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        sff[destination.x, destination.y] = 0f;
        SetSFF();
        foreach(float value in sff)
        {
            if(value < gui.sff_init_value && value > max_value)
                max_value = value;
        }
        for(int i = 0; i < gui.planeRow; i++)
        for(int j = 0; j < gui.planeCol; j++)
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

    void SetSFF()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        float offset_hv = gui.sff_offset_hv;
        float offset_d = offset_hv * gui.sff_offset_lambda;

        Queue<Vector2Int> toDoList = new Queue<Vector2Int>();
        toDoList.Enqueue(destination);

        while (toDoList.Count > 0)
        {
            Vector2Int curCell = toDoList.Dequeue();
            Vector2Int adjCell = curCell;

            for (int i = -1; i <= 1; i++)
			for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                adjCell = curCell + new Vector2Int(i, j);

                if (fm.isValidCell(adjCell) && !fm.isObstacleCell(adjCell))
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
