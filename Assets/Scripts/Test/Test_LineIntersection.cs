using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_LineIntersection : MonoBehaviour
{
    public Transform other;

    private void OnDrawGizmos()
    {
        Vector2 intersection = new Vector2();
        //Apron.LineIntersection(
        //To2D(transform.position), To2D(transform.forward),
        //To2D(other.position), To2D(other.forward), ref intersection);

        Apron.RayRayIntersection(To2D(transform.position), To2D(transform.forward),
            To2D(other.position), To2D(other.forward), ref intersection);

        Gizmos.DrawLine(transform.position - transform.forward * 1000,
            transform.position + transform.forward * 1000);
        Gizmos.DrawLine(other.position - other.forward * 1000,
            other.position + other.forward * 1000);

        Gizmos.DrawWireSphere(new Vector3(intersection.x, 0, intersection.y), 1 * 100);
    }

    Vector2 To2D(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
}
