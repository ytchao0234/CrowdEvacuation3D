using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleModel : MonoBehaviour
{
    public GameObject obstacle;
    float obstacleSize;

    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
    
        int planeRow = gui.planeRow;
        int planeCol = gui.planeCol;
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

        foreach (Vector2Int exit in gui.exitPos)
        {
            if (boundPos.Contains(exit))
                boundPos.Remove(exit);
        }

        foreach (Vector2Int bound in boundPos)
        {
            GameObject obj = GameObject.Instantiate(obstacle, fm.floor[bound.x, bound.y].transform);
            obj.transform.localScale = Vector3.one * (planeSize / obstacleSize);
            obj.transform.position += Vector3.up * planeSize / 2f;
            SetObstacleType(obj, "ImmovableObstacle");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetObstacleType(GameObject obj, string type)
    {
        switch (type)
        {
            case "ImmovableObstacle":
                obj.transform.tag = type;
                obj.GetComponent<Renderer>().material.color = Color.gray;
                break;
            case "MovableObstacle":
                obj.transform.tag = type;
                break;
            default:
                break;
        }
    }
}
