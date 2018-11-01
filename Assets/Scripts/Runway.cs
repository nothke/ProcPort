using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : MonoBehaviour
{

    public float taxiwayDistance = 100;
    public float taxiwayThreshold = -10;

    public bool inUse;

    public float GetTaxiwayZ()
    {
        return transform.position.z + taxiwayDistance;
    }

    public float GetThreshold()
    {
        return transform.position.z + taxiwayThreshold;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
