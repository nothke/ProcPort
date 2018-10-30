using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public float startAlt = 100;
    public float landingAngle = 3;
    public float landingSpeed = 10;

    //public float landingLength;
    //public float progressSpeed;
    //public float heightMult = 100;

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

    [Header("Landing")]
    public float landingAoA = 5;
    public float flareBeginAlt = 10;
    public float flareMaximumAngle;
    public float noseDropTime = 5;
    float speed = 0;
    public float validationDistance = 1;

    [Header("TakeOff")]
    public float speedUp = 1;
    public float startRotateAtSpeed = 30;
    public float rotateAngleRate = 5;
    public float rotateAngle = -7;
    public float takeoffSpeed = 35;
    public float takeoffMaxAngle = -10;
    public float takeOffAngleRate = 5;

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
                onGround = true;
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
    bool onGround;

    float flare;
    float flareSign;

    float curAngle = 0;

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

        float alt = transform.position.y;

        // UPDATE
        if (state == State.Landing)
        {

            if (!runway)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 veloDir = -Vector3.right;
            float flareAltMult = 1.0f / flareBeginAlt;

            float zAdd = 0;

            if (!onGround)
            {
                float zAddNoise = (-0.5f + Mathf.PerlinNoise(52.223f, alt * randomFreq));
                zAdd = Mathf.Lerp(0, zAddNoise, alt * 0.2f);

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

            if (alt < 0 && !onGround)
            {
                Vector3 v = transform.position;
                v.y = 0;
                transform.position = v;
                veloDir = -Vector3.right;
                flareSign = Mathf.Sign(flareMaximumAngle);
                onGround = true;
                Debug.Log("They touch Martin!");
            }
            Vector3 frw = Quaternion.Euler(0, 0, flare) * veloDir;

            Vector3 up = new Vector3(0, 1, zAdd * randomBankMult).normalized;
            transform.rotation = Quaternion.LookRotation(frw, up);
            transform.position += veloDir * speed * Time.deltaTime;
            Vector3 p = transform.position;
            p.z = runway.transform.position.z + zAdd * randomXMult;
            transform.position = p;
            if (onGround && speed <= runwayTaxiSpeed)
            {
                // END
                speed = runwayTaxiSpeed;

                // Find a closest taxiway
                taxiwayPoints = ATC.e.GetTaxiwayToGate(transform.position, frw, gate);

                state = State.TaxiingToGate;
            }
        }
        else if (state == State.TakingOff)
        {
            // --------
            // TAKE OFF
            // --------

            if (!runway)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 veloDir = Quaternion.Euler(0, 0, curAngle) * -Vector3.right;
            float flareAltMult = 1.0f / flareBeginAlt;

            float zAddNoise = (-0.5f + Mathf.PerlinNoise(52.223f, alt * randomFreq));
            float zAdd = Mathf.Lerp(0, zAddNoise, alt * 0.2f);

            if (onGround)
            {
                speed += Time.deltaTime * speedUp;

                /*
                if (speed > startRotateAtSpeed && flare > rotateAngle)
                {
                    flare -= Time.deltaTime * startRotateAtSpeed;
                }*/
            }

            float absAngle = Mathf.Abs(curAngle);

            if (speed > takeoffSpeed && absAngle < Mathf.Abs(takeoffMaxAngle))
            {
                //if (curAngle < takeoffMaxAngle)
                curAngle -= Time.deltaTime * takeOffAngleRate;

                veloDir = Quaternion.Euler(0, 0, curAngle) * -Vector3.right;
            }

            if (speed > startRotateAtSpeed && Mathf.Abs(flare) < Mathf.Abs(flareMaximumAngle))
            {
                flare -= Time.deltaTime * rotateAngleRate;
            }

            if (onGround && speed > takeoffSpeed)
            {
                onGround = false;
                runway.inUse = false;
                //runway = null;
            }

            Vector3 frw = Quaternion.Euler(0, 0, flare) * veloDir;

            Vector3 up = new Vector3(0, 1, zAdd * randomBankMult).normalized;
            transform.rotation = Quaternion.LookRotation(frw, up);
            transform.position += veloDir * speed * Time.deltaTime;

            Vector3 p = transform.position;
            p.z = runway.transform.position.z + zAdd * randomXMult;
            transform.position = p;

            if (alt > 500)
            {
                //Destroy(gameObject);
                return;
            }
        }
        else if (state == State.TaxiingToGate || state == State.TaxiingToRunway)
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

        //progress += Time.deltaTime * progressSpeed * speedCurve.Evaluate(progress);

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
