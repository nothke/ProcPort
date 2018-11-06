using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : MonoBehaviour
{

    public float taxiwayDistance = 100;
    public float taxiwayThreshold = -10;

    public bool inUse;

    public bool IsPointPastThreshold(Vector3 inputPoint, float offset)
    {
        Vector3 local = transform.InverseTransformPoint(inputPoint);
        return local.x > taxiwayThreshold + offset;
    }

    const float gizmoL = 3000;

    private void OnDrawGizmos()
    {
        Gizmos.color = inUse ? Color.red : Color.green;
        Gizmos.DrawLine(
            transform.TransformPoint(new Vector3(0, 0, -gizmoL)),
            transform.TransformPoint(new Vector3(0, 0, gizmoL)));

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.TransformPoint(new Vector3(taxiwayThreshold, 0, -gizmoL)),
            transform.TransformPoint(new Vector3(taxiwayThreshold, 0, gizmoL)));
    }
}
