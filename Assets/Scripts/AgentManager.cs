using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    int agentNumber;
    float agentHeight;
    Vector2Int[,] lastPos;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
    
        float agentDensity = gui.agentDensity;
        int planeRow = gui.planeRow;
        int planeCol = gui.planeCol;
        agentNumber = Mathf.FloorToInt(planeRow * planeCol * agentDensity);
        agentHeight = agent.transform.GetComponent<Renderer>().bounds.size.y;
        lastPos = new Vector2Int[planeRow, planeCol];

        int[] randomNumbers = new int [planeRow * planeCol];
        for (int k = 0; k < randomNumbers.Length; k ++)
        {
            randomNumbers[k] = k;
        }

        List<int> indexList = new List<int>( randomNumbers );

        for (int k = 0; k < agentNumber; k ++)
        {
            int rnd = Random.Range(0, indexList.Count);
            int index = indexList[rnd];
            int i = index / planeCol;
            int j = index % planeCol;
            indexList.RemoveAt(rnd);

            if (fm.isEmptyCell(new Vector2Int(i, j)) && !fm.isExitCell(new Vector2Int(i, j)))
            {
                GameObject obj = GameObject.Instantiate(agent, fm.floor[i, j].transform);
                obj.transform.localScale = Vector3.one * 4f;
                obj.transform.position += Vector3.up * agentHeight * 2f;
                obj.transform.tag = "ActiveAgent";
                obj.GetComponent<Renderer>().material.color = Color.white;
                lastPos[i,j] = new Vector2Int(i,j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AgentMove();
            FindObjectOfType<DynamicFloorField>().UpdateDFF_Diffuse_and_Decay();
        }
    }

    void AgentMove()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            if (fm.floor[i,j].transform.childCount <= 0) continue;

            Transform trans = fm.floor[i,j].transform.GetChild(0);
            if (trans.tag != "ActiveAgent") continue;

            AgentMove_OneAgent(i, j, trans);
        }
    }

    void AgentMove_OneAgent(int i, int j, Transform agentTrans)
    {
        FloorModel fm = FindObjectOfType<FloorModel>();
        FloorField ff = FindObjectOfType<FloorField>();
        DynamicFloorField dff = FindObjectOfType<DynamicFloorField>();
        List<(Vector2Int, float)> possiblePos = new List<(Vector2Int, float)>();

        for (int m = -1; m <= 1; m++)
        for (int n = -1; n <= 1; n++)
        {
            if (m == 0 && n == 0) continue;
            if (lastPos[i,j].x == i + m && lastPos[i,j].y == j + n) continue;
            Vector2Int cell = new Vector2Int(i + m, j + n);

            if (ff.isValidCell(cell) && fm.isValidCell(cell))
                possiblePos.Add((cell, ff.ff[cell.x, cell.y]));
        }

        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => -a.Item2.CompareTo(b.Item2));

        for (int m = possiblePos.Count - 1; m >= 0; m--)
        {
            if (possiblePos[m].Item2 >= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, m + 1);
                Vector2Int cell = possiblePos[rnd].Item1;
    
                agentTrans.position = fm.floor[cell.x, cell.y].transform.position;
                agentTrans.position += Vector3.up * agentHeight / 2f;
                agentTrans.parent = fm.floor[cell.x, cell.y].transform;
                dff.dff[lastPos[i,j].x, lastPos[i,j].y] += 1f;
                lastPos[i,j] = cell;

                if (fm.floor[cell.x, cell.y].transform.tag == "Exit")
                    agentTrans.tag = "ExitAgent";

                return;
            }
        }
    }
}
