using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public GameObject agent;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel floorModel = FindObjectOfType<FloorModel>();
    
        float agentDensity = gui.agentDensity;
        int planeRow = gui.planeRow;
        int planeCol = gui.planeCol;
        int agentNumber = Mathf.FloorToInt(planeRow * planeCol * agentDensity);
        float agentHeight = agent.transform.GetComponent<Renderer>().bounds.size.y;

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

            if (floorModel.checkValidPlane(i, j))
            {
                GameObject obj = GameObject.Instantiate(agent, floorModel.floor[i, j].transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.position += Vector3.up * agentHeight / 2f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
