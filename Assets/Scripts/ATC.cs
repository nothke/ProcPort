using System.Collections;
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
    public Apron apron;

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

    public bool FreeGatesExist()
    {
        for (int i = 0; i < gates.Count; i++)
        {
            if (!gates[i].plane) return true;
        }

        return false;
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

    /*
    void Start()
    {
        //QueuePlaneForLanding();
        //QueuePlaneForLanding();
    }*/

    void Update()
    {
        ProcessQueue();
    }

    [ContextMenu("Queue Landing Plane")]
    public void QueuePlaneForLanding()
    {
        RunwayRequest request = new RunwayRequest();
        request.type = RunwayRequest.Type.Landing;
        requestQueue.Enqueue(request);
    }

    public void SubmitPlaneForLanding(Plane plane)
    {
        RunwayRequest request = new RunwayRequest();
        request.type = RunwayRequest.Type.Landing;
        request.plane = plane;
        requestQueue.Enqueue(request);
    }

    void ProcessQueue()
    {
        if (runway.inUse) return;
        if (requestQueue.Count == 0) return;

        var request = requestQueue.Dequeue();

        if (request.type == RunwayRequest.Type.Landing)
        {
            if (!FreeGatesExist())
            {
                Destroy(request.plane);
                Debug.LogError("Destroyed plane because no gates were available");
                return;
            }

            if (request.plane)
            {
                //request.plane.gameObject.SetActive(true);
                SetPlaneToLand(request.plane);
                Debug.Log(request.plane.name + " cleared for landing");
            }
            else
            {
                Plane p = SpawnLandingPlane();
                if (!p) return;
                Debug.Log(p.name + " cleared for landing");
            }
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

    void SetPlaneToLand(Plane plane)
    {
        // TODO: Does nothing if all gates are taken!
        Gate gate = GetFreeGate();
        if (gate == null) return;

        plane.gameObject.SetActive(true);

        plane.state = Plane.State.Landing;
        plane.gate = gate;

        gate.plane = plane;
        runway.inUse = true;
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

    public Plane CreatePlane()
    {
        GameObject planeGO = Instantiate(planePrefab);
        Plane plane = planeGO.GetComponent<Plane>();

        plane.runway = runway;
        planes.Add(plane);

        plane.name = GetRandomPlaneName();

        return plane;
    }

    public void RemovePlane(Plane plane)
    {
        planes.Remove(plane);
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
        if (gate == null)
        {
            Debug.LogError("No gate!");
            return null;
        }

        // direction dependent
        int offset = inputFrw.x < 0 ? 0 : 1;

        // Get point on runway and apron
        apron.GetTaxiwayPointsFromRunway(inputPos, offset,
            out Vector3 p0, out Vector3 p1);

        // apron point from gate
        Vector3 p2 = apron.GetGateApronPoint(gate);
        // gate
        Vector3 p3 = new Vector3(gate.transform.position.x, 0, gate.transform.position.z);

        Vector3[] taxiwayPoints = new Vector3[] { p0, p1, p2, p3 };

        return taxiwayPoints;
    }

    public Vector3[] GetTaxiwayToRunway(Gate gate)
    {
        Vector3 inputPos = gate.transform.position;

        //int index;
        //index = Mathf.CeilToInt(inputPos.x / runway.taxiwayDistance) + 1;

        //Debug.Log(index);

        //float taxix = index * runway.taxiwayDistance;

        Vector3 apronPos = gate.transform.position - apron.transform.forward * 500;

        Vector3 p0 = apron.GetGateApronPoint(gate);
        Vector3 p1 = apron.GetApronTaxiwayPoint(apronPos, 0);// new Vector3(taxix, 0, gateTaxiwayZ);
        Vector3 p2 = apron.GetRunwayPointFromApron(apronPos, 0);// new Vector3(taxix, 0, runway.transform.position.z);
        Vector3 p3 = p2 + runway.transform.forward * 80;

        Vector3[] taxiwayPoints = new Vector3[] { p0, p1, p2, p3 };

        return taxiwayPoints;
    }

    const float gizmoL = 5000;

    public Color taxiwayColor;
}
