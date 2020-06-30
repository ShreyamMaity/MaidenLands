using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    Vector3 from = new Vector3();
    Vector3 to = new Vector3();
    RaycastHit hit = new RaycastHit();

    void Start()
    {
    }

    void Update()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out hit))
        {
        }
    }

    void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, to, Color.green);
    }
}
