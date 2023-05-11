using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    int agentNumber;
    float agentHeight;
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

            if (fm.isEmptyCell(new Vector2Int(i, j)))
            {
                GameObject obj = GameObject.Instantiate(agent, fm.floor[i, j].transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.position += Vector3.up * agentHeight / 2f;
                obj.transform.tag = "ActiveAgent";
                obj.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            AgentMove();
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
        List<(Vector2Int, float)> possiblePos = new List<(Vector2Int, float)>();

        for (int m = -1; m <= 1; m++)
        for (int n = -1; n <= 1; n++)
        {
            if (m == 0 && n == 0) continue;
            Vector2Int cell = new Vector2Int(i + m, j + n);

            if (ff.isValidCell(cell) && fm.isValidCell(cell))
                possiblePos.Add((cell, ff.ff[cell.x, cell.y]));
        }

        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        for (int m = possiblePos.Count - 1; m >= 0; m--)
        {
            if (possiblePos[m].Item2 <= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, m + 1);
                Vector2Int cell = possiblePos[rnd].Item1;
                Debug.Log(cell);
    
                agentTrans.position = fm.floor[cell.x, cell.y].transform.position;
                agentTrans.position += Vector3.up * agentHeight / 2f;
                agentTrans.parent = fm.floor[cell.x, cell.y].transform;

                if (fm.floor[cell.x, cell.y].transform.tag == "Exit")
                    agentTrans.tag = "ExitAgent";

                return;
            }
        }
    }
}
