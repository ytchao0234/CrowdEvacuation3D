using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ObstacleModel : MonoBehaviour
{
    public GameObject obstacle;
    float obstacleSize;
    List<GameObject> obstacleList;
    List<GameObject> wallList;

    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        
        obstacleList = new List<GameObject>();
        wallList = new List<GameObject>();
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

    public void Reset()
    {
        for (int i = 0; i < obstacleList.Count; i++)
        {
            // DestroyImmediate(obstacleList[i]);
            Destroy(obstacleList[i], 0f);
        }
        for(int i=0; i< wallList.Count;i++)
        {
            Destroy(wallList[i], 0f);
        }
        obstacleList.Clear();
        wallList.Clear();
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
            obstacleList.Add(obj);
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
                obj.GetComponent<Renderer>().material.color = Color.cyan * 0.8f;
                break;
            default:
                break;
        }
    }
}
