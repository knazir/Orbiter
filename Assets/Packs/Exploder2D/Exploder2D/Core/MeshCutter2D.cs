// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//#define PROFILING

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Exploder2D.Core
{
    public struct CutterMesh
    {
        public SpriteMesh mesh;
        public Vector2 centroidLocal;
    }

    public class MeshCutter2D
    {
        private List<int>[] triangles;
        private List<Vector2>[] vertices;

        private List<int> cutTris;
        private int[] triCache;

        private Vector2[] centroid;
        private int[] triCounter;

        private Dictionary<long, int>[] cutVertCache;
        private Dictionary<int, int>[] cornerVertCache;
        private int contourBufferSize;

        /// <summary>
        /// initialize cutter for faster start
        /// </summary>
        /// <param name="trianglesNum">potencial number of triangles</param>
        /// <param name="verticesNum">potencial number of vertices</param>
        public void Init(int trianglesNum, int verticesNum)
        {
            AllocateBuffers(trianglesNum, verticesNum);
        }

        void AllocateBuffers(int trianglesNum, int verticesNum)
        {
            // pre-allocate mesh data for both sides
            if (triangles == null || triangles[0].Capacity < trianglesNum)
            {
                triangles = new[] { new List<int>(trianglesNum), new List<int>(trianglesNum) };
            }
            else
            {
                triangles[0].Clear();
                triangles[1].Clear();
            }

            if (vertices == null || vertices[0].Capacity < verticesNum || triCache.Length < verticesNum)
            {
                vertices = new[] { new List<Vector2>(verticesNum), new List<Vector2>(verticesNum) };

                centroid = new Vector2[2];

                triCache = new int[verticesNum + 1];
                triCounter = new int[2] { 0, 0 };
                cutTris = new List<int>(verticesNum / 3);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    vertices[i].Clear();
                    centroid[i] = Vector2.zero;
                    triCounter[i] = 0;
                }
                cutTris.Clear();
                for (int i = 0; i < triCache.Length; i++)
                {
                    triCache[i] = 0;
                }
            }
        }

        void AllocateContours(int cutTrianglesNum)
        {
            // pre-allocate contour data
            if (cutVertCache == null || contourBufferSize < cutTrianglesNum)
            {
                cutVertCache = new[] { new Dictionary<long, int>(cutTrianglesNum * 2), new Dictionary<long, int>(cutTrianglesNum * 2) };
                cornerVertCache = new[] { new Dictionary<int, int>(cutTrianglesNum), new Dictionary<int, int>(cutTrianglesNum) };
                contourBufferSize = cutTrianglesNum;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    cutVertCache[i].Clear();
                    cornerVertCache[i].Clear();
                }
            }
        }

        /// <summary>
        /// cut mesh by plane
        /// </summary>
        /// <param name="spriteMesh">mesh to cut</param>
        /// <param name="meshTransform">transformation of the mesh</param>
        /// <param name="line2D">cutting plane</param>
        /// <returns>processing time</returns>
        public float Cut(SpriteMesh spriteMesh, Transform meshTransform, Math.Line2D line2D, ref List<CutterMesh> meshes)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

#if PROFILING
            MeasureIt.Begin("CutAllocations");
#endif

            // cache mesh data
            var trianglesNum = spriteMesh.triangles.Length;
            var verticesNum = spriteMesh.vertices.Length;
            var meshTriangles = spriteMesh.triangles;
            var meshVertices = spriteMesh.vertices;

            // preallocate buffers
            AllocateBuffers(trianglesNum, verticesNum);

            CutterMesh mesh0, mesh1;

#if PROFILING
            MeasureIt.End("CutAllocations");
            MeasureIt.Begin("CutCycleFirstPass");
