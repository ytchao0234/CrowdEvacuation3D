using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class GUI : MonoBehaviour
{
    public int planeRow = 30;
    public int planeCol = 30;
    public float agentDensity = 0.2f;
    public Vector2Int[] exitPos;
    public List<ExitParameter> exit_param_top;
    public List<ExitParameter> exit_param_bottom;
    public List<ExitParameter> exit_param_left;
    public List<ExitParameter> exit_param_right;
    public int[] exitWidth;
    public int totalExitWidth = 0;
    public float kS = 0.0f, kE = 1.0f, kD = 0.0f;
    public float sff_init_value = 1000f;
    public float sff_offset_hv = 1.0f;
    public float sff_offset_lambda = 1.5f;
    public float dff_decay = 0.8f;
    public float dff_diffuse = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        exitPos = GenExit();
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

    Vector2Int[] GenExit()
    {
        List<Vector2Int> exitResult = new List<Vector2Int>();
        Vector2Int temp;
        foreach (ExitParameter param in exit_param_top)
        {
            temp = GenExit_One(param,"Top");
            int start = temp.x;
            int end = temp.y;
            for(int i = start; i <= end; i++)
            {
                exitResult.Add(new Vector2Int(0,i));
            }
        }
        foreach (ExitParameter param in exit_param_bottom)
        {
            temp = GenExit_One(param,"Bottom");
            int start = temp.x;
            int end = temp.y;
            for(int i = start; i <= end; i++)
            {
                exitResult.Add(new Vector2Int(planeRow - 1,i));
            }
        }
        foreach (ExitParameter param in exit_param_left)
        {
            temp = GenExit_One(param,"Left");
            int start = temp.x;
            int end = temp.y;
            for(int i = start; i <= end; i++)
            {
                exitResult.Add(new Vector2Int(i,0));
            }
        }
        foreach (ExitParameter param in exit_param_right)
        {
            temp = GenExit_One(param,"Right");
            int start = temp.x;
            int end = temp.y;
            for(int i = start; i <= end; i++)
            {
                exitResult.Add(new Vector2Int(i,planeCol - 1));
            }
        }
        return exitResult.Distinct().ToArray();
    }
    
    Vector2Int GenExit_One(ExitParameter param,string Bound)
    {

        int center = 0;
        int width = 0;
        int start = 0;
        int end = 0;
        int maxWidth = 0;
        if(String.Equals(Bound,"Top") || String.Equals(Bound,"Bottom"))
            maxWidth = planeCol;
        else
            maxWidth = planeRow;

        center = Mathf.FloorToInt(param.position * maxWidth);
        width = Mathf.FloorToInt(param.width * maxWidth);

        if (width >= maxWidth)
        {
            start = 0;
            end = maxWidth - 1;
        }
        else
        {
            start = center - (width / 2);
            end = center + (width / 2);

            if (start < 0)
            {
                start = 0;
                end = start + width;
            }
            else if (end >= maxWidth)
            {
                end = maxWidth - 1;
                start = end - width;
            }
        }

        return new Vector2Int(start,end);
        
    }
}


