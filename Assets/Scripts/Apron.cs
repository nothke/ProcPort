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

    public Vector3 GetGateApronPoint(Gate gate)
    {
        Vector2 point;

        Vector2 p1 = new Vector2(gate.transform.position.x, gate.transform.position.z);
        Vector2 p2 = new Vector2(
            (gate.transform.position + gate.transform.forward * 10).x,
            (gate.transform.position + gate.transform.forward * 10).z);

        Vector2 p3 = new Vector2(transform.position.x, transform.position.z);
        Vector2 p4 = new Vector2(
            (transform.position + transform.forward * 10).x,
            (transform.position + transform.forward * 10).z);

        Vector2Utils.LineIntersection(p1, p2, p3, p4, out point);

        Vector3 p = new Vector3(point.x, 0, point.y);
        return p;
    }

    public Vector3 GetApronTaxiwayPoint(Vector3 inputPosition, int offset)
    {
        return GetRoundedPointOnApron(inputPosition, offset);
    }

    public Vector3 GetRunwayPointFromApron(Vector3 inputPosition, int offset)
    {
        Vector3 apronPoint = GetRoundedPointOnApron(inputPosition, offset);
        Vector3 runwayPoint = runway.transform.InverseTransformPoint(apronPoint);
        runwayPoint.x = 0;
        return runway.transform.TransformPoint(runwayPoint);
    }

    public void GetTaxiwayPointsFromRunway(Vector3 inputPosition, int offset,
        out Vector3 runwayPoint, out Vector3 apronPoint)
    {
        Vector3 pointOnApron = ProjectRunwayPointToApron(inputPosition);
        Debug.DrawRay(pointOnApron, Vector3.up * 100);

        Vector3 taxiwayPointOnApron = GetRoundedPointOnApron(pointOnApron, offset);
        Debug.DrawRay(taxiwayPointOnApron, Vector3.up * 200, Color.yellow);

        runwayPoint = runway.transform.InverseTransformPoint(taxiwayPointOnApron);
        runwayPoint.x = 0;

        apronPoint = taxiwayPointOnApron;
        runwayPoint = runway.transform.TransformPoint(runwayPoint);
    }

    Vector3 ProjectRunwayPointToApron(Vector3 input)
    {
        Vector2 inPos = new Vector2(input.x, input.z);
        Vector3 runDir = runway.transform.forward;
        Vector2 inDir = new Vector2(-runDir.z, runDir.x);

        Vector2 apronPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 apronDir = new Vector2(transform.forward.x, transform.forward.z);

        Vector2 intersection = Vector2.zero;
        Vector2Utils.RayRayIntersection(inPos, inDir, apronPos, apronDir, ref intersection);

        return new Vector3(intersection.x, 0, intersection.y);
    }

    Vector3 GetRoundedPointOnApron(Vector3 inputPosition, int offset)
    {
        float inputZ = GetLocalZ(inputPosition, offset);
        return transform.TransformPoint(0, 0, inputZ);
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

            /* temp test
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(test_point, 50);
            GetTaxiwayPointsFromRunway(test_point, test_offset, out Vector3 test_runwayPoint, out Vector3 test_apronPoint);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(test_runwayPoint, 50);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(test_apronPoint, 50);
            */
        }
    }


}
