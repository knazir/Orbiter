// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder2D
{
    /// <summary>
    /// fragment pool is a manager for fragments (create/recycle/...)
    /// </summary>
    public class FragmentPool2D : MonoBehaviour
    {
        /// <summary>
        /// instance
        /// </summary>
        public static FragmentPool2D Instance
        {
            get
            {
                if (instance == null)
                {
                    var fragmentRoot = new GameObject("FragmentRoot");
                    instance = fragmentRoot.AddComponent<FragmentPool2D>();
                }

                return instance;
            }
        }

        private static FragmentPool2D instance;
        private Fragment2D[] pool;
        private float fragmentSoundTimeout;

        void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            DestroyFragments();
            instance = null;
        }

        /// <summary>
        /// gets the size of the pool
        /// </summary>
        public int PoolSize { get { return pool.Length; } }

        /// <summary>
        /// returns all pool
        /// </summary>
        public Fragment2D[] Pool { get { return pool; } }

        /// <summary>
        /// timeout between impact sound effects
        /// </summary>
        public float HitSoundTimeout = 1.0f;

        /// <summary>
        /// maximal number of particle emitters
        /// </summary>
        public int MaxEmitters = 1000;

        /// <summary>
        /// returns list of fragments with requested size
        /// this method pick fragments hidden from camera or sleeping rather then visible
        /// </summary>
        /// <param name="size">number of requested fragments</param>
        /// <returns>list of fragments</returns>
        public List<Fragment2D> GetAvailableFragments(int size)
        {
            if (size > pool.Length)
            {
                Debug.LogError("Requesting pool size higher than allocated! Please call Allocate first! " + size);
                return null;
            }

            if (size == pool.Length)
            {
                return new List<Fragment2D>(pool);
            }

            var fragments = new List<Fragment2D>();

            int counter = 0;

            // get deactivated fragments first
            foreach (var fragment in pool)
            {
                // get invisible fragments
                if (!fragment.activeObj)
                {
                    fragments.Add(fragment);
                    counter++;
                }

                if (counter == size)
                {
                    return fragments;
                }
            }

            foreach (var fragment in pool)
            {
                // get invisible fragments
                if (!fragment.visible)
                {
                    fragments.Add(fragment);
                    counter++;
                }

                if (counter == size)
                {
                    return fragments;
                }
            }

            // there are still live fragments ... get sleeping ones
            if (counter < size)
            {
                foreach (var fragment in pool)
                {
                    if (fragment.IsSleeping() && fragment.visible)
                    {
                        Exploder2DUtils.Assert(!fragments.Contains(fragment), "!!!");
                        fragments.Add(fragment);
                        counter++;
                    }

                    if (counter == size)
                    {
                        return fragments;
                    }
                }
            }

            // there are still live fragments...
            if (counter < size)
            {
                foreach (var fragment in pool)
                {
                    if (!fragment.IsSleeping() && fragment.visible)
                    {
                        Exploder2DUtils.Assert(!fragments.Contains(fragment), "!!!");
                        fragments.Add(fragment);
                        counter ++;
                    }

                    if (counter == size)
                    {
                        return fragments;
                    }
                }
            }

            Exploder2DUtils.Assert(false, "ERROR!!!");
            return null;
        }

        /// <summary>
        /// create pool (array) of fragment game objects with all necessary components
        /// </summary>
        /// <param name="poolSize">number of fragments</param>
        public void Allocate(int poolSize)
        {
            Exploder2DUtils.Assert(poolSize > 0, "");

            if (pool == null || pool.Length < poolSize)
            {
                DestroyFragments();

                pool = new Fragment2D[poolSize];

                for (int i = 0; i < poolSize; i++)
                {
                    var fragment = new GameObject("fragment_" + i);
                    fragment.AddComponent<SpriteRenderer>();
                    fragment.AddComponent<PolygonCollider2D>();
                    fragment.AddComponent<Rigidbody2D>();
                    fragment.AddComponent<Exploder2DOption>();

                    var fragmentComponent = fragment.AddComponent<Fragment2D>();

                    fragment.transform.parent = gameObject.transform;

                    pool[i] = fragmentComponent;

                    Exploder2DUtils.SetActiveRecursively(fragment.gameObject, false);

                    fragmentComponent.RefreshComponentsCache();

                    fragmentComponent.Sleep();
                }
            }
        }

        /// <summary>
        /// wake up physics (just for testing...)
        /// </summary>
        public void WakeUp()
        {
            foreach (var fragment in pool)
            {
                fragment.WakeUp();
            }
        }

        /// <summary>
        /// sleep physics (just for testing...)
        /// </summary>
        public void Sleep()
        {
            foreach (var fragment in pool)
            {
                fragment.Sleep();
            }
        }

        /// <summary>
        /// destroy objects in the pool
        /// </summary>
        public void DestroyFragments()
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    if (fragment)
                    {
                        Object.Destroy(fragment.gameObject);
                    }
                }

                pool = null;
            }
        }

        /// <summary>
        /// deactivate all fragments immediately
        /// </summary>
        public void DeactivateFragments()
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    if (fragment)
                    {
                        fragment.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// set options for deactivations
        /// </summary>
        public void SetDeactivateOptions(DeactivateOptions options, FadeoutOptions fadeoutOptions, float timeout)
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    fragment.deactivateOptions = options;
                    fragment.deactivateTimeout = timeout;
                    fragment.fadeoutOptions = fadeoutOptions;
                }
            }
        }

        /// <summary>
        /// set options for explodable fragments, if true fragments can be destroyed again
        /// </summary>
        /// <param name="explodable"></param>
        public void SetExplodableFragments(bool explodable, bool dontUseTag)
        {
            if (pool != null)
            {
                if (dontUseTag)
                {
                    foreach (var fragment in pool)
                    {
                        if (fragment.gameObject)
                        {
                            if (!fragment.gameObject.GetComponent<Explodable2D>())
                            {
                                fragment.gameObject.AddComponent<Explodable2D>();
                            }
                        }
                    }
                }
                else
                {
                    if (explodable)
                    {
                        foreach (var fragment in pool)
                        {
                            fragment.tag = Exploder2DObject.Tag;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// set options for fragment rigid bodies and layer
        /// </summary>
        public void SetFragmentPhysicsOptions(Exploder2DObject.FragmentOption options)
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    if (fragment)
                    {
                        fragment.SetFragmentPhysicsOptions(options);
                    }
                }
            }
        }

        /// <summary>
        /// set options for fragment sound and particle effects
        /// </summary>
        /// <param name="sfx"></param>
        public void SetSFXOptions(Exploder2DObject.SFXOption sfx)
        {
            if (pool != null)
            {
                HitSoundTimeout = sfx.HitSoundTimeout;
                MaxEmitters = sfx.EmitersMax;

                for (int i = 0; i < pool.Length; i++)
                {
                    pool[i].SetSFX(sfx, i < MaxEmitters);
                }
            }
        }

        /// <summary>
        /// returns list of currently active (visible) fragments
        /// </summary>
        /// <returns></returns>
        public List<Fragment2D> GetActiveFragments()
        {
            if (pool != null)
            {
                var list = new List<Fragment2D>(pool.Length);

                foreach (var fragment in pool)
                {
                    if (Exploder2DUtils.IsActive(fragment.gameObject))
                    {
                        list.Add(fragment);
                    }
                }

                return list;
            }

            return null;
        }

        void Update()
        {
            fragmentSoundTimeout -= Time.deltaTime;
        }

        public void OnFragmentHit()
        {
            fragmentSoundTimeout = HitSoundTimeout;
        }

        public bool CanPlayHitSound()
        {
            return fragmentSoundTimeout <= 0.0f;
        }
    }
}
