using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    int agentNumber;
    float agentHeight;
    public List<GameObject> agentList = new List<GameObject>();
    List<Vector2Int> lastPos = new List<Vector2Int>();
    public List<Vector2Int> currentPos = new List<Vector2Int>();
    public List<List<int>> whiteList = new List<List<int>>();
    public List<List<int>> blackList = new List<List<int>>();
    public List<string> volunteerStrategy = new List<string>();
    public List<int> inChargeOfList = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindObjectOfType<FloorField>().Compute();
            RemoveExitAgents();
            AgentMove();
            FindObjectOfType<DynamicFloorField>().UpdateDFF_Diffuse_and_Decay();
            FindObjectOfType<FloorField>().DrawHeatMap(FindObjectOfType<DynamicFloorField>().dff);
        }
    }

    public void Setup()
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
                SetAgentType(obj,"ActiveAgent");
                lastPos.Add(new Vector2Int(i,j));
                currentPos.Add(new Vector2Int(i,j));
                agentList.Add(obj);
                whiteList.Add(new List<int>());
                blackList.Add(new List<int>());
                volunteerStrategy.Add("C");
                inChargeOfList.Add(gui.Not_Assign);
                k ++;
            }
        }


    }

    public void Reset()
    {
        agentNumber = 0;
        agentHeight = 0;

        for (int i = 0; i < agentList.Count; i++)
        {
            DestroyImmediate(agentList[i]);
            // Destroy(agentList[i], 0f);
        }
        agentList.Clear();
        lastPos.Clear();
        currentPos.Clear();
        volunteerStrategy.Clear();
        whiteList.Clear();
        blackList.Clear();
        inChargeOfList.Clear();
    }

    void AgentMove()
    {
        int[] random_agent = ShuffleRange_Int(0, agentNumber - 1);

        foreach (int i in random_agent)
        {
            // if (agentList[i].transform.tag != "ActiveAgent") continue;
            AgentMove_OneAgent(i);
        }

        foreach (int i in random_agent)
        {
            // if (agentList[i].transform.tag != "ActiveAgent") continue;
            StartCoroutine(AgentMove_Animation(i, 0f));
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

            if (fm.isValidCell(cell) && fm.isEmptyCell(cell))
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

                dff.dff[currentPos[i].x,currentPos[i].y] += 1f;
                lastPos[i] = currentPos[i];
                currentPos[i] = cell;

                if (fm.floor[cell.x, cell.y].transform.tag == "Exit")
                    SetAgentType(agentList[i],"ExitAgent");

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

    IEnumerator AgentMove_Animation(int i, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        FloorModel fm = FindObjectOfType<FloorModel>();

        float timer = 0.0f;
        float duration = 0.8f;
        Vector2Int lastCell = lastPos[i];
        Vector2Int curCell = currentPos[i];
        Vector3 last = fm.floor[lastCell.x, lastCell.y].transform.position;
        Vector3 current = fm.floor[curCell.x, curCell.y].transform.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            agentList[i].transform.position = Vector3.Lerp(last, current, timer/duration);
            yield return null;
        }

        agentList[i].transform.parent = fm.floor[curCell.x, curCell.y].transform;
        yield break;
    }

    public int GetIndexFromCell(Vector2Int cell)
    {
        int idx = currentPos.FindIndex(pos => pos == cell);
        return idx;
    }

    public void SetAgentType(GameObject agent, string type)
    {
        switch (type)
        {
            case "ActiveAgent":
                agent.transform.tag = type;
                agent.GetComponent<Renderer>().material.color = Color.gray;
                break;
            case "ExitAgent":
                agent.transform.tag = type;
                agent.GetComponent<Renderer>().material.color = Color.gray;
                break;
            case "Volunteer":
                agent.transform.tag = type;
                agent.GetComponent<Renderer>().material.color = Color.blue;
                break;
            default:
                break;
        }
    }
}
