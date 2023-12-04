using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lr;
    private Transform[] points;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();        
    }

    public void SetUpLine(Transform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;

        for(int i=0;i<points.Length; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }

    // private void Update()
    // {
    //     if(points!=null && points.Length!=0)
    //     {
    //         for(int i=0;i<points.Length; i++)
    //         {
    //             lr.SetPosition(i, points[i].position);
    //         }
    //     } 
    // }
}
