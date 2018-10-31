using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{

    public Plane plane;

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, -transform.forward * 20);
        Gizmos.DrawLine(
            transform.position - transform.right * 4,
            transform.position + transform.right * 4);
    }

}
