using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.ProcPort;

public class Test_GetTaxiway : MonoBehaviour
{
    public Gate gate;
    public ATC atc;

    public Vector3[] points;

    private void OnDrawGizmos()
    {
        if (!gate || !atc) return;

        points = atc.GetTaxiwayToGate(transform.position, transform.forward, gate);

        if (points != null && points.Length != 0)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < points.Length - 1; i++)
            {
                //Gizmos.DrawWireSphere(points[i], validationDistance);
                Gizmos.DrawLine(points[i] + Vector3.up * 10, points[i + 1] + Vector3.up * 10);
            }
        }
    }
}
