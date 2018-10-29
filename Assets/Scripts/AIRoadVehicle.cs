using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(ShuttleController))]
public class AIRoadVehicle : MonoBehaviour
{
    public Path path;
    public List<Path> nextPaths = new List<Path>();

    [HideInInspector]
    public int currentNode;

    public Transform target;

    public float maxSpeed = 10;
    public float acceleration = 1;
    public float turnSpeed = 2;

    public bool followGroundElevation;
    //public bool alignWithLastNode;

    void Start()
    {
        //sc = GetComponent<ShuttleController>();
    }

    void Update()
    {
        if (path)
        {
            accel = acceleration;

            if (!target)
                target = path.nodes[currentNode];

            if (Vector3.SqrMagnitude(path.nodes[currentNode].position - transform.position)
                < path.validationRange * path.validationRange)
            {
                currentNode++;

                // if got past last node
                if (currentNode >= path.nodes.Count)
                {
                    // if there is a next designated path
                    if (nextPaths.Count != 0)
                    {
                        SetPath(nextPaths[0]);
                        nextPaths.RemoveAt(0);

                        return;
                    }

                    // if there is an end path continue to it
                    if (path.endNodePaths.Count > 0 && path.endNodes.Count > 0)
                    {
                        SetPath(path.endNodePaths[0]);

                        // target = path.nodes[currentNode]; // unnecessary

                        return;
                    }

                    path = null;
                    target = null;

                    // STOP

                    accel = -acceleration;

                    return;
                }

                target = path.nodes[currentNode];
            }
        }

        GetSpeed();

        if (followGroundElevation)
            StickToSurface();

        if (target)
            GetSteer();
    }

    float speed;
    float accel;



    public void SetPath(Path p) { SetPath(p, false); }

    public void SetPath(Path p, bool clearOldPaths)
    {
        path = p;
        currentNode = 0;

        target = path.nodes[0];

        if (clearOldPaths)
            nextPaths.Clear();
    }

    public void SetPaths(Path firstP, Path[] nextP)
    {
        path = firstP;
        currentNode = 0;

        target = path.nodes[0];

        nextPaths.Clear();
        nextPaths.AddRange(nextP);
    }

    void GetSpeed()
    {
        speed += accel * Time.deltaTime;

        if (path)
            speed = Mathf.Clamp(speed, 0, Mathf.Min(maxSpeed, path.speedLimit));
        else
            speed = Mathf.Clamp(speed, 0, maxSpeed);

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void GetSteer()
    {
        Vector3 point = target.position; //new Vector3(target.position.x, 0, target.position.z);
        Vector3 steerVector = transform.InverseTransformPoint(point);

        float turn = steerVector.x / steerVector.magnitude;

        //float steer = steerVector.x / steerVector.magnitude;



        #region obstacle
        /*

        RaycastHit hit;

        if (!followPlayer)
        {
            if (Physics.Raycast(transform.position + Vector3.up * frontRayHeight, transform.forward, out hit, frontRayLength))
            {
                if (hit.collider.tag != "Debris" && hit.collider.tag != "Pickup" && !hit.collider.isTrigger)
                {
                    accel = -1;

                    if (vehicle.sqrSpeed < 0.1f)
                        accel = -0.05f;
                }
            }
            else
            {
                accel = speedMult;
                if (vehicle.sqrSpeed > maxSpeed * maxSpeed)
                    accel = 0;
            }
        }
        else
        {
            // USE OBSTACLE AVOIDANCE
            accel = 1;
        }*/
        #endregion


        transform.rotation *= Quaternion.AngleAxis(turn * turnSpeed, Vector3.up);
    }

    void StickToSurface()
    {
        RaycastHit hit;

        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);

        if (Physics.Raycast(ray, out hit))
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
    }
}
