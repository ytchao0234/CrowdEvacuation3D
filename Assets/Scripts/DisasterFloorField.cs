using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterFloorField : MonoBehaviour
{
    public float[,] dff;
    float diffuse = 0.3f;
    float initial_value = 10f;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        dff = new float[gui.planeRow, gui.planeCol];
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
        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            dff[i, j] = 0f;
        }
    }

    public void SetDisasterCell(Vector2Int cell)
    {
        dff[cell.x, cell.y] = initial_value;
    }

    public void UpdateDFF_Diffuse_and_Decay()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorField ff = FindObjectOfType<FloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        float[,] tmp_dff = new float[gui.planeRow, gui.planeCol];

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {

            Vector2Int curCell = new Vector2Int(i, j);
            if(!fm.isEmptyCell(curCell) && fm.floor[i,j].transform.GetChild(0).tag == "ImmovableObstacle")
            {
                continue;
            }
            else if(!fm.isEmptyCell(curCell) && fm.floor[i,j].transform.GetChild(0).tag == "MovableObstacle")
            {
                tmp_dff[curCell.x, curCell.y] = (1f - gui.dff_decay) * dff[i,j];
                continue;
            }
            else if(!fm.isEmptyCell(curCell) && fm.floor[i,j].transform.GetChild(0).tag == "AssignedObstacle")
            {
                tmp_dff[curCell.x, curCell.y] = (1f - gui.dff_decay) * dff[i,j];
                continue;
            }
            Vector2Int adjCell = curCell;

            for (int ii = -1; ii <= 1; ii++)
			for (int jj = -1; jj <= 1; jj++)
            {
                if (ii == 0 && jj == 0) continue;

                adjCell = curCell + new Vector2Int(ii, jj);

                if (fm.isValidCell(adjCell))
                {
                    tmp_dff[curCell.x, curCell.y] += dff[adjCell.x, adjCell.y];
                }
            }
            tmp_dff[curCell.x, curCell.y] = 
                (1f - diffuse) *
                dff[curCell.x, curCell.y] + 
                diffuse * tmp_dff[curCell.x, curCell.y] / 8f;
        }

        dff = tmp_dff;
    }
}
