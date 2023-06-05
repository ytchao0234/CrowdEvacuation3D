using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorField : MonoBehaviour
{
    public float[,] ff;
    Gradient gradient = new Gradient();

    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        ff = new float[gui.planeRow, gui.planeCol];
        SetGradientColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Compute()
    {
        GUI gui = FindObjectOfType<GUI>();
        StaticFloorField sff = FindObjectOfType<StaticFloorField>();
        StaticFloorField_ExitWidth sff_e = FindObjectOfType<StaticFloorField_ExitWidth>();
        DynamicFloorField dff = FindObjectOfType<DynamicFloorField>();
        DisasterFloorField dis_ff = FindObjectOfType<DisasterFloorField>();
        AnticipationFloorField aff = FindObjectOfType<AnticipationFloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            ff[i,j] = -gui.kS * sff.sff[i,j] - gui.kE * sff_e.sff_e[i,j] + gui.kD * dff.dff[i,j] - gui.kA * aff.aff[i,j];
            ff[i,j] -= 30f * dis_ff.dff[i,j];
        }
    }

    public void DrawHeatMap(float[,] src_ff)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        float min = src_ff.Cast<float>().Min();
        float max = src_ff.Cast<float>().Max();
        float range = max - min;

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            float value;
            if(range == 0.0f)
                value = 0.0f;
            else
                value = ((src_ff[i,j] - min) / range);
            fm.floor[i,j].GetComponent<Renderer>().material.color = GetColor(value);
        }
    }

    public void ClearColor()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            fm.floor[i,j].GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void SetGradientColor()
    {
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
    }
    public Color GetColor(float value)
    {
        return gradient.Evaluate(value);
    }
}
