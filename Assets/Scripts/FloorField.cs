using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorField : MonoBehaviour
{
    public float[,] ff;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        StaticFloorField sff = FindObjectOfType<StaticFloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        ff = new float[gui.planeRow, gui.planeCol];
        
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            ff[i,j] = gui.mS * sff.sff[i,j];
            // Debug.Log(ff[i, j]);
        }

        DrawHeatMap();
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

    void DrawHeatMap()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        float min = ff.Cast<float>().Min();
        float max = ff.Cast<float>().Max();
        float range = max - min;

        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[6];
        colorKey[0].color = Color.blue * 0.5f;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.blue;
        colorKey[1].time = 0.1f;
        colorKey[2].color = Color.cyan;
        colorKey[2].time = 0.4f;
        colorKey[3].color = Color.yellow;
        colorKey[3].time = 0.6f;
        colorKey[4].color = Color.red;
        colorKey[4].time = 0.9f;
        colorKey[5].color = Color.red * 0.5f;
        colorKey[5].time = 1.0f;

        // Populate the alpha keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            float value = ((ff[i,j] - min) / range);
            fm.floor[i,j].GetComponent<Renderer>().material.color = gradient.Evaluate(value);
        }
    }
}
