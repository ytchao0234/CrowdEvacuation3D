using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI : MonoBehaviour
{
    public int planeRow = 30;
    public int planeCol = 30;
    public float agentDensity = 0.3f;
    public Vector2Int[] exitPos = {
            new Vector2Int(0,16),
            new Vector2Int(29,14),
            new Vector2Int(29,15),
            new Vector2Int(29,16),
            new Vector2Int(29,17),
            new Vector2Int(29,18)
        };
    public float mS = 1.0f;
    public float sff_init_value = 1000f;
    public float sff_offset_hv = 1.0f;
    public float sff_offset_lambda = 1.5f;
    public float sff_offset_d = 1.0f * 1.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
