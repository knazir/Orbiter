// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder2D
{
    class Exploder2DSettings
    {
        public Vector2 Position;
        public Vector2 ForceVector;
        public float Force;
        public float FrameBudget;
        public float Radius;
        public float DeactivateTimeout;
        public int id;
        public int TargetFragments;
        public DeactivateOptions DeactivateOptions;
        public Exploder2DObject.FragmentOption FragmentOptions;
        public Exploder2DObject.SFXOption SfxOptions;
        public Exploder2DObject.OnExplosion Callback;
        public bool DontUseTag;
        public bool UseForceVector;
        public bool ExplodeSelf;
        public bool HideSelf;
        public bool DestroyOriginalObject;
        public bool ExplodeFragments;
        public bool SplitMeshIslands;
        public bool processing;
    }

    public class ExploderQueue
    {
        private readonly Queue<Exploder2DSettings> queue;
        private readonly Exploder2DObject _exploder2D;

        public ExploderQueue(Exploder2DObject _exploder2D)
        {
            this._exploder2D = _exploder2D;
            queue = new Queue<Exploder2DSettings>();
        }

        public bool IsProcessing()
        {
            return queue.Count > 0;
        }

        public void Explode(Exploder2DObject.OnExplosion callback)
        {
            var settings = new Exploder2DSettings
            {
                Position = Exploder2DUtils.GetCentroid(_exploder2D.gameObject),
                DontUseTag = _exploder2D.DontUseTag,
                Radius = _exploder2D.Radius,
                ForceVector = _exploder2D.ForceVector,
                UseForceVector = _exploder2D.UseForceVector,
                Force = _exploder2D.Force,
                FrameBudget = _exploder2D.FrameBudget,
                TargetFragments = _exploder2D.TargetFragments,
                DeactivateOptions = _exploder2D.DeactivateOptions,
                DeactivateTimeout = _exploder2D.DeactivateTimeout,
                ExplodeSelf = _exploder2D.ExplodeSelf,
                HideSelf = _exploder2D.HideSelf,
                DestroyOriginalObject = _exploder2D.DestroyOriginalObject,
                ExplodeFragments = _exploder2D.ExplodeFragments,
                SplitMeshIslands = _exploder2D.SplitMeshIslands,
                FragmentOptions = _exploder2D.FragmentOptions.Clone(),
                SfxOptions = _exploder2D.SFXOptions.Clone(),
                Callback = callback,
                processing = false,
            };

            queue.Enqueue(settings);
            ProcessQueue();
        }

        void ProcessQueue()
        {
            if (queue.Count > 0)
            {
                var peek = queue.Peek();

                if (!peek.processing)
                {
                    _exploder2D.DontUseTag = peek.DontUseTag;
                    _exploder2D.Radius = peek.Radius;
                    _exploder2D.ForceVector = peek.ForceVector;
                    _exploder2D.UseForceVector = peek.UseForceVector;
                    _exploder2D.Force = peek.Force;
                    _exploder2D.FrameBudget = peek.FrameBudget;
                    _exploder2D.TargetFragments = peek.TargetFragments;
                    _exploder2D.DeactivateOptions = peek.DeactivateOptions;
                    _exploder2D.DeactivateTimeout = peek.DeactivateTimeout;
                    _exploder2D.ExplodeSelf = peek.ExplodeSelf;
                    _exploder2D.HideSelf = peek.HideSelf;
                    _exploder2D.DestroyOriginalObject = peek.DestroyOriginalObject;
                    _exploder2D.ExplodeFragments = peek.ExplodeFragments;
                    _exploder2D.SplitMeshIslands = peek.SplitMeshIslands;
                    _exploder2D.FragmentOptions = peek.FragmentOptions;
                    _exploder2D.SFXOptions = peek.SfxOptions;

                    peek.id = Random.Range(int.MinValue, int.MaxValue);
                    peek.processing = true;

                    _exploder2D.StartExplosionFromQueue(peek.Position, peek.id, peek.Callback);
                }
            }
        }

        public void OnExplosionFinished(int id)
        {
            var explosion = queue.Dequeue();
            Exploder2DUtils.Assert(explosion.id == id, "Explosion id mismatch!");
            ProcessQueue();
        }
    }
}
