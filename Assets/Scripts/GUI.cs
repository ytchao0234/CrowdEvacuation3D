using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GUI : MonoBehaviour
{
    public int planeRow = 30;
    public int planeCol = 30;
    public float agentDensity = 0.3f;
    public Vector2Int[] exitPos = {
            new Vector2Int(0,16),
            new Vector2Int(29,14),
            new Vector2Int(29,15),
            new Vector2Int(29,16),
            new Vector2Int(29,17),
            new Vector2Int(29,18),
        };
    public int[] exitWidth;
    public int totalExitWidth = 0;
    public float kS = 0.0f, kE = 1.0f, kD = 1.0f;
    public float sff_init_value = 1000f;
    public float sff_offset_hv = 1.0f;
    public float sff_offset_lambda = 1.5f;
    public float dff_decay = 0.8f;
    public float dff_diffuse = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        SetupExitWidth();
    }

    // Update is called once per frame
    void Update()
    {
        
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

        foreach (int id in groups)
        {
            int width = 0;

            for (int i = 0; i < exitWidth.Length; i++)
                if (exitWidth[i] == id)
                    width ++;
            for (int i = 0; i < exitWidth.Length; i++)
                if (exitWidth[i] == id)
                    exitWidth[i] = width;

            totalExitWidth += width;
        }
    }
}
