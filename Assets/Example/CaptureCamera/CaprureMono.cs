using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CaprureMono : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        
    }


    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);
        Handles.color = Color.red;
        Handles.DrawLine(Vector3.zero, Vector3.one * 5);
        Handles.Label(Vector3.zero, "Fuck");
    }

}