#endif

            // inverse transform cutting plane
           // line2D.InverseTransform(meshTransform);

            // first pass - find complete triangles on both sides of the plane
            for (int i = 0; i < trianglesNum; i += 3)
            {
                // get triangle points
                var v0 = meshVertices[meshTriangles[i]];
                var v1 = meshVertices[meshTriangles[i + 1]];
                var v2 = meshVertices[meshTriangles[i + 2]];

                var side0 = line2D.GetSideFix(ref v0);
                var side1 = line2D.GetSideFix(ref v1);
                var side2 = line2D.GetSideFix(ref v2);

                meshVertices[meshTriangles[i]] = v0;
                meshVertices[meshTriangles[i + 1]] = v1;
                meshVertices[meshTriangles[i + 2]] = v2;

//                Utils.Log(plane.Pnt + " " + v0 + " " + v1 + " " + " " + v2);

                // all points on one side
                if (side0 == side1 && side1 == side2)
                {
                    var idx = side0 ? 0 : 1;

                    if (meshTriangles[i] >= triCache.Length)
                    {
                        Exploder2DUtils.Log("TriCacheError " + meshTriangles[i] + " " + triCache.Length + " " + meshVertices.Length);
                    }

                    if (triCache[meshTriangles[i]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i]]);

                        centroid[idx] += meshVertices[meshTriangles[i]];

                        triCache[meshTriangles[i]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i]] - 1);
                    }

                    if (triCache[meshTriangles[i + 1]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 1]]);

                        centroid[idx] += meshVertices[meshTriangles[i + 1]];

                        triCache[meshTriangles[i + 1]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 1]] - 1);
                    }

                    if (triCache[meshTriangles[i + 2]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 2]]);

                        centroid[idx] += meshVertices[meshTriangles[i + 2]];

                        triCache[meshTriangles[i + 2]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 2]] - 1);
                    }
                }
                else
                {
                    // intersection triangles add to list and process it in second pass
                    cutTris.Add(i);
                }
            }

            if (vertices[0].Count == 0)
            {
                centroid[0] = meshVertices[0];
            }
            else
            {
                centroid[0] /= vertices[0].Count;
            }

            if (vertices[1].Count == 0)
            {
                centroid[1] = meshVertices[1];
            }
            else
            {
                centroid[1] /= vertices[1].Count;
            }

#if PROFILING
            MeasureIt.End("CutCycleFirstPass");
            MeasureIt.Begin("CutCycleSecondPass");
#endif

            mesh0.centroidLocal = centroid[0];
            mesh1.centroidLocal = centroid[1];

            mesh0.mesh = null;
            mesh1.mesh = null;

            if (cutTris.Count < 1)
            {
                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            AllocateContours(cutTris.Count);

            // second pass - cut intersecting triangles in half
            foreach (var cutTri in cutTris)
            {
                var triangle = new Triangle
                {
                    ids = new[] { meshTriangles[cutTri + 0], meshTriangles[cutTri + 1], meshTriangles[cutTri + 2] },
                    pos = new[] { meshVertices[meshTriangles[cutTri + 0]], meshVertices[meshTriangles[cutTri + 1]], meshVertices[meshTriangles[cutTri + 2]] },
                };

                // check points with a plane
                var side0 = line2D.GetSide(triangle.pos[0]);
                var side1 = line2D.GetSide(triangle.pos[1]);
                var side2 = line2D.GetSide(triangle.pos[2]);

                float t0, t1;
                Vector2 s0 = Vector2.zero, s1 = Vector2.zero;

                var idxLeft = side0 ? 0 : 1;
                var idxRight = 1 - idxLeft;

                if (side0 == side1)
                {
                    var a = line2D.IntersectSegment(triangle.pos[2], triangle.pos[0], out t0, ref s0);
                    var b = line2D.IntersectSegment(triangle.pos[2], triangle.pos[1], out t1, ref s1);

                    Exploder2DUtils.Assert(a && b, "!!!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle.ids[2], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle.ids[2], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft]);
                    var v1Left = AddTrianglePoint(triangle.pos[1], triangle.ids[1], triCache, cornerVertCache[idxLeft], vertices[idxLeft]);

                    // Triangle (s0, v0, s1)
                    triangles[idxLeft].Add(s0Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // Triangle (s1, v0, v1)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(v1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle.ids[2], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle.ids[2], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight]);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight]);

                    // Triangle (v2, s0, s1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);
                }
                else if (side0 == side2)
                {
                    var a = line2D.IntersectSegment(triangle.pos[1], triangle.pos[0], out t0, ref s1);
                    var b = line2D.IntersectSegment(triangle.pos[1], triangle.pos[2], out t1, ref s0);

                    Exploder2DUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle.ids[1], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle.ids[1], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft]);
                    var v2Left = AddTrianglePoint(triangle.pos[2], triangle.ids[2], triCache, cornerVertCache[idxLeft], vertices[idxLeft]);

                    // Triangle (v2, s1, s0)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(s0Left);

                    // Triangle (v2, v0, s1)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle.ids[1], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle.ids[1], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight]);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight]);

                    // Triangle (s0, s1, v1)
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);
                }
                else
                {
                    var a = line2D.IntersectSegment(triangle.pos[0], triangle.pos[1], out t0, ref s0);
                    var b = line2D.IntersectSegment(triangle.pos[0], triangle.pos[2], out t1, ref s1);

                    Exploder2DUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // right side ... 2 triangles
                    var s0Right = AddIntersectionPoint(s0, triangle.ids[0], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle.ids[0], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight]);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight]);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight]);

                    // Triangle (v2, s1, v1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);

                    // Triangle (s1, s0, v1)
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(v1Right);

                    // left side ... 1 triangle
                    var s0Left = AddIntersectionPoint(s0, triangle.ids[0], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle.ids[0], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft]);

                    // Triangle (s1, v0, s0)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s0Left);
                }
            }

