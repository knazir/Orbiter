// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using UnityEngine;

namespace Exploder2D.Core.Math
{
    public class Line2D
    {
        /// <summary>
        /// tolerance distance epsylon for points on plane
        /// meaning points with distance less then epsylon from plane are "on the plane"
        /// </summary>
        private const float epsylon = 0.0001f;

        /// <summary>
        /// normal of the plane
        /// Points x on the plane satisfy Dot(n,x) = d
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// one of the creation point on plane (this is just for debugging)
        /// </summary>
        public Vector2 Pnt { get; private set; }

        /// <summary>
        /// distance of the plane
        /// d = dot(n,p) for a given point p on the plane
        /// </summary>
        public float Distance;

        /// <summary>
        /// 3 points constructor
        /// </summary>
	    public static Line2D CratePointPoint(Vector2 a, Vector2 b)
        {
            var line2D = new Line2D {Normal = (b - a).normalized};
            line2D.Distance = Vector2.Dot(line2D.Normal, a);
            line2D.Pnt = a;

            return line2D;
        }

        /// <summary>
        /// normal, point constructor
        /// </summary>
        public static Line2D CreateNormalPoint(Vector2 normal, Vector2 p)
        {
            var line2d = new Line2D {Normal = normal.normalized};
            line2d.Distance = Vector2.Dot(line2d.Normal, p);
            line2d.Pnt = p;

            return line2d;
        }

        public Line2D()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public Line2D(Line2D instance)
        {
            Normal = instance.Normal;
            Distance = instance.Distance;
            Pnt = instance.Pnt;
        }

        /// <summary>
        /// classification of the point with this plane
        /// </summary>
        [Flags]
        public enum PointClass
        {
            Coplanar = 0,
            Front = 1,
            Back = 2,
            Intersection = 3,
        }

        /// <summary>
        /// classify point
        /// </summary>
        public PointClass ClassifyPoint(Vector2 p)
        {
            var dot = Vector2.Dot(p, Normal) - Distance;
            return (dot < -epsylon) ? PointClass.Back : (dot > epsylon) ? PointClass.Front : PointClass.Coplanar;
        }

        /// <summary>
        /// test positive or negative side of the point n
        /// </summary>
        public bool GetSide(Vector2 n)
        {
            return Vector2.Dot(n, Normal) - Distance > epsylon;
        }

        /// <summary>
        /// flip normal
        /// </summary>
        public void Flip()
        {
            Normal = -Normal;
            Distance = -Distance;
        }

        /// <summary>
        /// hack for collinear points
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool GetSideFix(ref Vector2 n)
        {
            var dot = n.x*Normal.x + n.y*Normal.y - Distance;

//            var dot = Vector3.Dot(n, Normal) - Distance;

            var sign = 1.0f;
            var abs = dot;
            if (dot < 0)
            {
                sign = -1.0f;
                abs = -dot;
            }

            if (abs < epsylon + 0.001f)
            {
//                Utils.Log("Coplanar point!");

                n.x += Normal.x*0.001f*sign;
                n.y += Normal.y*0.001f*sign;

                dot = n.x*Normal.x + n.y*Normal.y - Distance;
//                n += Normal*0.001f*Mathf.Sign(dot);
            }

//            return Vector3.Dot(n, Normal) - Distance > epsylon;
            return dot > epsylon;
        }

        /// <summary>
        /// returns true if two points are on the same side of the plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool SameSide(Vector2 a, Vector2 b)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compute intersection between a segment line (a, b) and a plane (p)
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        /// <param name="a">first point of a segment</param>
        /// <param name="b">second point of a segment</param>
        /// <param name="t">normalized distance of intersection point on vector (ab)</param>
        /// <param name="q">point in intersection</param>
        /// <returns>true if there is an intersection</returns>
        public bool IntersectSegment(Vector2 a, Vector2 b, out float t, ref Vector2 q)
        {
            var abx = b.x - a.x;
            var aby = b.y - a.y;

            var dot0 = Normal.x*a.x + Normal.y*a.y;
            var dot1 = Normal.x*abx + Normal.y*aby;

            t = (Distance - dot0) / dot1;

            if (t >= 0.0f - epsylon && t <= 1.0f + epsylon)
            {
                q.x = a.x + t*abx;
                q.y = a.y + t*aby;

                return true;
            }

            Exploder2DUtils.Log("IntersectSegment failed: " + t);
            q = Vector2.zero;
            return false;
        }

        /// <summary>
        /// make inverse transformation of this plane to target space
        /// </summary>
        /// <param name="transform">target transformation space</param>
        public void InverseTransform(Transform transform)
        {
            // inverse transform normal
            var inverseNormal = transform.InverseTransformDirection(Normal);

            // inverse transform point
            var inversePoint = transform.InverseTransformPoint(Pnt);

            // update plane
            Normal = inverseNormal;
            Distance = Vector2.Dot(inverseNormal, inversePoint);
        }
	}
}
