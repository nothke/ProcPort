using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{


    public enum State { Landing, TakingOff, TaxiingToGate, AtGate, TaxiingToRunway }
    public State state;

    float progress;

    public Runway runway;

    State lastState;

    Vector3[] taxiwayPoints;
    int currentTarget = 0;


    [Header("Landing")]
    public float startAlt = 100;
    public float startAlongRunway = 0;
    public float landingAngle = 3;
    public float landingSpeed = 10;

    public float landingAoA = 5;
    public float flareBeginAlt = 10;
    public float veloFlareBeginAlt = 80;
    public float touchdownAoA = 0.5f;
    public float flareMaximumAngle;
    public float landingBrakeMult = 2;
    public float noseDropTime = 5;
    float speed = 0;
    public float validationDistance = 1;

    [Header("Taxiing")]
    public float runwayTaxiSpeed = 12;
    public float pushBackSpeed = 5;
    public float gateTaxiSpeed = 5;
    public float gateSlowdownRange = 100;
    public float afterPushbackWait = 10;
    public float pushbackTurnOffset = -10;
    public float pushbackTurnRate = 10;
    public float pushabackBlockerOffset = -30;

    [Header("TakeOff")]
    public float speedUp = 1;
    public float startRotateAtSpeed = 30;
    public float rotateAngleRate = 5;
    public float rotateAngle = -7;
    public float takeoffSpeed = 35;
    public float takeoffMaxAngle = -10;
    public float takeOffAngleRate = 5;
    public float takeOffNoiseMult = 0.5f;

    const float THROTTLE_LANDING = 0.7f;
    const float THROTTLE_REVERSETHRUST = 1;
    const float THROTTLE_TAXI = 0.5f;
    const float THROTTLE_SLOW_TAXI = 0.3f;
    const float THROTTLE_TAKEOFF = 1;

    public void PlaceAtGate(Gate gate)
    {
        state = State.AtGate;
        this.gate = gate;

        speed = 0;
        transform.position = gate.transform.position;
        transform.rotation = gate.transform.rotation;
    }

    void Start()
    {
#if UNITY_EDITOR
        gizmoColor = Random.ColorHSV(0, 1, 0.8f, 0.8f, 1, 1);
        pointHeight = Random.Range(1.0f, 2.0f);
#endif

        StateChange();
    }

    void StateChange()
    {
        Debug.Log("State changed");

        switch (state)
        {
            case State.Landing:
                // calculates position from slope and start altitude
                float alongRunway = startAlongRunway - startAlt * Mathf.Tan((90 - landingAngle) * Mathf.Deg2Rad);
                trackedPosition = runway.transform.TransformPoint(new Vector3(0, startAlt, alongRunway));
                transform.position = trackedPosition;

                break;
            case State.TakingOff:
                Debug.Log(name + " has starting taking off");
                onGround = true;
                alreadyUnusedRunway = false;
                trackedPosition = transform.position;

                break;
            case State.TaxiingToGate:
                currentTarget = 0;
                transform.rotation = Random.rotation;

                break;
            case State.AtGate:

                break;
            case State.TaxiingToRunway:
                currentTarget = 0;
                isPushback = true;
                gate.plane = null;
                taxiwayPoints = ATC.e.GetTaxiwayToRunway(gate);

                SetPushbackPoint();

                break;
            default:
                break;
        }
    }


    void SetPushbackPoint()
    {
        pushbackPoint = ATC.e.apron.GetGateApronPoint(gate);
        pushbackPoint -= ATC.e.apron.transform.forward * pushabackBlockerOffset;
    }

    public Gate gate;

    float spawnTime;

    public AnimationCurve flareAngleCurve;
    public float flareAngleHeightMult = 0.2f;
    float touchdownTime;
    bool onGround;

    float flare;

    float curAngle = 0;

    [Header("Random")]
    public float randomXMult = 5;
    public float randomFreq = 0.01f;
    public float randomBankMult = 10;

    bool alreadyUnusedRunway;

    Vector3 trackedPosition;

    Vector3 LandingFrw()
    {
        return Quaternion.Euler(0, 0, landingAngle) * -Vector3.right;
    }

    public bool isPushback { get; private set; }

    float steerRate;
    float steerTarget;
    float steerAngle;
    float pushbackEndedTime;

    public float throttle { get; private set; }

    bool hasRunwayClearance;
    bool requestSent;

    public void ClearForTakeoff()
    {
        hasRunwayClearance = true;
    }

    bool slowingDownForGate;

    void Update()
    {
        // ON CHANGE
        if (state != lastState)
            StateChange();

        lastState = state;

        float alt = transform.position.y;

        // UPDATE
        if (state == State.Landing)
        {
            if (!runway)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 veloDir = runway.transform.forward;
            Vector3 runwayForward = runway.transform.forward;
            Vector3 runwayRight = runway.transform.right;

            float flareAltMult = 1.0f / flareBeginAlt;
            float veloFlareAltMult = 1.0f / veloFlareBeginAlt;

            float sidewaysNoiseAdd = 0;

            if (!onGround)
            {
                float sidewaysNoise = (-0.5f + Mathf.PerlinNoise(52.223f, alt * randomFreq));
                sidewaysNoiseAdd = Mathf.Lerp(0, sidewaysNoise, alt * 0.05f);

                speed = landingSpeed;

                float angle = Mathf.Lerp(touchdownAoA, landingAngle, alt * veloFlareAltMult);

                // direction dependent - solved
                veloDir = Quaternion.AngleAxis(angle, runwayRight) * veloDir;
                float flareT = Mathf.Clamp01(alt * flareAltMult);
                //Debug.Log(flareT);
                flare = Mathf.Lerp(flareMaximumAngle, landingAoA, flareT);

                throttle = THROTTLE_LANDING;
            }
            else
            {
                if (speed > runwayTaxiSpeed)
                {
                    speed -= Time.deltaTime * landingBrakeMult;
                }

                float flareSign = Mathf.Sign(flareMaximumAngle);

                if (flare * flareSign > 0)
                {
                    flare -= flareSign * Time.deltaTime * 2;
                }

                throttle = THROTTLE_REVERSETHRUST;
            }

            if (alt < 0 && !onGround)
            {
                trackedPosition.y = 0;
                veloDir = -Vector3.right;
                onGround = true;
                //Debug.Log("They touch Martin!");
            }
            Vector3 frw = Quaternion.Euler(0, 0, flare) * runway.transform.forward;

            Vector3 up = new Vector3(0, 1, sidewaysNoiseAdd * randomBankMult).normalized;
            transform.rotation = Quaternion.LookRotation(frw, up);
            trackedPosition += veloDir * speed * Time.deltaTime;

            transform.position = trackedPosition + runwayRight * sidewaysNoiseAdd * randomXMult;

            if (onGround && speed <= runwayTaxiSpeed)
            {
                // END
                speed = runwayTaxiSpeed;

                // Find a closest taxiway
                taxiwayPoints = ATC.e.GetTaxiwayToGate(transform.position, frw, gate);
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                transform.rotation = runway.transform.rotation;

                state = State.TaxiingToGate;

                throttle = THROTTLE_TAXI;
            }
        }
        else if (state == State.TakingOff)
        {
            // --------
            // TAKE OFF
            // --------

            if (!runway)
            {
                Debug.LogError("Attempting to depart but no runway!");
                Destroy(gameObject);
                return;
            }

            Vector3 runwayForward = runway.transform.forward;
            Vector3 runwayRight = runway.transform.right;

            Vector3 veloDir = Quaternion.AngleAxis(curAngle, runwayRight) * runwayForward;

            float sidewaysNoise = (-0.5f + Mathf.PerlinNoise(52.223f, alt * randomFreq));
            float sidewaysNoiseAdd = Mathf.Lerp(0, sidewaysNoise * takeOffNoiseMult, alt * 0.2f);

            if (onGround)
            {
                speed += Time.deltaTime * speedUp;
            }

            float absAngle = Mathf.Abs(curAngle);

            if (speed > takeoffSpeed && absAngle < Mathf.Abs(takeoffMaxAngle))
            {
                //if (curAngle < takeoffMaxAngle)
                curAngle -= Time.deltaTime * takeOffAngleRate;

                // repeated!
                veloDir = Quaternion.AngleAxis(curAngle, runwayRight) * runwayForward;
            }

            if (speed > startRotateAtSpeed && Mathf.Abs(flare) < Mathf.Abs(flareMaximumAngle))
            {
                flare -= Time.deltaTime * rotateAngleRate;
            }

            if (onGround && speed > takeoffSpeed && !alreadyUnusedRunway)
            {
                onGround = false;
                runway.inUse = false;
                alreadyUnusedRunway = true;
            }

            //Vector3 frw = Quaternion.Euler(0, 0, flare) * veloDir;
            Vector3 frw = Quaternion.AngleAxis(flare, runwayRight) * veloDir;

            Vector3 up = new Vector3(0, 1, sidewaysNoiseAdd * randomBankMult).normalized;
            transform.rotation = Quaternion.LookRotation(frw, up);
            trackedPosition += veloDir * speed * Time.deltaTime;

            //Vector3 p = transform.position;
            //p.z = runway.transform.position.z + sidewaysNoiseAdd * randomXMult;
            //transform.position = p;
            trackedPosition += runwayRight * sidewaysNoiseAdd * randomXMult;

            transform.position = trackedPosition;

            if (alt > 500)
            {
                //Destroy(gameObject);
                return;
            }

            throttle = THROTTLE_TAKEOFF;
        }
        else if (state == State.TaxiingToGate)
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            if (taxiwayPoints == null)
            {
                speed -= Time.deltaTime * 0.75f;
                speed = Mathf.Clamp(speed, 0, runwayTaxiSpeed);

                if (speed == 0)
                {
                    state = State.AtGate;
                    throttle = 0;
                }
            }
            else
            {
                // follow path
                Vector3 tgt = taxiwayPoints[currentTarget];

                Vector3 dir = (taxiwayPoints[currentTarget] - transform.position).normalized;

                if (currentTarget < taxiwayPoints.Length - 1)
                    SteerTowards(dir, 30);
                else
                    SteerTowards(gate.transform.forward, 10);

                float distToGate = Vector3.Distance(taxiwayPoints[2], transform.position);

                if (distToGate < gateSlowdownRange && !slowingDownForGate)
                    slowingDownForGate = true;

                if (slowingDownForGate)
                {
                    speed -= Time.deltaTime * 2;
                    speed = Mathf.Clamp(speed, gateTaxiSpeed, runwayTaxiSpeed);
                    throttle = THROTTLE_SLOW_TAXI;
                }
                else
                {
                    speed += Time.deltaTime * 2;
                    speed = Mathf.Clamp(speed, 0, runwayTaxiSpeed);
                }

                if (Obstructed())
                {
                    speed -= Time.deltaTime * 10;
                    speed = Mathf.Clamp(speed, 0, runwayTaxiSpeed);
                }

                // validate node
                if (Vector3.Distance(transform.position, tgt) < validationDistance)
                {
                    currentTarget++;
                    if (currentTarget >= taxiwayPoints.Length)
                    {
                        taxiwayPoints = null;
                    }
                }

                // free runway
                if (runway && runway.inUse && !alreadyUnusedRunway)
                {
                    if (transform.position.z < runway.transform.position.z + runway.taxiwayThreshold)
                    {
                        //Debug.Log("Freeing runway", gameObject);
                        runway.inUse = false;
                        alreadyUnusedRunway = true;
                        //runway = null;
                    }
                }
            }


        }
        else if (state == State.TaxiingToRunway)
        {
            // pushback
            if (isPushback)
            {
                bool beforeTurn = !ATC.e.apron.IsPastPushbackTurnPoint(transform.position);
                //transform.position.z < ATC.e.gateTaxiwayZ + pushbackTurnOffset;

                if (beforeTurn)
                {
                    speed += -Time.deltaTime;
                    speed = Mathf.Clamp(speed, -pushBackSpeed, 0);
                }
                else // while turning
                {
                    throttle = THROTTLE_SLOW_TAXI;

                    float currentAngle = Vector3.SignedAngle(transform.forward, -ATC.e.apron.transform.forward, Vector3.up);
                    //Debug.Log(currentAngle);

                    // pushback turn
                    SteerTowards(-ATC.e.apron.transform.forward, pushbackTurnRate);

                    if (currentAngle > -2 && currentAngle < 2)
                    {
                        isPushback = false;
                        pushbackEndedTime = Time.time;
                    }
                    else
                    {
                        speed += -Time.deltaTime * 2;
                        speed = Mathf.Clamp(speed, -pushBackSpeed, 0);
                    }
                }
            }
            else
            {
                // Wait for runway to be free
                if (runway.IsPointPastThreshold(transform.position, -20) && !hasRunwayClearance)
                {
                    if (!requestSent)
                        SendTakeoffRequest();

                    speed -= Time.deltaTime * 4;
                    speed = Mathf.Clamp(speed, 0, runwayTaxiSpeed);
                }
                else
                {
                    speed += Time.deltaTime;

                    if (Obstructed())
                    {
                        speed -= Time.deltaTime * 10;
                        speed = Mathf.Clamp(speed, 0, runwayTaxiSpeed);
                    }

                    if (Time.time - pushbackEndedTime < afterPushbackWait)
                        // wait after pushback
                        speed = Mathf.Clamp(speed, -runwayTaxiSpeed, 0);
                    else // proceed to follow path
                    {
                        speed = Mathf.Clamp(speed, -runwayTaxiSpeed, runwayTaxiSpeed);
                        throttle = THROTTLE_TAXI;
                    }
                }

                // follow path
                Vector3 tgt = taxiwayPoints[currentTarget];
                //GetSteer(tgt);

                Vector3 dir = (taxiwayPoints[currentTarget] - transform.position).normalized;
                SteerTowards(dir, 30);

                // validate node
                if (Vector3.Distance(transform.position, tgt) < validationDistance)
                {
                    currentTarget++;
                    if (currentTarget >= taxiwayPoints.Length)
                    {
                        // Taxi to runway end
                        taxiwayPoints = null;
                        state = State.TakingOff;
                        onGround = true;
                        alreadyUnusedRunway = false;
                    }
                }
            }

            transform.position += transform.forward * speed * Time.deltaTime;

            /*
            float steerSign = Mathf.Sign(steerAngle - steerTarget);
            steerRate += steerSign * Time.deltaTime;
            steerRate = Mathf.Clamp(steerRate, -5, 5);
            steerAngle = steerRate;
            transform.forward = Quaternion.Euler(0, steerAngle, 0) * transform.forward;
            */

            //steerTarget = steerRate;

            // occupy runway
            /*
            if (runway && runway.inUse)
            {
                if (transform.position.z < runway.transform.position.z + runway.taxiwayThreshold)
                {
                    Debug.Log("Freeing runway", gameObject);
                    runway.inUse = false;
                    runway = null;
                }
            }*/
        }
        else if (state == State.AtGate)
        {

        }


        //progress += Time.deltaTime * progressSpeed * speedCurve.Evaluate(progress);

    }

    public float turnSpeed = 1;

    void GetSteer(Vector3 target)
    {
        Vector3 point = target;
        Vector3 steerVector = transform.InverseTransformPoint(point);

        float turn = steerVector.x / steerVector.magnitude * Time.deltaTime;

        transform.rotation *= Quaternion.AngleAxis(turn * turnSpeed, Vector3.up);
    }

    const float zMin = 5;
    const float zMax = 70;
    const float xMin = 30;

    public bool Obstructed()
    {
        bool obstructed = false;

        for (int i = 0; i < ATC.e.planes.Count; i++)
        {
            Plane p = ATC.e.planes[i];

            if (!p) continue;
            if (p == this) continue;
            if (!p.gameObject.activeSelf) continue;
            if (!(p.state == State.TaxiingToGate || p.state == State.TaxiingToRunway)) continue;

            Vector3 pos = p.transform.position;
            if (p.isPushback) pos = p.pushbackPoint;

            Vector3 lp = transform.InverseTransformPoint(pos);

            if (lp.z > zMin && lp.z < zMax &&
                lp.x > -xMin && lp.x < xMin)
            {
                obstructed = true;
                break;
            }
        }

        Color c = obstructed ? Color.red : Color.green;

        Debug.DrawLine(
            transform.position + transform.forward * zMax - transform.right * xMin,
            transform.position + transform.forward * zMax + transform.right * xMin, c);

        Debug.DrawLine(
            transform.position + transform.forward * zMin - transform.right * xMin,
            transform.position + transform.forward * zMin + transform.right * xMin, c);

        Debug.DrawLine(
            transform.position + transform.forward * zMin - transform.right * xMin,
            transform.position + transform.forward * zMax - transform.right * xMin, c);

        Debug.DrawLine(
            transform.position + transform.forward * zMin + transform.right * xMin,
            transform.position + transform.forward * zMax + transform.right * xMin, c);

        return obstructed;
    }

    public PID steerPID;
    float turnVelo;
    public float maxAngVelo = 200;

    void SteerTowards(Vector3 dir, float maxSteer)
    {
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        angle = Mathf.Clamp(angle, -maxSteer, maxSteer);
        //Debug.Log(angle);

        steerAngle = steerPID.Update(angle, turnVelo, Time.deltaTime);

        turnVelo += steerAngle * Time.deltaTime;
        turnVelo = Mathf.Clamp(turnVelo, -maxAngVelo, maxAngVelo);

        transform.Rotate(new Vector3(0, turnVelo * Time.deltaTime, 0));
    }

    void SendTakeoffRequest()
    {
        ATC.RunwayRequest request = new ATC.RunwayRequest();
        request.plane = this;
        request.type = ATC.RunwayRequest.Type.TakeOff;
        ATC.e.requestQueue.Enqueue(request);
        requestSent = true;
        Debug.Log("Sent takeoff request");
    }

    Color gizmoColor;
    float pointHeight;

    Vector3 pushbackPoint;

    private void OnDrawGizmos()
    {
        if (isPushback)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawRay(pushbackPoint, Vector3.up * 10);
        }

        if (taxiwayPoints != null && taxiwayPoints.Length != 0)
        {
            Gizmos.color = gizmoColor;

            Vector3 pt1 = transform.position;
            Vector3 pt2 = taxiwayPoints[currentTarget];
            pt2.y = pointHeight;
            Gizmos.DrawLine(pt1, pt2);

            for (int i = currentTarget; i < taxiwayPoints.Length - 1; i++)
            {
                Vector3 p1 = taxiwayPoints[i];
                p1.y = pointHeight;
                Vector3 p2 = taxiwayPoints[i + 1];
                p2.y = pointHeight;

                //Gizmos.DrawWireSphere(taxiwayPoints[i], validationDistance);
                Gizmos.DrawLine(p1, p2);
            }
        }

        /*
        if (gate && state == State.TaxiingToGate)
        {
            Gizmos.DrawWireSphere(gate.transform.position, gateSlowdownRange);
        }*/
    }
}
