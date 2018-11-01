using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apron : MonoBehaviour
{
    public Runway runway;

    public float pushbackTurnOffset = -10;

    const float gizmoL = 5000;
    public Color taxiwayColor;

    public Vector3 test_point;
    public int test_offset;

    public bool IsPastPushbackTurnPoint(Vector3 inputWorldPosition)
    {
        float x = transform.InverseTransformPoint(inputWorldPosition).x;
        return x > pushbackTurnOffset;
    }

    Vector3 GetApronTaxiwayPoint(Vector3 inputPosition, int offset)
    {
        float taxiZ = GetLocalZ(inputPosition, offset);

        return transform.TransformPoint(new Vector3(0, 0, taxiZ));
    }

    Vector3 GetRunwayTaxiwayPoint(Vector3 inputPosition, int offset)
    {
        float taxiZ = GetLocalZ(inputPosition, offset);

        //Vector3 runwayPoint = transform.TransformPoint(new Vector3(0, 0, taxiZ));
        Vector3 apronPoint = transform.TransformPoint(new Vector3(0, 0, taxiZ));
        Vector3 runwayPoint = runway.transform.InverseTransformPoint(apronPoint);
        runwayPoint.x = 0;
        return runway.transform.TransformPoint(runwayPoint);
    }

    float GetLocalZ(Vector3 inputPosition, int offset)
    {
        Vector3 localInput = transform.InverseTransformPoint(inputPosition);
        float inputZ = localInput.z;

        int index = Mathf.CeilToInt(inputZ / runway.taxiwayDistance) + offset;
        return index * runway.taxiwayDistance;
    }

    void OnDrawGizmos()
    {
        Color yellowish = Color.yellow;
        yellowish.a = 0.3f;
        Gizmos.color = yellowish;
        Gizmos.DrawLine(
            transform.TransformPoint(new Vector3(pushbackTurnOffset, 0, -gizmoL)),
            transform.TransformPoint(new Vector3(pushbackTurnOffset, 0, gizmoL)));


        Gizmos.color = taxiwayColor;
        Gizmos.DrawLine(
            transform.TransformPoint(new Vector3(0, 0, -gizmoL)),
            transform.TransformPoint(new Vector3(0, 0, gizmoL)));

        if (runway)
        {
            for (int i = -10; i < 10; i++)
            {
                Vector3 apronPoint = transform.TransformPoint(new Vector3(0, 0, i * runway.taxiwayDistance));
                //Vector3 runwayPoint = transform.TransformPoint(apronPoint);
                Vector3 runwayPoint = runway.transform.InverseTransformPoint(apronPoint);
                runwayPoint.x = 0;
                runwayPoint = runway.transform.TransformPoint(runwayPoint);

                Gizmos.DrawLine(apronPoint, runwayPoint);
            }

            // temp test
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(test_point, 50);
            Gizmos.DrawSphere(GetApronTaxiwayPoint(test_point, test_offset), 50);
            Gizmos.DrawSphere(GetRunwayTaxiwayPoint(test_point, test_offset), 50);
        }
    }
}
