using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{

    public AnimationCurve landingSlope;
    public float landingLength;
    public float progressSpeed;
    public float heightMult = 100;
    public AnimationCurve takeoffSlope;
    public AnimationCurve speedCurve;
    public AnimationCurve rotationCurve;

    void Start()
    {
        StateChange();
    }

    public enum State { Landing, TakingOff, TaxiingToGate, AtGate, TaxiingToRunway }
    public State state;

    float progress;

    public Runway runway;

    State lastState;

    Vector3[] taxiwayPoints;
    int currentTarget = 0;

    float speed = 0;
    public float validationDistance = 1;

    void StateChange()
    {
        switch (state)
        {
            case State.Landing:
                break;
            case State.TakingOff:
                break;
            case State.TaxiingToGate:
                //runway.inUse = false;
                break;
            case State.AtGate:
                break;
            case State.TaxiingToRunway:
                break;
            default:
                break;
        }
    }

    public Gate gate;

    void Update()
    {
        // ON CHANGE
        if (state != lastState)
            StateChange();

        // UPDATE
        if (state == State.Landing)
        {
            if (!runway)
            {
                state = State.AtGate;
                Destroy(gameObject);
                return;
            }

            float alt = landingSlope.Evaluate(progress) * heightMult;

            transform.position = runway.transform.position +
                new Vector3(
                -progress * landingLength,
                alt,
                0);

            Vector3 frw = runway.transform.forward + Vector3.up * rotationCurve.Evaluate(progress);
            transform.rotation = Quaternion.LookRotation(frw);

            if (progress > 1)
            {
                speed = landingLength * progressSpeed * Time.deltaTime * speedCurve.Evaluate(1);

                // orient to runway
                transform.forward = runway.transform.forward;


                // Find a free gate
                //Gate gate = ATC.e.GetFreeGate();

                // Find a closest taxiway
                Vector3 pos = transform.position;
                taxiwayPoints = ATC.e.GetTaxiwayToGate(pos, gate);

                state = State.TaxiingToGate;
            }
        }

        if (state == State.TaxiingToGate)
        {
            // follow path
            transform.position += transform.forward * speed;

            Vector3 tgt = taxiwayPoints[currentTarget];
            GetSteer(tgt);

            // free runway
            if (runway && runway.inUse)
            {
                if (transform.position.z < runway.transform.position.z + runway.taxiwayThreshold)
                {
                    Debug.Log("Freeing runway", gameObject);
                    runway.inUse = false;
                    runway = null;
                }
            }

            // validate node
            if (Vector3.Distance(transform.position, tgt) < validationDistance)
            {
                currentTarget++;
                if (currentTarget >= taxiwayPoints.Length)
                {
                    taxiwayPoints = null;
                    state = State.AtGate;
                }
            }
        }

        progress += Time.deltaTime * progressSpeed * speedCurve.Evaluate(progress);

        lastState = state;
    }

    private void OnDrawGizmos()
    {
        if (taxiwayPoints != null && taxiwayPoints.Length != 0)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < taxiwayPoints.Length - 1; i++)
            {
                Gizmos.DrawWireSphere(taxiwayPoints[i], validationDistance);
                Gizmos.DrawLine(taxiwayPoints[i], taxiwayPoints[i + 1]);
            }
        }
    }

    public float turnSpeed = 1;

    void GetSteer(Vector3 target)
    {
        Vector3 point = target;
        Vector3 steerVector = transform.InverseTransformPoint(point);

        float turn = steerVector.x / steerVector.magnitude * Time.deltaTime;

        transform.rotation *= Quaternion.AngleAxis(turn * turnSpeed, Vector3.up);
    }
}
