using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class GUI : MonoBehaviour
{
    public int planeRow = 10;
    public int planeCol = 10;
    public float agentDensity = 0.2f;
    public Vector2Int[] exitPos;
    public List<List<int>> exit_group = new List<List<int>>();
    public int[] exitWidth;
    public int totalExitWidth = 0;
    public float kS = 0.0f, kE = 1.0f, kD = 0.0f;
    public float sff_init_value = 1000f;
    public float sff_offset_hv = 1.0f;
    public float sff_offset_lambda = 1.5f;
    public float dff_decay = 0.3f;
    public float dff_diffuse = 0.8f;
    public int Not_Assign = -1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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
    }

    public void Reset()
    {
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


