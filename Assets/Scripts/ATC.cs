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

    int landingRequests;

    public Queue<RunwayRequest> requestQueue = new Queue<RunwayRequest>();

    public class RunwayRequest
    {
        public Plane plane;
        public enum Type { Landing, TakeOff }
        public Type type;
    }

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
        QueuePlaneForLanding();
        QueuePlaneForLanding();
    }

    void Update()
    {
        ProcessQueue();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 1;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = 3;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Time.timeScale = 6;

        if (Input.GetKeyDown(KeyCode.Alpha4))
            Time.timeScale = 10;
    }

    [ContextMenu("Queue Landing Plane")]
    public void QueuePlaneForLanding()
    {
        RunwayRequest request = new RunwayRequest();
        request.type = RunwayRequest.Type.Landing;
        requestQueue.Enqueue(request);
    }

    void ProcessQueue()
    {
        if (runway.inUse) return;
        if (requestQueue.Count == 0) return;

        var request = requestQueue.Dequeue();

        if (request.type == RunwayRequest.Type.Landing)
        {
            Plane p = SpawnLandingPlane();
            if (!p) return;
            Debug.Log(p.name + " cleared for landing");
        }
        else
        {
            Debug.Log(request.plane.name + " cleared for takeoff");
            request.plane.ClearForTakeoff();
        }

        runway.inUse = true;
    }

    Plane SpawnLandingPlane()
    {
        // HMM WHAT TO DO?
        Gate gate = GetFreeGate();
        if (gate == null) return null;

        Plane plane = CreatePlane();

        plane.state = Plane.State.Landing;
        plane.gate = gate;

        gate.plane = plane;
        runway.inUse = true;

        return plane;
    }

    void Test_SpawnLandingPlane()
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

    void Test_SpawnTakingOffPlane()
    {
        Plane plane = CreatePlane();
        plane.state = Plane.State.TakingOff;

        plane.runway = runway;

        plane.transform.position = runway.transform.position;
        plane.transform.forward = -Vector3.right;

        runway.inUse = true;
    }

    void Test_SpawnDepartingPlane()
    {
        Gate gate = GetFreeGate();

        Plane plane = CreatePlane();
        plane.state = Plane.State.TaxiingToRunway;
        plane.gate = gate;
        gate.plane = plane;

        plane.transform.position = gate.transform.position;
        plane.transform.rotation = gate.transform.rotation;
    }


    Plane CreatePlane()
    {
        GameObject planeGO = Instantiate(planePrefab);
        Plane plane = planeGO.GetComponent<Plane>();

        plane.runway = runway;
        planes.Add(plane);

        plane.name = GetRandomPlaneName();

        return plane;
    }

    const string glyphs = "abcdefghijklmnopqrstuvwxyz"; //add the characters you want

    // TODO: Move to database or smth
    string GetRandomPlaneName()
    {
        string str = "";
        for (int i = 0; i < 5; i++)
        {
            if (i == 1) str += "-";
            str += char.ToUpper(glyphs[Random.Range(0, glyphs.Length)]);
        }

        return str;
    }

    public Vector3[] GetTaxiwayToGate(Vector3 inputPos, Vector3 inputFrw, Gate gate)
    {
        int index;
        if (inputFrw.x < 0)
            index = Mathf.FloorToInt(inputPos.x / runway.taxiwayDistance);
        else
            index = Mathf.CeilToInt(inputPos.x / runway.taxiwayDistance);

        //Debug.Log("Using taxiway: " + index);

        float taxix = index * runway.taxiwayDistance;

        Vector3 p0 = new Vector3(taxix, 0, runway.transform.position.z);
        Vector3 p1 = new Vector3(taxix, 0, gateTaxiwayZ);
        Vector3 p2 = new Vector3(gate.transform.position.x, 0, gateTaxiwayZ);
        Vector3 p3 = new Vector3(gate.transform.position.x, 0, gate.transform.position.z);

        Vector3[] taxiwayPoints = new Vector3[] { p0, p1, p2, p3 };

        return taxiwayPoints;
    }

    public Vector3[] GetTaxiwayToRunway(Gate gate)
    {
        Vector3 pos = gate.transform.position;

        int index;
        index = Mathf.CeilToInt(pos.x / runway.taxiwayDistance);

        Debug.Log(index);

        float taxix = index * runway.taxiwayDistance;

        Vector3 p0 = new Vector3(gate.transform.position.x, 0, gateTaxiwayZ);
        Vector3 p1 = new Vector3(taxix, 0, gateTaxiwayZ);
        Vector3 p2 = new Vector3(taxix, 0, runway.transform.position.z);
        Vector3 p3 = new Vector3(taxix - 80, 0, runway.transform.position.z);

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
