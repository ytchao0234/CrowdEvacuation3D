using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovment : MonoBehaviour
{
    private Vector3 CameraPosition;
    [Header("Camera Setting")]
    public float CameraSpeed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        CameraPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKey(KeyCode.W))
            CameraPosition.z += CameraSpeed / 10;
        if(Input.GetKey(KeyCode.S))
            CameraPosition.z -= CameraSpeed / 10;
        if(Input.GetKey(KeyCode.A))
            CameraPosition.x -= CameraSpeed / 10;
        if(Input.GetKey(KeyCode.D))
            CameraPosition.x += CameraSpeed / 10;
        if(Input.GetKey(KeyCode.E))
            CameraPosition.y -= CameraSpeed / 10;
        if(Input.GetKey(KeyCode.Q))
            CameraPosition.y += CameraSpeed / 10;
        this.transform.position = CameraPosition;
    }

    public void Setup()
    {
        // AgentManager am = FindObjectOfType<AgentManager>();
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();

        Vector2Int cell = new Vector2Int(gui.planeRow - 1,gui.planeCol / 2);
        CameraPosition = fm.floor[cell.x,cell.y].transform.position + Vector3.left * 5f + Vector3.back * 15f + Vector3.up * 50f;
        // new Vector3(fm.floor[cell.x,cell.y].transform.position.x,fm.floor[cell.x,cell.y].y + 50,0);
        this.transform.eulerAngles = new Vector3(45f,0f,0f);
    }
}
