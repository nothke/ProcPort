using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public float startAlt = 100;
    public float landingAngle = 3;
    public float landingSpeed = 10;

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

    public float runwayTaxiSpeed = 5;

    public float landingAoA = 5;
    public float flareBeginAlt = 10;
    public float flareMaximumAngle;
    public float noseDropTime = 5;
    float speed = 0;
    public float validationDistance = 1;

    void StateChange()
    {
        switch (state)
        {
            case State.Landing:
                float x = startAlt * Mathf.Tan((90 - landingAngle) * Mathf.Deg2Rad);
                Debug.Log(x);
                transform.position = runway.transform.position + new Vector3(x, startAlt, 0);
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

    float spawnTime;

    public AnimationCurve flareAngleCurve;
    public float flareAngleHeightMult = 0.2f;
    float touchdownTime;
    bool touchedDown;

    float flare;
    float flareSign;

    [Header("Random")]
    public float randomXMult = 5;
    public float randomFreq = 0.01f;
    public float randomBankMult = 10;

    Vector3 LandingFrw()
    {
        return Quaternion.Euler(0, 0, landingAngle) * -Vector3.right;
    }

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

            //float alt = Time.deltaTime * landingSpeed;

            float alt = transform.position.y;

            Vector3 veloDir = -Vector3.right;
            float flareAltMult = 1.0f / flareBeginAlt;

            float zAdd = 0;

            if (!touchedDown)
            {
                zAdd = (-0.5f + Mathf.PerlinNoise(52.223f, alt * randomFreq));

                speed = landingSpeed;

                float angle = Mathf.Lerp(0.1f, landingAngle, alt * 0.2f);
                veloDir = Quaternion.Euler(0, 0, angle) * -Vector3.right;
                flare = Mathf.Lerp(flareMaximumAngle, landingAoA, alt * flareAltMult);
            }
            else
            {
                if (speed > runwayTaxiSpeed)
                {
                    speed -= Time.deltaTime;
                }

                float flareSign = Mathf.Sign(flareMaximumAngle);

                if (flare * flareSign > 0)
                {
                    flare -= flareSign * Time.deltaTime * 2;
                }
            }

            if (alt < 0 && !touchedDown)
            {
                Vector3 v = transform.position;
                v.y = 0;
                transform.position = v;
                veloDir = -Vector3.right;
                flareSign = Mathf.Sign(flareMaximumAngle);
                touchedDown = true;
                Debug.Log("They touch Martin!");
            }

            //float flareTime += Time.deltaTime;
            //float flare = flareAngleCurve.Evaluate(alt * flareAngleHeightMult);
            Vector3 frw = Quaternion.Euler(0, 0, flare) * veloDir;

            Vector3 up = new Vector3(0, 1, zAdd * randomBankMult).normalized;
            transform.rotation = Quaternion.LookRotation(frw, up);
            transform.position += veloDir * speed * Time.deltaTime;
            Vector3 p = transform.position;
            p.z = runway.transform.position.z + zAdd * randomXMult;
            transform.position = p;
            if (touchedDown && speed <= runwayTaxiSpeed)
            {
                // END
                speed = runwayTaxiSpeed;

                // Find a closest taxiway
                taxiwayPoints = ATC.e.GetTaxiwayToGate(transform.position, frw, gate);

                state = State.TaxiingToGate;
            }

            /*
            float alt = landingSlope.Evaluate(progress) * heightMult;
            */
            /*
            transform.position = runway.transform.position +
                new Vector3(
                -progress * landingLength,
                alt,
                0);*/

            //Vector3 frw = runway.transform.forward + Vector3.up * rotationCurve.Evaluate(progress);


            /*
            if (progress > 1)
            {
                speed = landingLength * progressSpeed * Time.deltaTime * speedCurve.Evaluate(1);

                // orient to runway
                transform.forward = runway.transform.forward;

                // Find a closest taxiway
                //Vector3 pos = transform.position;
                taxiwayPoints = ATC.e.GetTaxiwayToGate(pos, gate);

                state = State.TaxiingToGate;
            }*/
        }

        if (state == State.TaxiingToGate)
        {
            // follow path
            transform.position += transform.forward * speed * Time.deltaTime;

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

    public float turnSpeed = 1;

    void GetSteer(Vector3 target)
    {
        Vector3 point = target;
        Vector3 steerVector = transform.InverseTransformPoint(point);

        float turn = steerVector.x / steerVector.magnitude * Time.deltaTime;

        transform.rotation *= Quaternion.AngleAxis(turn * turnSpeed, Vector3.up);
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
}
