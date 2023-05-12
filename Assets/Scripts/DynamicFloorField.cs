using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicFloorField : MonoBehaviour
{
    public float[,] dff;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        Vector2Int[] exitPos = gui.exitPos;
        dff = new float[gui.planeRow, gui.planeCol];

        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            dff[i, j] = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateDFF_Diffuse_and_Decay()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorField ff = FindObjectOfType<FloorField>();
        float[,] tmp_dff = new float[gui.planeRow, gui.planeCol];

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            Vector2Int curCell = new Vector2Int(i, j);
            Vector2Int adjCell = curCell;

            for (int ii = -1; ii <= 1; ii++)
			for (int jj = -1; jj <= 1; jj++)
            {
                if (ii == 0 && jj == 0) continue;

                adjCell = curCell + new Vector2Int(ii, jj);

                if (ff.isValidCell(adjCell))
                {
                    tmp_dff[curCell.x, curCell.y] += dff[adjCell.x, adjCell.y];
                }
            }
            tmp_dff[curCell.x, curCell.y] = 
                (1f - gui.dff_decay) * 
                    (
                        (1f - gui.dff_diffuse) *
                        dff[curCell.x, curCell.y] + 
                        gui.dff_diffuse * tmp_dff[curCell.x, curCell.y] / 8f
                    );
        }

        dff = tmp_dff;
        DrawHeatMap();
    }
    void DrawHeatMap()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        float min = dff.Cast<float>().Min();
        float max = dff.Cast<float>().Max();
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
            float value = ((dff[i,j] - min) / range);
            fm.floor[i,j].GetComponent<Renderer>().material.color = gradient.Evaluate(value);
        }
    }
}
