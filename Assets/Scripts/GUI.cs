using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GUI : MonoBehaviour
{
    public int planeRow = 10;
    public int planeCol = 10;
    public float agentDensity = 0.2f;
    public Vector2Int[] exitPos;
    public List<List<int>> exit_group = new List<List<int>>();
    public int[] exitWidth;
    public int totalExitWidth = 0;
    public float kS = 0.0f, kE = 1.0f, kD = 0.0f, kA = 0.0f;
    public float sff_init_value = 1000f;
    public float sff_offset_hv = 1.0f;
    public float sff_offset_lambda = 1.5f;
    public float dff_decay = 0.3f;
    public float dff_diffuse = 0.8f;
    public int aff_radius = 4;
    public int influence_radius = 1;
    public int Not_Assign = -1;
    public float low_density = 0.2f;
    public int min_distance_from_exits = 2;
    public bool flg_update_sff = false;
    public bool flg_update = false;
    public List<string> infoTextList = new List<string>();
    public TextMeshProUGUI info_text;

    // Start is called before the first frame update
    void Start()
    {
        info_text = transform.Find("Info/text").GetComponent<TextMeshProUGUI>();
        infoTextList.Add("TimeSteps: 0");
        infoTextList.Add("Dimension: " + planeRow.ToString() + " x " + planeCol.ToString());
        infoTextList.Add("Agent Density: " + agentDensity.ToString());
        infoTextList.Add("kS: " + kS.ToString());
        infoTextList.Add("kE: " + kE.ToString());
        infoTextList.Add("kD: " + kD.ToString());
        infoTextList.Add("kA: " + kA.ToString());
        infoTextList.Add("DFF Decay: " + dff_decay.ToString());
        infoTextList.Add("DFF Diffuse: " + dff_diffuse.ToString());
        infoTextList.Add("AFF Radius: " + aff_radius.ToString());
        infoTextList.Add("Influence Radius: " + influence_radius.ToString());
        infoTextList.Add("Lowest Density: " + low_density.ToString());
        infoTextList.Add("Min Distance From Exits: " + min_distance_from_exits.ToString());
        SetInfoText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInfo(string property, string value)
    {
        int idx = infoTextList.FindIndex( text => text.Contains(property) );
        if (idx >= 0)
            infoTextList[idx] = property + ": " + value;
        SetInfoText();
    }

    void SetInfoText()
    {
        string text = "";
        foreach (string info in infoTextList)
        {
            text += info;
            text += "\n";
        }
        info_text.text = text;
    }

    public void Setup()
    {
        SetupExitWidth();
        FindObjectOfType<AgentManager>().Setup();
        FindObjectOfType<FloorModel>().Setup();
        FindObjectOfType<DynamicFloorField>().Setup();
        FindObjectOfType<StaticFloorField>().Setup();
        FindObjectOfType<StaticFloorField_ExitWidth>().Setup();
        FindObjectOfType<ObstacleModel>().Setup();
        FindObjectOfType<CameraMovment>().Setup();
    }

    public void Reset()
    {
        flg_update = false;
        FindObjectOfType<AgentManager>().Reset();
        FindObjectOfType<ObstacleModel>().Reset();
        FindObjectOfType<FloorModel>().Reset();
        FindObjectOfType<DynamicFloorField>().Reset();
        FindObjectOfType<StaticFloorField>().Reset();
        FindObjectOfType<StaticFloorField_ExitWidth>().Reset();
    }

    void SetupExitWidth()
    {
        exitWidth = new int[exitPos.Length];
        totalExitWidth = 0;

        // Union-Find Initialize
        for (int i = 0; i < exitWidth.Length; i++)
        {
            exitWidth[i] = -(i+1);
        }

        // Union-Find
        for (int i = 0; i < exitWidth.Length; i++)
        for (int j = 0; j < exitWidth.Length; j++)
        {
            // vertical link
            if (exitPos[i].x == exitPos[j].x && Mathf.Abs(exitPos[j].y - exitPos[i].y) == 1)
            {
                for (int k = 0; k < exitWidth.Length; k++)
                    if (exitWidth[k] == exitWidth[j])
                        exitWidth[k] = exitWidth[i];
            }
            // horizontal link
            else if (exitPos[i].y == exitPos[j].y && Mathf.Abs(exitPos[j].x - exitPos[i].x) == 1)
            {
                for (int k = 0; k < exitWidth.Length; k++)
                    if (exitWidth[k] == exitWidth[j])
                        exitWidth[k] = exitWidth[i];
            }
        }

        // Calculate group size (exit width)
        HashSet<int> groups = new HashSet<int>(exitWidth);
        Dictionary<int,List<int>> dict = new Dictionary<int,List<int>>();
        

        foreach (int id in groups)
        {
            int width = 0;
            dict.Add(id,new List<int>());
            for (int i = 0; i < exitWidth.Length; i++)
                if (exitWidth[i] == id)
                {
                    width ++;
                    dict[id].Add(i);
                }     
            for (int i = 0; i < exitWidth.Length; i++)
                if (exitWidth[i] == id)
                    exitWidth[i] = width;

            totalExitWidth += width;
        }

        
        exit_group = dict.Values.ToList();
    }
}