#if PROFILING
            MeasureIt.End("CutCycleSecondPass");
#endif

            List<int>[] trianglesCut = null;

            if (vertices[0].Count > 3 && vertices[1].Count > 3)
            {
#if PROFILING
                MeasureIt.Begin("CutEndCopyBack");
#endif

                mesh0.mesh = new SpriteMesh();
                mesh1.mesh = new SpriteMesh();

                var verticesArray0 = vertices[0].ToArray();
                var verticesArray1 = vertices[1].ToArray();

                mesh0.mesh.vertices = verticesArray0;
                mesh1.mesh.vertices = verticesArray1;

                if (trianglesCut != null && trianglesCut[0].Count > 3)
                {
                    triangles[0].AddRange(trianglesCut[0]);
                    triangles[1].AddRange(trianglesCut[1]);
                }

                mesh0.mesh.triangles = triangles[0].ToArray();
                mesh1.mesh.triangles = triangles[1].ToArray();

                mesh0.centroidLocal = Vector2.zero;
                mesh1.centroidLocal = Vector2.zero;

                foreach (var p in vertices[0])
                {
                    mesh0.centroidLocal += p;
                }
                mesh0.centroidLocal /= vertices[0].Count;

                foreach (var p in vertices[1])
                {
                    mesh1.centroidLocal += p;
                }
                mesh1.centroidLocal /= vertices[1].Count;
#if PROFILING
                MeasureIt.End("CutEndCopyBack");
#endif

                meshes = new List<CutterMesh> { mesh0, mesh1 };

                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            stopWatch.Stop();

//            UnityEngine.Debug.Log("Empty cut! " + vertices[0].Count + " " + vertices[1].Count);

            return stopWatch.ElapsedMilliseconds;
        }

        struct Triangle
        {
            public int[] ids;
            public Vector2[] pos;
        }

        int AddIntersectionPoint(Vector2 pos, int edge0, int edge1, Dictionary<long, int> cache, List<Vector2> vertices)
        {
            //! TODO: figure out position hash for shared vertices
//            var key = pos.GetHashCode();
            var key = edge0 < edge1 ? (edge0 << 16) + edge1 : (edge1 << 16) + edge0;

            int result;
            if (cache.TryGetValue(key, out result))
            {
                // cache hit!
                return result;
            }

            vertices.Add(pos);

            var vertIndex = vertices.Count - 1;

            cache.Add(key, vertIndex);

            return vertIndex;
        }

        int AddTrianglePoint(Vector2 pos, int idx, int[] triCache, Dictionary<int, int> cache, List<Vector2> vertices)
        {
            // tricache
            if (triCache[idx] != 0)
            {
                // cache hit!
                return triCache[idx] - 1;
            }

            // second cache
            int result;
            if (cache.TryGetValue(idx, out result))
            {
                // cache hit!
                return result;
            }

            vertices.Add(pos);

            var vertIndex = vertices.Count - 1;

            cache.Add(idx, vertIndex);

            return vertIndex;
        }
    }
}
