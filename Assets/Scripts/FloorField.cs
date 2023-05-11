using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorField : MonoBehaviour
{
    public float[,] ff;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        StaticFloorField sff = FindObjectOfType<StaticFloorField>();

        ff = new float[gui.planeRow, gui.planeCol];
        
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            ff[i,j] = gui.mS * sff.sff[i,j];
            // Debug.Log(ff[i, j]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool isValidCell(Vector2Int cell)
    {
        GUI gui = FindObjectOfType<GUI>();

        bool flg = true;
        flg &= cell.x >= 0 && cell.x < gui.planeRow;
        flg &= cell.y >= 0 && cell.y < gui.planeCol;

        return flg;
    }
}
