﻿using System.Collections;
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
                alreadyUnusedRunway = false;
                break;
            case State.TaxiingToGate:
                currentTarget = 0;
                Debug.Log("WTF!");
                transform.rotation = Random.rotation;// runway.transform.rotation;
                break;
            case State.AtGate:

                break;
            case State.TaxiingToRunway:
                currentTarget = 0;
                Debug.Log("Got points to runway");
                isPushback = true;
                gate.plane = null;
                taxiwayPoints = ATC.e.GetTaxiwayToRunway(gate);

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

    bool alreadyUnusedRunway;

    Vector3 LandingFrw()
    {
        return Quaternion.Euler(0, 0, landingAngle) * -Vector3.right;
    }

    bool isPushback;

    float steerRate;
    float steerTarget;
    float steerAngle;

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
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                transform.rotation = runway.transform.rotation;

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
                Debug.LogError("Attempting to depart but no runway!");
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
                //Debug.Log("Speedingup");
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

            if (onGround && speed > takeoffSpeed && !alreadyUnusedRunway)
            {
                onGround = false;
                runway.inUse = false;
                alreadyUnusedRunway = true;
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
        else if (state == State.TaxiingToGate)
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            if (taxiwayPoints == null)
            {
                speed -= Time.deltaTime * 1;
                speed = Mathf.Clamp(speed, 0, 5);

                if (speed == 0)
                    state = State.AtGate;
            }
            else
            {
                // follow path
                //transform.position += transform.forward * speed * Time.deltaTime;

                // follow path
                Vector3 tgt = taxiwayPoints[currentTarget];

                Vector3 dir = (taxiwayPoints[currentTarget] - transform.position).normalized;

                if (currentTarget < taxiwayPoints.Length - 1)
                    SteerTowards(dir, 30);
                else
                    SteerTowards(gate.transform.forward, 10);

                float distToGate = Vector3.Distance(gate.transform.position, transform.position);

                speed = Mathf.Lerp(1, 5, distToGate * 0.01f);

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
                        Debug.Log("Freeing runway", gameObject);
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
                bool beforeTurn = transform.position.z < ATC.e.gateTaxiwayZ - 10;

                if (beforeTurn)
                {
                    speed += -Time.deltaTime;
                    speed = Mathf.Clamp(speed, -5, 0);
                }
                else
                {

                    float currentAngle = Vector3.SignedAngle(transform.forward, -runway.transform.forward, Vector3.up);
                    //Debug.Log(currentAngle);

                    SteerTowards(Vector3.right, 30);

                    if (currentAngle < 0)
                    {
                        speed += -Time.deltaTime;
                        speed = Mathf.Clamp(speed, -5, 0);
                    }
                    else
                    {
                        isPushback = false;

                    }
                }
            }
            else
            {
                // Wait for runway to be free
                if (transform.position.z > runway.GetThreshold() - 20 && runway.inUse)
                {
                    speed -= Time.deltaTime;
                    speed = Mathf.Clamp(speed, 0, 5);
                }
                else
                {
                    speed += Time.deltaTime;
                    speed = Mathf.Clamp(speed, -5, 5);
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
                        taxiwayPoints = null;
                        state = State.TakingOff;
                        onGround = true;
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
