using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    public List<Material> materials;
    int agentNumber;
    float agentHeight;
    public List<GameObject> agentList = new List<GameObject>();
    List<Vector2Int> lastPos = new List<Vector2Int>();
    public List<Vector2Int> currentPos = new List<Vector2Int>();
    List<Quaternion> lastRot = new List<Quaternion>();
    List<Quaternion> currentRot = new List<Quaternion>();
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
        GUI gui = FindObjectOfType<GUI>();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindObjectOfType<FloorField>().Compute();
            FindObjectOfType<AnticipationFloorField>().UpdateAFF();
            RemoveExitAgents();
            AgentMove();
            FindObjectOfType<DynamicFloorField>().UpdateDFF_Diffuse_and_Decay();
            if(gui.flg_update_sff)
            {
                FindObjectOfType<StaticFloorField>().Setup();
                FindObjectOfType<StaticFloorField_ExitWidth>().Setup();
            }
            // FindObjectOfType<FloorField>().DrawHeatMap(FindObjectOfType<FloorField>().ff);
            FindObjectOfType<ObstacleModel>().Setup();
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
        agentHeight = agent.transform.Find("top").transform.GetComponent<Renderer>().bounds.size.y;
        agentHeight += agent.transform.Find("bottom").transform.GetComponent<Renderer>().bounds.size.y;

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
                obj.transform.localScale = Vector3.one * 6f;
                obj.transform.position += Vector3.up * agentHeight / 2f;
                obj.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                SetAgentType(obj,"ActiveAgent");
                lastPos.Add(new Vector2Int(i,j));
                currentPos.Add(new Vector2Int(i,j));
                lastRot.Add(obj.transform.rotation);
                currentRot.Add(obj.transform.rotation);
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
        lastRot.Clear();
        currentRot.Clear();
        volunteerStrategy.Clear();
        whiteList.Clear();
        blackList.Clear();
        inChargeOfList.Clear();
    }


    void AgentMove()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        int[] random_agent = ShuffleRange_Int(0, agentNumber - 1);
        gui.flg_update_sff = false;

        foreach (int i in random_agent)
        {
            if (agentList[i].transform.tag == "ActiveAgent")
                AgentMove_Evacuee(i);
            else if (agentList[i].transform.tag == "Volunteer")
            {
                AgentMove_Volunteer(i);
            }
            
            // if (agentList[i].transform.tag == "Volunteer")
            //     AgentMove_Volunteer(i);
        }

        foreach (int i in random_agent)
        {
            StartCoroutine(AgentMove_Animation(i, 0f));
        }
    }

    void AgentMove_Evacuee(int i)
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

            if (fm.isValidCell(cell) && fm.isEmptyCell(cell))
            {
                float ff_value = ff.ff[cell.x, cell.y];
                if (lastPos[i] == cell)
                    ff_value = ff_value - gui.kD;
                possiblePos.Add((cell, Mathf.Exp(ff_value)));
            }
        }

        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => -a.Item2.CompareTo(b.Item2));

        for (int k = possiblePos.Count - 1; k >= 0; k--)
        {
            if (possiblePos[k].Item2 >= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, k + 1);
                Vector2Int cell = possiblePos[rnd].Item1;
                if (cell == currentPos[i]) return;

                dff.dff[currentPos[i].x,currentPos[i].y] += 1f;
                lastPos[i] = currentPos[i];
                currentPos[i] = cell;
                Vector3 direct = fm.floor[currentPos[i].x, currentPos[i].y].transform.position - fm.floor[lastPos[i].x, lastPos[i].y].transform.position;
                direct = Vector3.Normalize(direct);
                lastRot[i] = agentList[i].transform.rotation;
                currentRot[i] = Quaternion.LookRotation(direct);
                agentList[i].transform.parent = fm.floor[cell.x, cell.y].transform;

                if (fm.floor[cell.x, cell.y].transform.tag == "Exit")
                    SetAgentType(agentList[i],"ExitAgent");

                return;
            }
        }
    }

    void AgentMove_Volunteer(int i)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        SpecificFloorField specific_ff = agentList[i].GetComponent<SpecificFloorField>();
        ObstacleModel om = FindObjectOfType<ObstacleModel>();
        Vector2Int obstaclePos = om.currentPos[inChargeOfList[i]];
        Vector2Int volunteerPos = currentPos[i];

        if(om.LowTauDensity(inChargeOfList[i]))
        {
            Debug.Log("obstaclePos: " + obstaclePos.ToString());
            Debug.Log("density: " + om.densityList[inChargeOfList[i]].ToString());
            om.SetObstacleType(om.obstacleList[inChargeOfList[i]],"MovableObstacle");
            SetAgentType(agentList[i],"ActiveAgent");
            AgentMove_Evacuee(i);
            return;
        }

        List<(Vector2Int, float)> possiblePos = new List<(Vector2Int, float)>();

        for (int m = -1; m <= 1; m++)
        for (int n = -1; n <= 1; n++)
        {
            Vector2Int cell = obstaclePos + new Vector2Int(m, n);
            

            if (fm.isValidCell(cell) && fm.isEmptyCell(cell) && !fm.isExitCell(cell))
            {
                float ff_value = -1f * specific_ff.sff[cell.x, cell.y];
                possiblePos.Add((cell, ff_value));
            }
        }

        if (possiblePos.Count == 0) return;

        possiblePos.Sort((a, b) => -a.Item2.CompareTo(b.Item2));

        for (int k = possiblePos.Count - 1; k >= 0; k--)
        {
            if (possiblePos[k].Item2 >= possiblePos[0].Item2)
            {
                int rnd = Random.Range(0, k + 1);
                Vector2Int cell = possiblePos[rnd].Item1;
                if (cell == obstaclePos && possiblePos.Count > 1)
                    cell = possiblePos[(rnd + 1) % possiblePos.Count].Item1;
                else if (cell == obstaclePos)
                    return;

                GetPosition_MoveObstacle(ref volunteerPos, ref obstaclePos, cell);
                if(obstaclePos != om.currentPos[inChargeOfList[i]])
                    gui.flg_update_sff = true;

                lastPos[i] = currentPos[i];
                currentPos[i] = volunteerPos;
                Vector3 direct = fm.floor[obstaclePos.x, obstaclePos.y].transform.position - fm.floor[currentPos[i].x, currentPos[i].y].transform.position;
                direct = Vector3.Normalize(direct);
                lastRot[i] = agentList[i].transform.rotation;
                currentRot[i] = Quaternion.LookRotation(direct);
                agentList[i].transform.parent = fm.floor[currentPos[i].x, currentPos[i].y].transform;

                // om.currentPos[inChargeOfList[i]] = obstaclePos;
                om.obstacleList[inChargeOfList[i]].transform.parent = fm.floor[obstaclePos.x, obstaclePos.y].transform;
                // TODO: animation
                // float planeSize = fm.plane.transform.GetComponent<Renderer>().bounds.size.x;
                // om.obstacleList[inChargeOfList[i]].transform.localScale = Vector3.one * (planeSize / om.obstacleSize);
                // om.obstacleList[inChargeOfList[i]].transform.position = fm.floor[obstaclePos.x, obstaclePos.y].transform.position;
                // om.obstacleList[inChargeOfList[i]].transform.position += Vector3.up * planeSize / 2f;
                StartCoroutine(om.ObstacleMove_Animation(inChargeOfList[i], obstaclePos, 0f));
                //
                if (obstaclePos == specific_ff.destination)
                {
                    om.SetObstacleType(om.obstacleList[inChargeOfList[i]], "ImmovableObstacle");
                    SetAgentType(agentList[i], "ActiveAgent");
                }
                if (gui.flg_update_sff)
                    specific_ff.Setup();
                return;
            }
        }
    }

    void GetPosition_MoveObstacle(ref Vector2Int volunteerPos, ref Vector2Int obstaclePos, Vector2Int targetPos)
    {
        FloorModel fm = FindObjectOfType<FloorModel>();
        Vector2 n = obstaclePos - volunteerPos;
        Vector2 f = targetPos - obstaclePos;
        if (Vector2Int.Distance(volunteerPos, targetPos) >= Vector2Int.Distance(obstaclePos, targetPos))
        {
            if (Vector2.Angle(n, f) <= 45)
            {
                Debug.Log("Push");
                volunteerPos = obstaclePos;
                obstaclePos = targetPos;
            }
            else if (n.x * f.y - n.y * f.x <= 0f)
            {
                Debug.Log("Rotate 1");
                Vector2 rotate = Quaternion.AngleAxis(-45f, Vector3.forward) * n;
                targetPos = volunteerPos + Vector2Int.RoundToInt(rotate);
                if (fm.isValidCell(targetPos) && fm.isEmptyCell(targetPos) && !fm.isExitCell(targetPos))
                    obstaclePos = targetPos;
            }
            else 
            {
                Debug.Log("Rotate 2");
                Vector2 rotate = Quaternion.AngleAxis(45f, Vector3.forward) * n;
                targetPos = volunteerPos + Vector2Int.RoundToInt(rotate);
                if (fm.isValidCell(targetPos) && fm.isEmptyCell(targetPos) && !fm.isExitCell(targetPos))
                    obstaclePos = targetPos;
            }
        }
        else
        {
            bool toPull = (Random.value > 0.5f);
            List<Vector2Int> candidates = new List<Vector2Int>();
            if (toPull)
            {
                Debug.Log("Pull");
                Vector2Int[] toAdd = {
                    volunteerPos - Vector2Int.RoundToInt(n),
                    volunteerPos - Vector2Int.RoundToInt(Quaternion.AngleAxis(-45f, Vector3.forward) * n),
                    volunteerPos - Vector2Int.RoundToInt(Quaternion.AngleAxis( 45f, Vector3.forward) * n)
                };
                foreach (Vector2Int candidate in toAdd)
                {
                    if (fm.isValidCell(candidate) && fm.isEmptyCell(candidate) && !fm.isExitCell(candidate))
                        candidates.Add(candidate);
                }
            }
            else
            {
                Debug.Log("Change");
                for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    Vector2Int cell = obstaclePos + new Vector2Int(i, j);
                    if (cell == obstaclePos) continue;
                    if (fm.isValidCell(cell) && fm.isEmptyCell(cell) && !fm.isExitCell(cell))
                        if (Mathf.Abs(cell.x - volunteerPos.x) <= 1 &&
                            Mathf.Abs(cell.y - volunteerPos.y) <= 1)
                            candidates.Add(cell);
                }
            }
            if (candidates.Count > 0)
            {
                obstaclePos = volunteerPos;
                volunteerPos = candidates[Random.Range(0, candidates.Count)];
            }
        }
    }

    void RemoveExitAgents()
    {
        for(int i = 0; i < agentNumber;)
        {
            if(agentList[i].transform.tag == "ExitAgent")
            {
                DestroyImmediate(agentList[i]);
                agentList.RemoveAt(i);
                lastPos.RemoveAt(i);
                currentPos.RemoveAt(i);
                lastRot.RemoveAt(i);
                currentRot.RemoveAt(i);
                volunteerStrategy.RemoveAt(i);
                whiteList.RemoveAt(i);
                blackList.RemoveAt(i);
                inChargeOfList.RemoveAt(i);
                agentNumber--;
            }
            else i++;
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
        float rotation_duration = 0.4f;
        Vector2Int lastCell = lastPos[i];
        Vector2Int curCell = currentPos[i];
        Vector3 last = fm.floor[lastCell.x, lastCell.y].transform.position;
        Vector3 current = fm.floor[curCell.x, curCell.y].transform.position;
        last.y = agentList[i].transform.position.y;
        current.y = agentList[i].transform.position.y;
        
        Quaternion lastRotation = lastRot[i];
        Quaternion currentRotation = currentRot[i];
        Animator anima = agentList[i].transform.GetComponent<Animator>();

        // animation.SetValue("Walk", false);
        // animation.SetValue("MoveObs", false);
        if (lastCell != curCell && agentList[i].transform.tag == "ActiveAgent")
            anima.SetBool("Walk", true);
        else if ((lastCell != curCell || lastRotation != currentRotation) && agentList[i].transform.tag == "Volunteer")
            anima.SetBool("MoveObs", true);
        if(i == 0)
        {
            Debug.Log("Walk: " + anima.GetBool("Walk").ToString());
            Debug.Log("MoveObs: " + anima.GetBool("MoveObs").ToString());
        }
        
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            agentList[i].transform.position = Vector3.Lerp(last, current, timer/duration);
            agentList[i].transform.rotation = Quaternion.Lerp(lastRotation,currentRotation,timer/rotation_duration);
            yield return null;
        }

        anima.SetBool("Walk", false);
        anima.SetBool("MoveObs", false);
        yield break;
    }

    public int GetIndexFromCell(Vector2Int cell)
    {
        int idx = currentPos.FindIndex(pos => pos == cell);
        return idx;
    }

    public void SetAgentType(GameObject agent, string type)
    {
        Renderer[] renders = {
            agent.transform.Find("bottom").GetComponent<Renderer>(),
            agent.transform.Find("top").GetComponent<Renderer>(),
            agent.transform.Find("face").GetComponent<Renderer>(),
            agent.transform.Find("hair").GetComponent<Renderer>()
        };
    
        switch (type)
        {
            case "ActiveAgent":
                // B03, A06, C04, C04
                agent.transform.tag = type;
                renders[0].material = Resources.Load<Material>("Materials/B03");
                renders[1].material = Resources.Load<Material>("Materials/A06");
                renders[2].material = Resources.Load<Material>("Materials/C04");
                renders[3].material = Resources.Load<Material>("Materials/C04");
                break;
            case "ExitAgent":
                // C06, C06, C04, C04
                agent.transform.tag = type;
                renders[0].material = Resources.Load<Material>("Materials/C06");
                renders[1].material = Resources.Load<Material>("Materials/C06");
                renders[2].material = Resources.Load<Material>("Materials/C04");
                renders[3].material = Resources.Load<Material>("Materials/C04");
                break;
            case "Volunteer":
                // A04, C03, C04, C04
                agent.transform.tag = type;
                renders[0].material = Resources.Load<Material>("Materials/A04");
                renders[1].material = Resources.Load<Material>("Materials/C03");
                renders[2].material = Resources.Load<Material>("Materials/C04");
                renders[3].material = Resources.Load<Material>("Materials/C04");
                break;
            default:
                break;
        }
    }

    public Vector2Int GetDestination(int i)
    {
        if (agentList[i].transform.tag != "Volunteer")
            return Vector2Int.zero;
        else
            return agentList[i].GetComponent<SpecificFloorField>().destination;
    }
}
