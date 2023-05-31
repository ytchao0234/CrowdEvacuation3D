using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnticipationFloorField : MonoBehaviour
{
    public float[,] aff;
    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        aff = new float[gui.planeRow, gui.planeCol];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAFF()
    {
        Setup();
    }

    public void Setup()
    {
        Reset();
        AgentManager am = FindObjectOfType<AgentManager>();
        for (int i = 0; i < am.agentList.Count; i++)
        {
            if (am.agentList[i].transform.tag != "Volunteer")
                continue;
            SetAFF_OneVolunteer(i);
        }
    }

    public void Reset()
    {
        GUI gui = FindObjectOfType<GUI>();
        // Set initial values
        for (int i = 0; i < gui.planeRow; i++)
        for (int j = 0; j < gui.planeCol; j++)
        {
            aff[i, j] = 0f;
        }
    }

    void SetAFF_OneVolunteer(int volunteer_idx)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        AgentManager am = FindObjectOfType<AgentManager>();
        ObstacleModel om = FindObjectOfType<ObstacleModel>();

        Vector2Int volunteerPos = am.currentPos[volunteer_idx];
        Vector2Int obstaclePos = om.currentPos[am.inChargeOfList[volunteer_idx]];
        int radius = gui.aff_radius;
        
        // Fan-Shape
        for (int i = -radius; i <= radius; i++)
        for (int j = -radius; j <= radius; j++)
        {
            Vector2Int targetPos = volunteerPos + new Vector2Int(i, j);
            if (!fm.isValidCell(targetPos)) continue;
            Vector2 n = obstaclePos - volunteerPos;
            Vector2 f = targetPos - volunteerPos;
            if (Vector2.Angle(n, f) > 45) continue;
            SetAFFValue(obstaclePos, targetPos, radius);

        }
        // Circle
        for (int i = -radius / 2; i <= radius / 2; i++)
        for (int j = -radius / 2; j <= radius / 2; j++)
        {
            Vector2Int targetPos = volunteerPos + new Vector2Int(i, j);
            if (!fm.isValidCell(targetPos)) continue;
            Vector2 n = obstaclePos - volunteerPos;
            Vector2 f = targetPos - volunteerPos;
            if (Vector2.Angle(n, f) <= 45) continue;
            SetAFFValue(volunteerPos, targetPos, radius / 2);
        }
    }

    void SetAFFValue(Vector2Int center, Vector2Int cell, int radius)
    {
        float distance = GetDistance(center, cell);
        if (distance >= 0 && distance <= 1f * radius / 3f)
            aff[cell.x, cell.y] += 3f;
        else if (distance > 1f * radius / 3f && distance <= 2f * radius / 3f)
            aff[cell.x, cell.y] += 2f;
        else if (distance > 2f * radius / 3f && distance <= radius)
            aff[cell.x, cell.y] += 1f;
    }

    float GetDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return 1.5f * Mathf.Min(
            Mathf.Abs(pos1.x - pos2.x),
            Mathf.Abs(pos1.y - pos2.y)
        ) + Mathf.Abs (
            Mathf.Abs(pos1.x - pos2.x) -
            Mathf.Abs(pos1.y - pos2.y)
        );
    }
}
