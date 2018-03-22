// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Exploder2D.Core
{
    public static class MeshUtils
	{
        /// <summary>
        /// Compute barycentric coordinates (u, v, w) for point p with respect to triangle (a, b, c)
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector3 ComputeBarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            var v0x = b.x - a.x;
            var v0y = b.y - a.y;
            var v0z = b.z - a.z;

            var v1x = c.x - a.x;
            var v1y = c.y - a.y;
            var v1z = c.z - a.z;

            var v2x = p.x - a.x;
            var v2y = p.y - a.y;
            var v2z = p.z - a.z;

            var d00 = v0x * v0x + v0y * v0y + v0z * v0z;
            var d01 = v0x * v1x + v0y * v1y + v0z * v1z;
            var d11 = v1x * v1x + v1y * v1y + v1z * v1z;
            var d20 = v2x * v0x + v2y * v0y + v2z * v0z;
            var d21 = v2x * v1x + v2y * v1y + v2z * v1z;
            var denom = d00 * d11 - d01 * d01;

            var v = (d11 * d20 - d01 * d21) / denom;
            var w = (d00 * d21 - d01 * d20) / denom;
            var u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }

        /// <summary>
        /// swap
        /// </summary>
        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        /// <summary>
        /// adjust vertices around the centroid
        /// </summary>
        /// <param name="vertices">list of vertices</param>
        /// <param name="centroid">centroid position</param>
        public static void CenterPivot(Vector3[] vertices, Vector3 centroid)
        {
            int count = vertices.Length;
            for (int i = 0; i < count; i++)
            {
                var val = vertices[i];

                val.x -= centroid.x;
                val.y -= centroid.y;
                val.z -= centroid.z;

                vertices[i] = val;
            }
        }

        /// <summary>
        /// find and isolate independent (not connecting) parts in a mesh
        /// </summary>
        public static List<CutterMesh> IsolateMeshIslands(SpriteMesh mesh)
        {
            var triangles = mesh.triangles;
            var vertexCount = mesh.vertices.Length;

            // cache mesh data
            var trianglesNum = mesh.triangles.Length;
            var vertices = mesh.vertices;

            if (trianglesNum <= 3)
            {
                return null;
            }

            Exploder2DUtils.Assert(trianglesNum > 3, "IsolateMeshIslands error: " + trianglesNum);

            var lsHash = new LSHash(0.1f, vertexCount);
            var vertHash = new int[trianglesNum];

            for (int i = 0; i < trianglesNum; i++)
            {
                vertHash[i] = lsHash.Hash(vertices[triangles[i]]);
            }

            var islands = new List<HashSet<int>> { new HashSet<int> { vertHash[0], vertHash[1], vertHash[2] } };
            var islandsIdx = new List<List<int>> { new List<int>(trianglesNum) { 0, 1, 2 } };
            var triVisited = new bool[trianglesNum];

            triVisited[0] = true;
            triVisited[1] = true;
            triVisited[2] = true;

            var currIsland = islands[0];
            var currIslandIdx = islandsIdx[0];

            var counter = 3;
            var lastInvalidIdx = -1;
            var loopCounter = 0;

            while (true)
            {
                var foundIsland = false;

                for (int j = 3; j < trianglesNum; j += 3)
                {
                    if (triVisited[j])
                    {
                        continue;
                    }

                    if (currIsland.Contains(vertHash[j]) ||
                        currIsland.Contains(vertHash[j + 1]) ||
                        currIsland.Contains(vertHash[j + 2]))
                    {
                        currIsland.Add(vertHash[j]);
                        currIsland.Add(vertHash[j + 1]);
                        currIsland.Add(vertHash[j + 2]);

                        currIslandIdx.Add(j);
                        currIslandIdx.Add(j + 1);
                        currIslandIdx.Add(j + 2);

                        triVisited[j] = true;
                        triVisited[j + 1] = true;
                        triVisited[j + 2] = true;

                        counter += 3;
                        foundIsland = true;
                    }
                    else
                    {
                        lastInvalidIdx = j;
                    }
                }

                if (counter == trianglesNum)
                {
                    break;
                }

                if (!foundIsland)
                {
                    // create new island
                    currIsland = new HashSet<int>{vertHash[lastInvalidIdx], vertHash[lastInvalidIdx+1], vertHash[lastInvalidIdx+2]};
                    currIslandIdx = new List<int>(trianglesNum/2) {lastInvalidIdx, lastInvalidIdx + 1, lastInvalidIdx + 2};

                    islands.Add(currIsland);
                    islandsIdx.Add(currIslandIdx);
                }

                loopCounter++;
                if (loopCounter > 100)
                {
                    Exploder2DUtils.Log("10000 loop exceeded, islands: " + islands.Count);
                    break;
                }
            }

            var islandNum = islands.Count;

            Exploder2DUtils.Assert(islandNum >= 1, "No island found!");

            // no more than one islands
            if (islandNum == 1)
            {
                return null;
            }

            var result = new List<CutterMesh>(islands.Count);

            foreach (var island in islandsIdx)
            {
                var cutterMesh = new CutterMesh {mesh = new SpriteMesh()};
                var triCount = island.Count;

                var m = cutterMesh.mesh;

                var tt = new List<int>(triCount);
                var vs = new List<Vector2>(triCount);

                var triCache = new Dictionary<int, int>(trianglesNum);
                var centroid = Vector2.zero;
                var centroidCounter = 0;
                var triCounter = 0;

                foreach (var i in island)
                {
                    var tri = triangles[i];
                    var id = 0;

                    if (triCache.TryGetValue(tri, out id))
                    {
                        tt.Add(id);
                        continue;
                    }

                    tt.Add(triCounter);
                    triCache.Add(tri, triCounter);
                    triCounter++;

                    centroid += vertices[tri];
                    centroidCounter++;

                    vs.Add(vertices[tri]);
                }

                m.vertices = vs.ToArray();

                m.triangles = tt.ToArray();

                cutterMesh.centroidLocal = centroid/centroidCounter;

                result.Add(cutterMesh);
            }

            return result;
        }

        /// <summary>
        /// generate collider path based on mesh vertices
        /// </summary>
        public static void GeneratePolygonCollider(PolygonCollider2D collider, SpriteMesh mesh)
        {
            if (mesh != null && collider)
            {
                var vertices = mesh.vertices;

                var path = new Vector2[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    path[i] = vertices[i];
                }

                var hull = Utils.Hull2D.ChainHull2D(path);

                collider.SetPath(0, hull);
            }
        }

        /// <summary>
        /// Returns 2 times the signed triangle area. The result is positive if
        /// abc is ccw, negative if abc is cw, zero if abc is degenerate.
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        public static float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);
        }

        /// <summary>
        /// Test if segments ab and cd overlap. If they do, compute and return intersection t value along ab and intersection position p
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        public static bool Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 p)
        {
            // Sign of areas correspond to which side of ab points c and d are
            float a1 = Signed2DTriArea(a, b, d); // Compute winding of abd (+ or -)
            float a2 = Signed2DTriArea(a, b, c); // To intersect, must have sign opposite of a1

            // If c and d are on different sides of ab, areas have different signs
            if (a1 * a2 < 0.0f)
            {
                // Compute signs for a and b with respect to segment cd
                float a3 = Signed2DTriArea(c, d, a); // Compute winding of cda (+ or -)
                // Since area is constant a1 - a2 = a3 - a4, or a4 = a3 + a2 - a1
                // float a4 = Signed2DTriArea(c, d, b); // Must have opposite sign of a3
                float a4 = a3 + a2 - a1;
                // Points a and b on different sides of cd if areas have different signs
                if (a3 * a4 < 0.0f)
                {
                    // Segments intersect. Find intersection point along L(t) = a + t * (b - a).
                    // Given height h1 of an over cd and height h2 of b over cd,
                    // t = h1 / (h1 - h2) = (b*h1/2) / (b*h1/2 - b*h2/2) = a3 / (a3 - a4),
                    // where b (the base of the triangles cda and cdb, i.e., the length
                    // of cd) cancels out.
                    var t = a3 / (a3 - a4);
                    p = a + t * (b - a);
                    return true;
                }
            }

            // Segments not intersecting (or collinear)
            p = Vector2.zero;
            return false;
        }
	}
}
