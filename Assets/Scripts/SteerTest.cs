using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerTest : MonoBehaviour
{
    public PID steerPid;

    [Range(-1, 1)]
    public float steerTarget = 0;
    float steerAngle = 0;
    public float maxSteer = 30;

    float turnVelo = 0;
    public float maxAngVelo = 200;

    public Transform targetDir;

    void Update()
    {
        SteerTowards(targetDir.forward);
    }

    void SteerTowards(Vector3 dir)
    {
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        angle = Mathf.Clamp(angle, -maxSteer, maxSteer);
        //Debug.Log(angle);

        steerAngle = steerPid.Update(angle, turnVelo, Time.deltaTime);

        turnVelo += steerAngle * Time.deltaTime;
        turnVelo = Mathf.Clamp(turnVelo, -maxAngVelo, maxAngVelo);

        transform.Rotate(new Vector3(0, turnVelo * Time.deltaTime, 0));
    }
}
