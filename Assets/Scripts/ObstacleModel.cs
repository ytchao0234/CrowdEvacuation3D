using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ObstacleModel : MonoBehaviour
{
    public GameObject obstacle;
    public float obstacleSize;
    public List<GameObject> obstacleList;
    public List<Vector2Int> currentPos;
    List<int> influenceRadiusList;
    List<List<int>> inRangeList;
    List<int> volunteerList;
    List<GameObject> wallList;
    public List<float> densityList;
    List<float> tauList;

    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        
        obstacleList = new List<GameObject>();
        currentPos = new List<Vector2Int>();
        wallList = new List<GameObject>();
        influenceRadiusList = new List<int>();
        inRangeList = new List<List<int>>();
        volunteerList = new List<int>();
        densityList = new List<float>();
        tauList = new List<float>();                                          
        obstacleSize = obstacle.transform.GetComponent<Renderer>().bounds.size.x;
        float planeSize = fm.plane.transform.GetComponent<Renderer>().bounds.size.x;
        List<Vector2Int> boundPos = new List<Vector2Int>();

        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            if (i != 0 && i != gui.planeRow - 1 && j != 0 && j != gui.planeCol - 1)
                continue;

            boundPos.Add(new Vector2Int(i, j));
        }

        // foreach (Vector2Int exit in gui.exitPos)
        // {
        //     if (boundPos.Contains(exit))
        //         boundPos.Remove(exit);
        // }

        foreach (Vector2Int bound in boundPos)
        {
            GameObject obj = GameObject.Instantiate(obstacle, fm.floor[bound.x, bound.y].transform);
            obj.transform.localScale = Vector3.one * (planeSize / obstacleSize);
            obj.transform.position += Vector3.up * planeSize / 2f;
            SetObstacleType(obj, "ImmovableObstacle");
            wallList.Add(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        Debug.Log("Setup ObModel");
        SetupInRange();
        SetWhiteBlackList();
        SetVolunteer();
    }

    public void Reset()
    {
        for (int i = 0; i < obstacleList.Count; i++)
        {
            DestroyImmediate(obstacleList[i]);
            // Destroy(obstacleList[i], 0f);
        }
        for(int i = 0; i < wallList.Count;i++)
        {
            DestroyImmediate(wallList[i]);
            // Destroy(wallList[i], 0f);
        }
        obstacleList.Clear();
        currentPos.Clear();
        inRangeList.Clear();
        volunteerList.Clear();
        influenceRadiusList.Clear();
        wallList.Clear();
        densityList.Clear();
        tauList.Clear();
    }

    void GenObstacle(int i, int j, string type)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        float planeSize = fm.plane.transform.GetComponent<Renderer>().bounds.size.x;
    
        GameObject obj = GameObject.Instantiate(obstacle, fm.floor[i, j].transform);
        obj.transform.localScale = Vector3.one * (planeSize / obstacleSize);
        obj.transform.position += Vector3.up * planeSize / 2f;
        SetObstacleType(obj, type);
        if(i == 0 || i == gui.planeRow - 1 || j == 0 || j == gui.planeCol - 1)
            wallList.Add(obj);
        else
        {
            obstacleList.Add(obj);
            currentPos.Add(new Vector2Int(i,j));
            inRangeList.Add(new List<int>());
            volunteerList.Add(-1);
            influenceRadiusList.Add(1);
        }
    }

    public void SetObstaclesFromGUI()
    {
        GUI gui = FindObjectOfType<GUI>();
        ButtonModel bm = FindObjectOfType<ButtonModel>();
        Button[,] checkboxes = bm.checkboxes;
        List<Vector2Int> exitResult = new List<Vector2Int>();

        foreach (Button btn in checkboxes)
        {
            Checkbox script = btn.GetComponent<Checkbox>();
            int select = btn.GetComponent<Checkbox>().select;
            string type = btn.GetComponent<Checkbox>().type;

            if(type  == "Bound")
                GenObstacle(script.i, script.j, "ImmovableObstacle");
            else if(type == "Exit")
                exitResult.Add(new Vector2Int(script.i, script.j));
            else
            {
                switch (select)
                {
                    case 0:
                        break;
                    case 1:
                        GenObstacle(script.i, script.j, "MovableObstacle");
                        break;
                    case 2:
                        GenObstacle(script.i, script.j, "ImmovableObstacle");
                        break;
                    default:
                        break;
                }
            }
            
        }
        gui.exitPos = exitResult.Distinct().ToArray();
    }


    public void SetObstacleType(GameObject obj, string type)
    {
        switch (type)
        {
            case "ImmovableObstacle":
                obj.transform.tag = type;
                obj.GetComponent<Renderer>().material.color = Color.gray;
                break;
            case "MovableObstacle":
                obj.transform.tag = type;
                obj.GetComponent<Renderer>().material.color = Color.cyan * 0.8f;
                break;
            case "AssignedObstacle":
                obj.transform.tag = type;
                obj.GetComponent<Renderer>().material.color = Color.yellow * 0.8f;
                break;
            default:
                break;
        }
    }

    void SetupInRange()
    {
        FloorField ff = FindObjectOfType<FloorField>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        AgentManager am = FindObjectOfType<AgentManager>();

        densityList.Clear();
        for (int i = 0; i < obstacleList.Count; i++)
        {
            inRangeList[i].Clear();
            densityList.Add(0f);
            if (obstacleList[i].transform.tag != "MovableObstacle" && obstacleList[i].transform.tag != "AssignedObstacle")
                continue;

            int r = influenceRadiusList[i];
            int floorCount = 0;
            int agentCount = 0;

            for (int j = -r; j <=r ; j++)
            for (int k = -r; k <=r ; k++)
            {
                if(((j*j + k*k) > (r*r)) || (j == 0 && k == 0))
                    continue;
                floorCount++;
                Vector2Int cell = currentPos[i] + new Vector2Int(j, k);

                if (fm.isValidCell(cell) && fm.isAgentCell(cell))
                {
                    int idx = am.GetIndexFromCell(cell);
                    if (idx >= 0)
                    {
                        inRangeList[i].Add(idx);
                        // Debug.Log("inRangeList[" + i.ToString() + "]: " + idx.ToString());
                    }
                    agentCount++;
                }
            }
            if(floorCount != 0)
                densityList[i] = (float)agentCount/floorCount;
        }
    }

    void SetWhiteBlackList()
    {
        AgentManager am = FindObjectOfType<AgentManager>();
        tauList.Clear();
        for(int i = 0; i < obstacleList.Count; i++)
        {
            tauList.Add(0f);
            if(obstacleList[i].transform.tag != "MovableObstacle")
            {
                continue;
            }
            tauList[i] = CalcBlockedProportion(i);
            // Debug.Log("tau: " + tau.ToString());
            if(tauList[i] < 1f)
            {
                float factor = Mathf.Pow(1f - tauList[i], 1f / inRangeList[i].Count);
                SolveConflictVolunteer(inRangeList[i], factor);
                foreach(int agent_idx in inRangeList[i])
                {
                    if(am.whiteList[agent_idx].Contains(i))
                        am.whiteList[agent_idx].Remove(i);
                    if(am.blackList[agent_idx].Contains(i))
                        am.blackList[agent_idx].Remove(i);

                    if(am.volunteerStrategy[agent_idx] == "C")
                        am.blackList[agent_idx].Add(i);
                    else
                        am.whiteList[agent_idx].Add(i);
                }
            }
            else
            {
                foreach(int agent_idx in inRangeList[i])
                {
                    if(am.blackList[agent_idx].Contains(i))
                        am.blackList[agent_idx].Remove(i);
                    if(!am.whiteList[agent_idx].Contains(i))
                        am.whiteList[agent_idx].Add(i);
                }
            }

        }
    }

    float CalcBlockedProportion(int obstacle_idx)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        foreach(List<int> exit in gui.exit_group)
        {
            bool isBlocked = false;
            foreach(int exit_idx in exit)
            {
                if(Mathf.Abs(currentPos[obstacle_idx].x - gui.exitPos[exit_idx].x) <= 1 &&
                    Mathf.Abs(currentPos[obstacle_idx].y - gui.exitPos[exit_idx].y) <= 1)
                   isBlocked = true;
            }
            if(!isBlocked)
                continue;
            List<Vector2Int> neighbor = new List<Vector2Int>();
            foreach(int exit_idx in exit)
            {
                Vector2Int adjcell;
                for(int i = -1; i <= 1; i++)
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        if(i == 0 && j == 0)
                            continue;
                        adjcell = gui.exitPos[exit_idx] + new Vector2Int(i,j);
                        if(fm.isValidCell(adjcell) && fm.isNotImmovableExitCell(adjcell) && !neighbor.Contains(adjcell))
                            neighbor.Add(adjcell);
                    }
                }

            }
            return (float)CountMovable(neighbor) / neighbor.Count;
            
        }
        return 0f;
    }

    int CountMovable(List<Vector2Int> neighbor)
    {
        FloorModel fm = FindObjectOfType<FloorModel>();
        int count = 0;
        foreach(Vector2Int cell in neighbor)
        {
            if(!fm.isEmptyCell(cell) && fm.floor[cell.x, cell.y].transform.GetChild(0).tag == "MovableObstacle")
                count++;
            if(!fm.isEmptyCell(cell) && fm.floor[cell.x, cell.y].transform.GetChild(0).tag == "AssignedObstacle")
                count++;
        }
        return count;
    }

    void SolveConflictVolunteer(List<int> agents, float factor)
    {
        AgentManager am = FindObjectOfType<AgentManager>();
        if (agents.Count == 0)
        {
            // Debug.Log("Number of Potential Volunteers: 0");
            return;
        }
        if (agents.Count == 1)
        {
            am.volunteerStrategy[0] = "D";
            // Debug.Log("Number of Potential Volunteers: 1");
            return;
        }

        float cv = 1f; // Range(0f, 1f)
        float p = 1f - Mathf.Pow(cv, 1f / (agents.Count - 1)) * factor;
        int count = 0;
        foreach (int agent_idx in agents)
        {
            am.volunteerStrategy[agent_idx] = Random.Range(0f, 1f) < p ? "D" : "C";
            if (am.volunteerStrategy[agent_idx] == "D") count ++;
        }
        // Debug.Log("Number of Potential Volunteers: " + count.ToString());
    }

    bool SetVolunteer()
    {
        AgentManager am = FindObjectOfType<AgentManager>();
        bool flag = false;

        for(int i = 0; i < obstacleList.Count; i++)
        {
            if(obstacleList[i].transform.tag != "MovableObstacle")
            {
                continue;
            }
            List<int> candidates = new List<int>();
            foreach(int agent_idx in inRangeList[i])
            {
                if(Mathf.Abs(am.currentPos[agent_idx].x - currentPos[i].x) <= 1 &&
                    Mathf.Abs(am.currentPos[agent_idx].y - currentPos[i].y) <= 1 && am.whiteList[agent_idx].Contains(i))
                {
                    candidates.Add(agent_idx);
                }
                    
            }
            if(candidates.Count > 0)
            {
                int volunteer_idx = candidates[Random.Range(0,candidates.Count)];
                SetObstacleType(obstacleList[i],"AssignedObstacle");
                // Debug.Log("obstacleList[" + i.ToString()  + "]");
                // Debug.Log("candidates.Count" + candidates.Count.ToString());
                am.SetAgentType(am.agentList[volunteer_idx],"Volunteer");

                am.inChargeOfList[volunteer_idx] = i;
                am.whiteList[volunteer_idx].Clear();
                am.blackList[volunteer_idx].Clear();
                Debug.Log("Obstacle: " + currentPos[i].ToString());
                SetDestination(volunteer_idx, i);
                flag = true;
                foreach(int agent_idx in inRangeList[i])
                {
                    if(am.whiteList[agent_idx].Contains(i))
                        am.whiteList[agent_idx].Remove(i);
                    if(am.blackList[agent_idx].Contains(i))
                        am.blackList[agent_idx].Remove(i);
                }
                inRangeList[i].Clear();

            }

        }
        return flag;
    }

    void SetDestination(int volunteer_idx, int obstacle_idx)
    {
        AgentManager am = FindObjectOfType<AgentManager>();
        SpecificFloorField specific_ff = am.agentList[volunteer_idx].GetComponent<SpecificFloorField>();
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        StaticFloorField sff = FindObjectOfType<StaticFloorField>();
        StaticFloorField_ExitWidth sff_e = FindObjectOfType<StaticFloorField_ExitWidth>();

        // TODO
        Vector2Int dest = currentPos[obstacle_idx];
        specific_ff.SetDestination(dest);
        specific_ff.Setup();
        List<(Vector2Int,float)> possiblePos = new List<(Vector2Int,float)>();
        for(int i = 0;i < gui.planeRow; i++)
        {
            for(int j = 0; j < gui.planeCol;j++)
            {
                Vector2Int cell = new Vector2Int(i,j);
                float sff_value = gui.kS * sff.sff[i,j] + gui.kE * sff_e.sff_e[i,j];
                if(!(fm.isEmptyCell(cell) || fm.isAgentCell(cell)) || 
                    sff_value < gui.min_distance_from_exits || cell == am.currentPos[volunteer_idx])
                    continue;
                    
                int numWallNeighbors = 0;
                Vector2Int adjCell = cell;
                
                for(int ii = -1; ii <= 1; ii++)
                {
                    for(int jj = -1; jj <=1; jj++)
                    {
                        if(ii == 0 && jj == 0)
                            continue;
                        adjCell = cell + new Vector2Int(ii,jj);
                        if(fm.isValidCell(adjCell) && fm.isImmovableObstacle(adjCell))
                            numWallNeighbors++;
                    }
                }

                if(numWallNeighbors >= 3)
                    possiblePos.Add((cell, specific_ff.sff[cell.x, cell.y]));
            }
        }
        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        for (int k = possiblePos.Count - 1; k >= 0; k--)
        {
            if (possiblePos[k].Item2 <= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, k + 1);
                int count = 0;
                dest = possiblePos[rnd].Item1;
                if (possiblePos.Count > 1)
                {
                    while (am.agentList.FindIndex(agent => agent.GetComponent<SpecificFloorField>().destination == dest) != -1 && count ++ < possiblePos.Count)
                    {
                        dest = possiblePos[(rnd + 1) % possiblePos.Count].Item1;
                    }
                }
                Debug.Log("Dest: " + dest.ToString());
                break;
            }
        }

        
        //
        specific_ff.SetDestination(dest);
        specific_ff.Setup();
    }

    public bool LowTauDensity(int obstacle_idx)
    {
        GUI gui = FindObjectOfType<GUI>();
        return (densityList[obstacle_idx] < gui.low_density && tauList[obstacle_idx] < 1f);
    }

    public IEnumerator ObstacleMove_Animation(int i, Vector2Int dest, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        FloorModel fm = FindObjectOfType<FloorModel>();

        float timer = 0.0f;
        float duration = 0.8f;
        Vector3 from = fm.floor[currentPos[i].x, currentPos[i].y].transform.position;
        Vector3 to = fm.floor[dest.x, dest.y].transform.position;
        from.y = obstacleList[i].transform.position.y;
        to.y = obstacleList[i].transform.position.y;
        currentPos[i] = dest;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            obstacleList[i].transform.position = Vector3.Lerp(from, to, timer/duration);
            yield return null;
        }

        yield break;
    }
}
