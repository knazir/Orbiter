using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exploder2D.Core
{
    class Grid
    {
        readonly private Vector2 min/*, max*/;
        readonly private float resolution;
        private readonly short hCount;
        private readonly short vCount;
        private readonly Dictionary<long, Vector2> intersections;
        private readonly Cell[][] grid;

        enum CellType
        {
            Out =  0,
            Edge = 1,
            In =   2,
        }

        struct Cell
        {
            public Vector2 pos;
            public Vector2 center;
            public CellType type;
            public float size;
            public List<Vector2> pnts;

            public void AddPnt(Vector2 pnt)
            {
                if (pnts == null)
                {
                    pnts = new List<Vector2>(2);
                }

                pnts.Add(pnt);
            }

            public void LineIntersection(Vector2 p0, Vector2 p1, short v, short h, IDictionary<long, Vector2> intersections)
            {
                Vector2 res;

                var cell0 = (v << 16) | h;

                //
                // segment 0
                //
                var c0 = pos;
                var c1 = pos + Vector2.up*size;

                if (MeshUtils.Test2DSegmentSegment(c0, c1, p0, p1, out res))
                {
                    var cell1 = (v-1) << 16 | h;
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = res;
                }

                //
                // segment 1
                //
                c0 = c1;
                c1 = c1 + Vector2.right*size;

                if (MeshUtils.Test2DSegmentSegment(c0, c1, p0, p1, out res))
                {
                    var cell1 = v << 16 | (h + 1);
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = res;
                }

                //
                // segment 2
                //
                c0 = c1;
                c1 = pos + Vector2.right * size;

                if (MeshUtils.Test2DSegmentSegment(c0, c1, p0, p1, out res))
                {
                    var cell1 = (v+1) << 16 | h;
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = res;
                }

                //
                // segment 3
                //
                c0 = pos;

                if (MeshUtils.Test2DSegmentSegment(c0, c1, p0, p1, out res))
                {
                    var cell1 = v << 16 | (h-1);
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = res;
                }
            }

            public void PointIntersection(Vector2 p, short v, short h, IDictionary<long, Vector2> intersections)
            {
                const float epsilon = 0.01f;
                var cell0 = (v << 16) | h;

                //
                // segment 0
                //
                var c0 = pos;

                if (Mathf.Abs(p.x - c0.x) < epsilon)
                {
                    var cell1 = (v-1) << 16 | h;
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = p;
                }

                //
                // segment 1
                //
                c0 = pos + Vector2.up*size;

                if (Mathf.Abs(p.y - c0.y) < epsilon)
                {
                    var cell1 = v << 16 | (h + 1);
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = p;
                }

                //
                // segment 2
                //
                c0 = pos + Vector2.right * size;

                if (Mathf.Abs(p.x - c0.x) < epsilon)
                {
                    var cell1 = (v+1) << 16 | h;
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = p;
                }

                //
                // segment 3
                //
                c0 = pos;

                if (Mathf.Abs(p.y - c0.x) < epsilon)
                {
                    var cell1 = v << 16 | (h-1);
                    var key = (cell0 < cell1 ? ((long)cell0 << 32) | (uint)cell1 : ((long)cell1 << 32) | (uint)cell0);
                    intersections[key] = p;
                }
            }
        }

        public Grid(Vector2 min, Vector2 max, float resolution)
        {
            this.min = min;
//            this.max = max;
            this.resolution = resolution;

            var size = max - min;

            if (resolution > size.x)
            {
                resolution = size.x;
            }

            if (resolution > size.y)
            {
                resolution = size.y;
            }

            hCount = (short)Mathf.Clamp((size.x / resolution) + 4, 0, short.MaxValue);
            vCount = (short)Mathf.Clamp((size.y / resolution) + 4, 0, short.MaxValue);

            intersections = new Dictionary<long, Vector2>(vCount * hCount);
            grid = new Cell[vCount][];

            for (int i = 0; i < vCount; i++)
            {
                grid[i] = new Cell[hCount];

                for (int j = 0; j < hCount; j++)
                {
                    grid[i][j] = new Cell
                    {
                        pos = min + new Vector2(j * resolution - resolution*1.5f, i * resolution - resolution*1.5f),
                        type = CellType.Out,
                        size = resolution
                    };
                }
            }
        }

        public void Intersect(Vector2[] path)
        {
            int v1 = 0, h1 = 0;

            for (var i = 0; i < path.Length-1; i++)
            {
                var p0 = path[i] - min;
                var p1 = path[i+1] - min;

                int seg = (int)((p0 - p1).sqrMagnitude / (resolution*0.3*resolution*0.3f)) + 1;

                for (int j = 0; j <= seg; j++)
                {
                    var t = (float) j/seg;
                    var p = p0*(1.0f - t) + p1*t;

                    var h0 = (short)((p.x + resolution*1.5f) / resolution);
                    var v0 = (short)((p.y + resolution*1.5f) / resolution);

                    Exploder2DUtils.Assert(v0 < vCount);
                    Exploder2DUtils.Assert(h0 < hCount);

                    grid[v0][h0].type = CellType.Edge;

                    if (j == 0 || j == seg)
                    {
                        grid[v0][h0].LineIntersection(p0 + min, p1 + min, v0, h0, intersections);
                        grid[v0][h0].PointIntersection(p + min, v0, h0, intersections);
                        grid[v0][h0].AddPnt(p + min);
                    }
                    else if (v0 != v1 || h0 != h1)
                    {
                        grid[v0][h0].LineIntersection(p0 + min, p1 + min, v0, h0, intersections);
                    }

                    v1 = v0;
                    h1 = h0;
                }
            }

            for (var i = 0; i < vCount; i++)
            {
                int leftEdge = -1;
                int rightEdge = -1;

                for (var j = 0; j < hCount; j++)
                {
                    if (leftEdge == -1 && grid[i][j].type == CellType.Edge)
                    {
                        leftEdge = j;
                    }

                    if (rightEdge == -1 && grid[i][hCount - 1 - j].type == CellType.Edge)
                    {
                        rightEdge = hCount - 1 - j;
                    }

                    if (leftEdge != -1 && rightEdge != -1)
                    {
                        break;
                    }
                }

                if (leftEdge != -1 && rightEdge != -1)
                {
                    for (int j = leftEdge+1; j < rightEdge; j++)
                    {
                        if (grid[i][j].type != CellType.Edge)
                        {
                            grid[i][j].type = CellType.In;
                        }
                    }
                }
            }

            foreach (var inter in intersections)
            {
                var cell0 = (int) inter.Key;
                var cell1 = (int) (inter.Key >> 32);

                var hp0 = (short) cell0;
                var vp0 = (short) (cell0 >> 16);

                var hp1 = (short) cell1;
                var vp1 = (short) (cell1 >> 16);

                grid[vp0][hp0].AddPnt(inter.Value);
                grid[vp1][hp1].AddPnt(inter.Value);
            }
        }

        public void Triangulate()
        {
            for (int i = 1; i < vCount-1; i++)
            {
                for (int j = 1; j < hCount-1; j++)
                {
                    if (grid[i][j].type == CellType.Edge)
                    {
                        var center = grid[i][j].pos;

                        var eHigh = (int)grid[i][j - 1].type + (int)grid[i - 1][j - 1].type + (int)grid[i - 1][j].type;
                        var e = (int)grid[i][j - 1].type + (int)grid[i + 1][j - 1].type + (int)grid[i + 1][j].type;

                        if (e > eHigh)
                        {
                            center = grid[i + 1][j].pos;
                            eHigh = e;
                        }

                        e = (int)grid[i + 1][j].type + (int)grid[i + 1][j + 1].type + (int)grid[i][j + 1].type;

                        if (e > eHigh)
                        {
                            center = grid[i + 1][j + 1].pos;
                            eHigh = e;
                        }

                        e = (int)grid[i][j + 1].type + (int)grid[i - 1][j - 1].type + (int)grid[i - 1][j].type;

                        if (e > eHigh)
                            center = grid[i][j + 1].pos;

                        grid[i][j].center = center;
                    }
                }
            }
        }

        public void DebugDraw(Transform transform)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    Vector2 center = transform.TransformPoint(grid[i][j].center);
                    Vector2 pos = transform.TransformPoint(grid[i][j].pos);
                    Vector2 right = transform.TransformDirection(Vector2.right*resolution*1.0f);
                    Vector2 up = transform.TransformDirection(Vector2.up*resolution*1.0f);

                    Color color;
                    switch (grid[i][j].type)
                    {
                        case CellType.Edge:
                            color = Color.red;
                            break;

                        case CellType.In:
                            color = Color.yellow;
                            break;

                        default:
                            color = Color.white;
                            break;
                    }

                    UnityEngine.Debug.DrawLine(pos, pos + right, color, 10);
                    UnityEngine.Debug.DrawLine(pos, pos + up, color, 10);

                    if (grid[i][j].type == CellType.Edge)
                    {
                        if (grid[i][j].pnts != null)
                        foreach (var p in grid[i][j].pnts)
                        {
                            Vector2 p0 = transform.TransformPoint(p);
                            UnityEngine.Debug.DrawLine(center, p0, Color.magenta, 10);
                        }
                    }
                }
            }
        }
    }
}
