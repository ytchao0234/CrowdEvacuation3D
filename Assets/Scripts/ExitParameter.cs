using UnityEngine;
using System.Collections;

[System.Serializable]
public class ExitParameter  {

    [Range(0f, 1f)]
    public float position;
    [Range(0f, 1f)]
    public float width;
}
