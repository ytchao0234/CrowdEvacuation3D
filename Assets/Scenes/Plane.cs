using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public Material material;
    void Awake()
    {
        float planeSize = transform.GetComponent<Renderer>().bounds.size.x;
        float startx = transform.position.x - planeSize / 2f;
        float startz = transform.position.z - planeSize / 2f;
        float endx = transform.position.x + planeSize / 2f;
        float endz = transform.position.z + planeSize / 2f;

        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        Vector3[] corners = {
            new Vector3(startx, 0f, startz),
            new Vector3(startx, 0f, endz),
            new Vector3(endx, 0f, endz),
            new Vector3(endx, 0f, startz),
            new Vector3(startx, 0f, startz)
        };
        Gradient color = new Gradient();
        color.SetKeys(
            new GradientColorKey [] {new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 1.0f)},
            new GradientAlphaKey [] {new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)}
        );
        lr.colorGradient = color;
        lr.positionCount = corners.Length;
        lr.SetPositions(corners);
        lr.material = material;
        lr.widthMultiplier = 0.5f;
    }

    void Update()
    {
        
    }
}
