﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ATC : MonoBehaviour
{
    public static ATC e;
    private void Awake()
    {
        e = this;
    }

    public GameObject planePrefab;
    public Runway runway;

    public List<Gate> gates = new List<Gate>();

    public List<Plane> planes = new List<Plane>();

    public float gateTaxiwayZ = 10;

    public Gate GetFreeGate()
    {
        // TODO: find random instead of first
        foreach (var gate in gates)
        {
            if (gate.plane == null) return gate;
        }

        return null;
    }

    void Start()
    {
        SpawnTakingOffPlane();
    }

    void Update()
    {
        if (!runway.inUse)
        {
            SpawnLandingPlane();
        }
    }

    void SpawnLandingPlane()
    {
        if (runway.inUse) return;
        Gate gate = GetFreeGate();
        if (gate == null) return;

        Plane plane = CreatePlane();

        plane.state = Plane.State.Landing;
        plane.gate = gate;

        gate.plane = plane;
        runway.inUse = true;
    }

    void SpawnTakingOffPlane()
    {
        Plane plane = CreatePlane();
        plane.state = Plane.State.TakingOff;

        plane.transform.position = Vector3.zero;
        plane.transform.forward = -Vector3.right;

        runway.inUse = true;
    }

    Plane CreatePlane()
    {
        GameObject planeGO = Instantiate(planePrefab);
        Plane plane = planeGO.GetComponent<Plane>();

        plane.runway = runway;
        planes.Add(plane);

        return plane;
    }

    public Vector3[] GetTaxiwayToGate(Vector3 inputPos, Vector3 inputFrw, Gate gate)
    {
        int index;
        if (inputFrw.x < 0)
            index = Mathf.FloorToInt(inputPos.x / runway.taxiwayDistance);
        else
            index = Mathf.CeilToInt(inputPos.x / runway.taxiwayDistance);

        Debug.Log(index);

        float taxix = index * runway.taxiwayDistance;

        Vector3 p0 = new Vector3(taxix, 0, runway.transform.position.z);
        Vector3 p1 = new Vector3(taxix, 0, gateTaxiwayZ);
        Vector3 p2 = new Vector3(gate.transform.position.x, 0, gateTaxiwayZ);
        Vector3 p3 = new Vector3(gate.transform.position.x, 0, gate.transform.position.z);

        Vector3[] taxiwayPoints = new Vector3[] { p0, p1, p2, p3 };

        return taxiwayPoints;
    }

    const float gizmoL = 1000;

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(
            new Vector3(-gizmoL, 0, gateTaxiwayZ),
            new Vector3(gizmoL, 0, gateTaxiwayZ));

        if (runway)
        {
            Gizmos.color = runway.inUse ? Color.red : Color.green;
            Gizmos.DrawLine(
                new Vector3(-gizmoL, 0, runway.transform.position.z),
                new Vector3(gizmoL, 0, runway.transform.position.z));

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(
                new Vector3(-gizmoL, 0, runway.transform.position.z + runway.taxiwayThreshold),
                new Vector3(gizmoL, 0, runway.transform.position.z + runway.taxiwayThreshold));
        }

        Gizmos.color = Color.yellow;
        for (int i = -10; i < 10; i++)
        {
            Gizmos.DrawLine(
                new Vector3(i * runway.taxiwayDistance, 0, gateTaxiwayZ),
                new Vector3(i * runway.taxiwayDistance, 0, runway.transform.position.z));
        }
    }
}