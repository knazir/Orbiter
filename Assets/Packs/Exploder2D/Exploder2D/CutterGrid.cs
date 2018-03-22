#define DBG

using System.Diagnostics;

namespace Exploder2D
{
    public partial class Exploder2DObject
    {
        private bool ProcessCutterGrid(out long cuttingTime)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (var mesh in meshSet)
            {
                if (!mesh.transform)
                {
                    continue;
                }

                //
                // step 1: calculate convex hull
                //
                var hull = Utils.Hull2D.ChainHull2D(mesh.spriteMesh.vertices);

                //
                // step 2: create grid
                //
                var grid = new Core.Grid(mesh.spriteMesh.min, mesh.spriteMesh.max, 0.5f);

                //
                // step 3: find grid intersection with convex hull
                grid.Intersect(hull);

                //
                // step 4: triangulate the intersected grid
                //
                grid.Triangulate();

                grid.DebugDraw(mesh.transform);

                break;
            }

            watch.Stop();
            cuttingTime = watch.ElapsedMilliseconds;
            return true;
        }
    }
}
