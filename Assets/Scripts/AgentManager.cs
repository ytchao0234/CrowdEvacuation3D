using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    int agentNumber;
    float agentHeight;
    List<GameObject> agentList = new List<GameObject>();
    List<Vector2Int> lastPos = new List<Vector2Int>();
    List<Vector2Int> currentPos = new List<Vector2Int>();

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

        for (int k = 0; k < agentNumber; )
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
                lastPos.Add(new Vector2Int(i,j));
                currentPos.Add(new Vector2Int(i,j));
                agentList.Add(obj);
                k ++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RemoveExitAgents();
            AgentMove();
            FindObjectOfType<DynamicFloorField>().UpdateDFF_Diffuse_and_Decay();
        }
    }

    void AgentMove()
    {
        int[] random_agent = ShuffleRange_Int(0, agentNumber - 1);

        foreach (int i in random_agent)
        {
            if (agentList[i].transform.tag != "ActiveAgent") continue;
            AgentMove_OneAgent(i);
        }
    }

    void AgentMove_OneAgent(int i)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        FloorField ff = FindObjectOfType<FloorField>();
        DynamicFloorField dff = FindObjectOfType<DynamicFloorField>();

        List<(Vector2Int, float)> possiblePos = new List<(Vector2Int, float)>();

        for (int m = -1; m <= 1; m++)
        for (int n = -1; n <= 1; n++)
        {
            Vector2Int cell = currentPos[i] + new Vector2Int(m, n);
            float ff_value = ff.ff[cell.x, cell.y];

            if (ff.isValidCell(cell) && fm.isEmptyCell(cell))
            {
                if (lastPos[i] == cell)
                    ff_value = ff_value - gui.kD;
                possiblePos.Add((cell, ff_value));
            }
        }

        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => -a.Item2.CompareTo(b.Item2));

        for (int m = possiblePos.Count - 1; m >= 0; m--)
        {
            if (possiblePos[m].Item2 >= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, m + 1);
                Vector2Int cell = possiblePos[rnd].Item1;
                if (cell == currentPos[i]) return;
    
                agentList[i].transform.position = fm.floor[cell.x, cell.y].transform.position;
                agentList[i].transform.position += Vector3.up * agentHeight / 2f;
                agentList[i].transform.parent = fm.floor[cell.x, cell.y].transform;

                dff.dff[currentPos[i].x,currentPos[i].y] += 1f;
                lastPos[i] = currentPos[i];
                currentPos[i] = cell;

                if (fm.floor[cell.x, cell.y].transform.tag == "Exit")
                    agentList[i].transform.tag = "ExitAgent";

                return;
            }
        }
    }

    void RemoveExitAgents()
    {

        for(int i=0;i<agentNumber;)
        {
            if(agentList[i].transform.tag == "ExitAgent")
            {
                DestroyImmediate(agentList[i]);
                agentList.RemoveAt(i);
                lastPos.RemoveAt(i);
                currentPos.RemoveAt(i);
                agentNumber--;
            }
            else
                i++;
        }
    }

    int[] ShuffleRange_Int(int range_start, int range_end)
    {
        int range = range_end - range_start + 1;
        int[] result = new int[range];
        int rnd, tmp;

        for (int i = 0; i < range; i++) result[i] = i + range_start;

        for (int i = 0; i < range; i++)
        {
            rnd = Random.Range(0, range);
            tmp = result[rnd];
            result[rnd] = result[i];
            result[i] = tmp;
        }

        return result;
    }
}
