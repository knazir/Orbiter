//#define DBG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exploder2D.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Exploder2D
{
    public partial class Exploder2DObject
    {
        private bool ProcessCutterRandomized(out long cuttingTime)
        {
            Exploder2DUtils.Assert(state == State.ProcessCutter || state == State.DryRun, "Wrong state!");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            bool cutting = true;
            bool timeBudgetStop = false;
            var cycleCounter = 0;
            cuttingTime = 0;

            while (cutting)
            {
                cycleCounter++;

                if (cycleCounter > TargetFragments)
                {
                    Exploder2DUtils.Log("Explode Infinite loop!");
                    break;
                }

                newFragments.Clear();
                meshToRemove.Clear();

                cutting = false;
                var fragmentsCount = meshSet.Count;

                foreach (var mesh in meshSet)
                {
                    if (levelCount[mesh.level] > 0)
                    {
                        var randomLineNormalized = Random.insideUnitCircle;

                        if (!mesh.transform)
                        {
                            continue;
                        }

                        var plane = Core.Math.Line2D.CreateNormalPoint(randomLineNormalized, mesh.centroidLocal);

#if DBG
                        cuts++;
#endif

                        if (mesh.option)
                        {
                            splitMeshIslands |= mesh.option.SplitMeshIslands;
                        }

                        List<CutterMesh> meshes = null;
                        cutter.Cut(mesh.spriteMesh, mesh.transform, plane, ref meshes);

                        cutting = true;

                        if (meshes != null)
                        {
                            foreach (var cutterMesh in meshes)
                            {
                                newFragments.Add(new CutMesh
                                {
                                    spriteMesh = cutterMesh.mesh,
                                    centroidLocal = cutterMesh.centroidLocal,

                                    sprite = mesh.sprite,
                                    vertices = mesh.vertices,
                                    transform = mesh.transform,
                                    distance = mesh.distance,
                                    level = mesh.level,
                                    fragments = mesh.fragments,
                                    original = mesh.original,

                                    parent = mesh.transform.parent,
                                    position = mesh.transform.position,
                                    rotation = mesh.transform.rotation,
                                    localScale = mesh.transform.localScale,

                                    sortingLayer = mesh.sortingLayer,
                                    orderInLayer = mesh.orderInLayer,
									color = mesh.color,

                                    option = mesh.option,
                                });
                            }

                            meshToRemove.Add(mesh);

                            levelCount[mesh.level] -= 1;

                            // stop this madness!
                            if (fragmentsCount + newFragments.Count - meshToRemove.Count >= TargetFragments)
                            {
                                cuttingTime = stopWatch.ElapsedMilliseconds;
                                meshSet.ExceptWith(meshToRemove);
                                meshSet.UnionWith(newFragments);
                                return true;
                            }

                            // computation took more than FrameBudget ... 
                            if (stopWatch.ElapsedMilliseconds > FrameBudget)
                            {
                                timeBudgetStop = true;
                                break;
                            }
                        }
                    }
                }

                meshSet.ExceptWith(meshToRemove);
                meshSet.UnionWith(newFragments);

                if (timeBudgetStop)
                {
                    break;
                }
            }

            cuttingTime = stopWatch.ElapsedMilliseconds;

            // explosion is finished
            if (!timeBudgetStop)
            {
                return true;
            }

            return false;
        }
    }
}
