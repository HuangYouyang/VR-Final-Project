using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class calculateCenter : MonoBehaviour
{
    public GameObject mb;
    public Vector3 center;
    // Start is called before the first frame update
    void Start()
    {
        center = mb.GetComponent<MeshRenderer>().bounds.center;
    }   

    // Update is called once per frame
    void Update()
    {
        
    }
}
