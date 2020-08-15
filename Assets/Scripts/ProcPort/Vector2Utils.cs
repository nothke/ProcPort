using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.ProcPort
{
    public static class Vector2Utils
    {

        public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        public static bool RayRayIntersection(Vector2 apos, Vector2 adir, Vector2 bpos, Vector2 bdir, ref Vector2 intersection)
        {
            var PQx = bpos.x - apos.x;
            var PQy = bpos.y - apos.y;
            var rx = adir.x;
            var ry = adir.y;
            var rxt = -ry;
            var ryt = rx;
            var qx = PQx * rx + PQy * ry;
            var qy = PQx * rxt + PQy * ryt;
            var sx = bdir.x * rx + bdir.y * ry;
            var sy = bdir.x * rxt + bdir.y * ryt;

            // if lines are identical or do not cross...
            if (sy == 0) return false;

            var a = qx - qy * sx / sy;
            intersection = new Vector2(apos.x + a * rx, apos.y + a * ry);

            return true;
        }
    }
}