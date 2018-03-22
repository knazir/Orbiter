// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//#define DBG
//#define PROFILING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exploder2D.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Exploder2D
{
    /// <summary>
    /// main Exploder class
    /// usage:
    /// 1) assign this class to any GameObject (empty or with mesh)
    /// 2) adjust parameters (radius, number of fragments, force, ...)
    /// 3) call function Explode()
    /// 4) wait one or more frames for calculating the explosion
    /// 5) see the explosion
    /// 7) DONE
    /// </summary>
    public partial class Exploder2DObject : MonoBehaviour
    {
        /// <summary>
        /// name of the tag for destroyable objects
        /// only objects with this tag can be destroyed, other objects are ignored
        /// </summary>
        public static string Tag = "Exploder2D";

        /// <summary>
        /// flag for not tagging Explodable objects
        /// if you set this to TRUE you will have to assign "Explodable" to your GameObject instead of Tagging it
        /// this is useful if you already have tagged GameObject and you don't want to re-tag it to "Exploder"
        /// </summary>
        public bool DontUseTag = false;

        /// <summary>
        /// radius of explosion
        /// see red wire-frame sphere inside scene view
        /// </summary>
        public float Radius = 10;

        /// <summary>
        /// vector of explosion force
        /// NOTE: this parameter is used only if "UseForceVector == true"
        /// ex.: with Vector(0, 0, 1) exploding fragments will fly in "UP" direction
        /// </summary>
        public Vector2 ForceVector = Vector2.up;

        /// <summary>
        /// flag for using "ForceVector"
        /// if this flag is false explosion force is distributed randomly on unit sphere (from sphere center to all directions)
        /// </summary>
        public bool UseForceVector;

        /// <summary>
        /// force of explosion
        /// more means higher velocity of exploding fragments
        /// </summary>
        public float Force = 30;

        /// <summary>
        /// time budget in [ms] for processing explosion calculation in one frame
        /// if the calculation takes more time it is stopped and resumed in next frame
        /// recommended settings: 15 - 30 (30 frame-per-second game takes approximately 33ms in one frame)
        /// for example:
        /// if your game is running 30 fps in average this value should be lower than 30 (~ 15 can be ok)
        /// if your game is running 60 fps in average this value can be 30 and more
        /// in other words, higher the value is faster the calculation is finished but more time in one frame can take
        /// </summary>
        public float FrameBudget = 15;

        /// <summary>
        /// number of target fragments that will be created by cutting the exploding objects
        /// more fragments means more calculation and more PhysX overhead
        /// </summary>
        public int TargetFragments = 30;

        /// <summary>
        /// deactivate options for fragment pieces
        /// </summary>
        public DeactivateOptions DeactivateOptions = DeactivateOptions.Never;

        /// <summary>
        /// deactivate timeout, valid only if DeactivateOptions == DeactivateTimeout
        /// </summary>
        public float DeactivateTimeout = 10.0f;

        /// <summary>
        /// options for fading out fragments after explosion
        /// </summary>
        public FadeoutOptions FadeoutOptions = FadeoutOptions.None;

        /// <summary>
        /// flag for destroying this GameObject if there is any mesh
        /// </summary>
        public bool ExplodeSelf = true;

        /// <summary>
        /// disable scanning for explodable objects in radius
        /// this options is valid only if ExplodeSelf is true
        /// </summary>
        public bool DisableRadiusScan = false;

        /// <summary>
        /// flag for hiding this game object after explosion
        /// </summary>
        public bool HideSelf = true;

        /// <summary>
        /// flag for destroying game object after explosion
        /// </summary>
        public bool DestroyOriginalObject = false;

        /// <summary>
        /// flag for destroying already destroyed fragments
        /// if this is true you can destroy object and all the new created fragments
        /// you can keep destroying fragments until they are small enough (see Fragment.cs)
        /// </summary>
        public bool ExplodeFragments = true;

        /// <summary>
        /// by enabling this Exploder will handle all objets in radius equally
        /// they will have the same number of fragments
        /// </summary>
        public bool UniformFragmentDistribution = false;

        /// <summary>
        /// option for separating not-connecting parts of the same mesh
        /// if this option is enabled all exploding fragments are searched for not connecting 
        /// parts of the same mesh and these parts are separated into new fragments
        /// example:
        /// if you explode a "chair" model, mesh cutter cut it into pieces however it is likely
        /// possible that one of the fragments will contain not-connecting "chair legs" (no sitting part) 
        /// and it will look not very realistic, by enabling this all not connecting "chair legs" are found 
        /// and split into different meshes
        /// 
        /// IMPORTANT: by enabling this you can achieve better visual quality but it will take more CPU power
        /// (more frames to process the explosion)
        /// </summary>
        public bool SplitMeshIslands = false;

        /// <summary>
        /// disable exploder queue, only one explosion at the time is allowed
        /// </summary>
        public bool DisableQueue = false;

        /// <summary>
        /// mesh cutter options, use this to change shape of fragments
        /// </summary>
        public enum CutStrategy
        {
            /// <summary>
            /// default, cutting plane is randomized, realistic looking fracture
            /// </summary>
            Randomized,

            /// <summary>
            /// TODO:
            /// cutting plane is processed line by line in the grid, result fragments will have rectangular shape
            /// </summary>
//            Grid,
        }

        /// <summary>
        /// mesh cuter options, use this to change shape of fragments
        /// </summary>
        public CutStrategy CuttingStrategy = CutStrategy.Randomized;

        /// <summary>
        /// maximum number of all available fragments
        /// this number should be higher than TargetFragments
        /// </summary>
        public int FragmentPoolSize = 200;

        [Serializable]
        public class SFXOption
        {
            public AudioClip ExplosionSoundClip;
            public AudioClip FragmentSoundClip;
            public GameObject FragmentEmitter;
            public float HitSoundTimeout;
            public int EmitersMax;

            public SFXOption Clone()
            {
                return new SFXOption
                {
                    ExplosionSoundClip = ExplosionSoundClip,
                    FragmentSoundClip = FragmentSoundClip,
                    FragmentEmitter = FragmentEmitter,
                    HitSoundTimeout = HitSoundTimeout,
                    EmitersMax = EmitersMax,
                };
            }
        }

        public SFXOption SFXOptions = new SFXOption
        {
            ExplosionSoundClip = null,
            FragmentSoundClip = null,
            FragmentEmitter = null,
            HitSoundTimeout = 0.3f,
            EmitersMax = 1000,
        };

        [Serializable]
        public class FragmentOption
        {
            public string Layer;

            public bool InheritSortingLayer;

            public string SortingLayer;

            public bool InheritOrderInLayer;

            public int OrderInLayer;

            public bool InheritSpriteRendererColor;

            public Color SpriteRendererColor;

            /// <summary>
            /// maximal velocity the fragment can fly
            /// </summary>
            public float MaxVelocity;

            /// <summary>
            /// if set to true, mass, velocity and angular velocity will be inherited from original game object
            /// </summary>
            public bool InheritParentPhysicsProperty;

            /// <summary>
            /// mass property which will apply to fragments
            /// NOTE: if the parent object object has rigidbody and InheritParentPhysicsProperty is true
            /// the mass property for fragments will be calculated based on this equation (fragmentMass = parentMass / TargetFragments)
            /// </summary>
            public float Mass;

            /// <summary>
            /// gravity settings
            /// </summary>
            public float GravityScale;

            /// <summary>
            /// disable collider on fragments
            /// </summary>
            public bool DisableColliders;

            /// <summary>
            /// angular velocity of fragments
            /// </summary>
            public float AngularVelocity;

            /// <summary>
            /// direction of angular velocity
            /// </summary>
            public Vector2 AngularVelocityVector;

            /// <summary>
            /// set this to true if you want to have randomly rotated fragments
            /// </summary>
            public bool RandomAngularVelocityVector;

            public FragmentOption Clone()
            {
                return new FragmentOption
                {
                    Layer = Layer,
                    InheritSortingLayer = InheritSortingLayer,
                    SortingLayer = SortingLayer,
                    OrderInLayer = OrderInLayer,
                    InheritSpriteRendererColor = InheritSpriteRendererColor,
                    SpriteRendererColor = SpriteRendererColor,
                    InheritOrderInLayer = InheritOrderInLayer,
                    Mass = Mass,
                    DisableColliders = DisableColliders,
                    GravityScale = GravityScale,
                    MaxVelocity = MaxVelocity,
                    InheritParentPhysicsProperty = InheritParentPhysicsProperty,
                    AngularVelocity = AngularVelocity,
                    AngularVelocityVector = AngularVelocityVector,
                    RandomAngularVelocityVector = RandomAngularVelocityVector,
                };
            }
        }

        /// <summary>
        /// global settings for fragment options
        /// constrains for rigid bodies and name of the layer
        /// </summary>
        public FragmentOption FragmentOptions = new FragmentOption
        {
            Layer = "Default",
            SortingLayer = "Default",
            InheritSortingLayer = true,
            InheritOrderInLayer = true,
            OrderInLayer = 0,
            InheritSpriteRendererColor = true,
            SpriteRendererColor = Color.white,
            Mass = 20,
            MaxVelocity = 1000,
            DisableColliders = false,
            GravityScale = 1.0f,
            InheritParentPhysicsProperty = true,
            AngularVelocity = 1.0f,
            AngularVelocityVector = Vector2.up,
            RandomAngularVelocityVector = true,
        };

        /// <summary>
        /// explosion callback
        /// this callback is called when the calculation is finished and the physics explosion is started
        /// this is useful for playing explosion sound effect, particles etc.
        /// </summary>
        public delegate void OnExplosion(float timeMS, ExplosionState state);

        /// <summary>
        /// state of explosion, this enum used as parameter in callback
        /// </summary>
        public enum ExplosionState
        {
            /// <summary>
            /// explosion just started to show flying fragment pieces, but it can take several frames to
            /// start all pieces (activate rigid bodies, etc...)
            /// this is a good place to play explosion soundeffects
            /// </summary>
            ExplosionStarted,

            /// <summary>
            /// explosion process is finally completed, all fragment pieces are generated and visible
            /// this is a good place to get all active fragments and do watever necessery (particles, FX, ...)
            /// </summary>
            ExplosionFinished,
        }

        /// <summary>
        /// main function, call this to start explosion
        /// </summary>
        public void Explode()
        {
            Explode(null);
        }

        /// <summary>
        /// main function, call this to start explosion
        /// </summary>
        /// <param name="callback">callback to be called when explosion calculation is finished
        /// play your sound effects or particles on this callback
        /// </param>
        public void Explode(OnExplosion callback)
        {
            if (DisableQueue && queue.IsProcessing())
            {
                return;
            }

            queue.Explode(callback);
        }

        /// <summary>
        /// callback from queue, do not call this unles you know what you are doing!
        /// </summary>
        public void StartExplosionFromQueue(Vector2 pos, int id, OnExplosion callback)
        {
            Exploder2DUtils.Assert(state == State.None, "Wrong state: " + state);

            mainCentroid = pos;
            explosionID = id;
            state = State.Preprocess;
            ExplosionCallback = callback;

#if DBG
        processingTime = 0.0f;
        preProcessingTime = 0.0f;
        postProcessingTime = 0.0f;
        postProcessingTimeEnd = 0.0f;
        isolatingIslandsTime = 0.0f;
        cuts = 0;
#endif
        }

        /// <summary>
        /// cracking callback
        /// this callback is called when the explosion calculation is finished and the objects are ready for explosion
        /// </summary>
        public delegate void OnCracked();

        /// <summary>
        /// crack will calculate fragments and prepare object for explosion
        /// Use this method in combination with ExplodeCracked()
        /// Purpose of this method is to get higher performance of explosion, Crack() will 
        /// calculate the explosion and prepare all fragments. Calling ExplodeCracked() will 
        /// then start the explosion (flying fragments...) immediately
        /// </summary>
        public void Crack()
        {
            Crack(null);
        }

        /// <summary>
        /// crack will calculate fragments and prepare object for explosion
        /// Use this method in combination with ExplodeCracked()
        /// Purpose of this method is to get higher performance of explosion, Crack() will 
        /// calculate the explosion and prepare all fragments. Calling ExplodeCracked() will 
        /// then start the explosion (flying fragments...) immediately
        /// </summary>
        public void Crack(OnCracked callback)
        {
            Exploder2DUtils.Assert(!crack, "Another crack in progress!");

            if (!crack)
            {
                CrackedCallback = callback;
                crack = true;
                cracked = false;
                Explode(null);
            }
        }

        /// <summary>
        /// explode cracked objects
        /// Use this method in combination with Crack()
        /// Purpose of this method is to get higher performance of explosion, Crack() will
        /// calculate the explosion and prepare all fragments. Calling ExplodeCracked() will
        /// then start the explosion (flying fragments...) immediately
        /// </summary>
        public void ExplodeCracked(OnExplosion callback)
        {
            Exploder2DUtils.Assert(crack, "You must call Crack() first!");

            if (cracked)
            {
                PostCrackExplode(callback);
                crack = false;
            }
        }

        /// <summary>
        /// explode cracked objects
        /// Use this method in combination with Crack()
        /// Purpose of this method is to get higher performance of explosion, Crack() will 
        /// calculate the explosion and prepare all fragments. Calling ExplodeCracked() will 
        /// run the explosion immediately.
        /// </summary>
        public void ExplodeCracked()
        {
            ExplodeCracked(null);
        }

        private OnExplosion ExplosionCallback;
        private OnCracked CrackedCallback;
        private bool crack;
        private bool cracked;

        enum State
        {
            None,
            Preprocess,
            ProcessCutter,
            IsolateMeshIslands,
            PostprocessInit,
            Postprocess,
            DryRun,
        }

        private State state;
        private ExploderQueue queue;
        private Core.MeshCutter2D cutter;
        private Stopwatch timer;

        private HashSet<CutMesh> newFragments;
        private HashSet<CutMesh> meshToRemove;
        private HashSet<CutMesh> meshSet;
        private int[] levelCount;

        private HashSet<CutMesh> meshToCut;

        private int poolIdx;
        private List<CutMesh> postList;
        private List<Fragment2D> pool;
        private Vector2 mainCentroid;

        private bool splitMeshIslands;
        private List<CutMesh> islands;

        private int explosionID;

        private AudioSource audioSource;

#if DBG

    private int processingFrames = 0;
    private int postProcessingFrames = 0;
    private int isolatingIslandsFrames = 0;

    private float processingTime = 0.0f;
    private float preProcessingTime = 0.0f;
    private float postProcessingTime = 0.0f;
    private float postProcessingTimeEnd = 0.0f;
    private float isolatingIslandsTime = 0.0f;
    private int cuts = 0;
#endif

        private struct CutMesh
        {
            public SpriteMesh spriteMesh;
            public Transform transform;

            public Transform parent;
            public Vector2 position;
            public Quaternion rotation;
            public Vector2 localScale;

            public Sprite sprite;
            public GameObject original;
            public Vector2 centroidLocal;
            public float distance;
            public int vertices;
            public int level;
            public int fragments;

            public string sortingLayer;
            public int orderInLayer;
            public Color color;

            public Exploder2DOption option;
        }

        private void Awake()
        {
            // init cutter
            cutter = new Core.MeshCutter2D();
            cutter.Init(512, 512);
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

            // init pool
            FragmentPool2D.Instance.Allocate(FragmentPoolSize);
            FragmentPool2D.Instance.SetDeactivateOptions(DeactivateOptions, FadeoutOptions, DeactivateTimeout);
            FragmentPool2D.Instance.SetExplodableFragments(ExplodeFragments, DontUseTag);
            FragmentPool2D.Instance.SetFragmentPhysicsOptions(FragmentOptions);
            FragmentPool2D.Instance.SetSFXOptions(SFXOptions);
            timer = new Stopwatch();

            // init queue
            queue = new ExploderQueue(this);

            if (DontUseTag)
            {
                gameObject.AddComponent<Explodable2D>();
            }
            else
            {
                gameObject.tag = Tag;
            }

            state = State.DryRun;

            PreAllocateBuffers();

            state = State.None;

            if (SFXOptions.ExplosionSoundClip)
            {
                audioSource = gameObject.GetComponent<AudioSource>();

                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        void PreAllocateBuffers()
        {
            // kick memory allocator for better performance at startup

            newFragments = new HashSet<CutMesh>();
            meshToRemove = new HashSet<CutMesh>();
            meshSet = new HashSet<CutMesh>();
            meshToCut = new HashSet<CutMesh>();

            for (int i = 0; i < 64; i++)
            {
                meshSet.Add(new CutMesh());
            }

            levelCount = new int[64];

            // run dummy preprocess to run more allocations...
            Preprocess();
            long t;
            ProcessCutterRandomized(out t);
        }

        void OnDrawGizmos()
        {
            if (enabled && !(ExplodeSelf && DisableRadiusScan))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Exploder2DUtils.GetCentroid(gameObject), Radius);
            }
        }

        private int GetLevelFragments(int level, int fragmentsMax)
        {
            return (fragmentsMax * 2) / (level * level + level) + 1;
        }

        private int GetLevel(float distance, float radius)
        {
            Exploder2DUtils.Assert(distance >= 0.0f, "");
            Exploder2DUtils.Assert(radius >= 0.0f, "");

            var normDistance = distance / radius * 6;
            var level = (int)normDistance / 2 + 1;

            return Mathf.Clamp(level, 0, 10);
        }

        struct MeshData
        {
            public SpriteMesh spriteMesh;
            public Sprite sprite;
            public SpriteRenderer spriteRenderer;
            public GameObject gameObject;
            public GameObject parentObject;
            public Vector2 centroid;
        }

        private List<MeshData> GetMeshData(GameObject obj)
        {
            var sprites = obj.GetComponentsInChildren<SpriteRenderer>();

            var outList = new List<MeshData>(sprites.Length);

            for (int i = 0; i < sprites.Length; i++)
            {
                var spriteMesh = new SpriteMesh(sprites[i].sprite);

                Vector2 ctr = obj.transform.TransformPoint(spriteMesh.GetCentroidLocal());
//                UnityEngine.Debug.DrawLine(ctr, ctr + Vector2.up*100, Color.red, 100);

                outList.Add(new MeshData
                {
                    spriteMesh = spriteMesh,
                    spriteRenderer = sprites[i],
                    centroid = ctr,
                    sprite = sprites[i].sprite,
                    gameObject = sprites[i].gameObject,
                    parentObject = obj,
                });
            }

            return outList;
        }

        bool IsExplodable(GameObject obj)
        {
            if (DontUseTag)
            {
                return obj.GetComponent<Explodable2D>() != null;
            }
            else
            {
                return obj.CompareTag(Exploder2DObject.Tag);
            }
        }

        private List<CutMesh> GetMeshList()
        {
            GameObject[] objects = null;

            if (DontUseTag)
            {
                var objs = FindObjectsOfType(typeof(Explodable2D));
                var objList = new List<GameObject>(objs.Length);

                foreach (var o in objs)
                {
                    var ex = (Explodable2D)o;

                    if (ex)
                    {
                        objList.Add(ex.gameObject);
                    }
                }

                objects = objList.ToArray();
            }
            else
            {
                objects = GameObject.FindGameObjectsWithTag(Tag);
            }

            var list = new List<CutMesh>(objects.Length);

            foreach (var o in objects)
            {
                // don't destroy the destroyer :)
                if (!ExplodeSelf && o == gameObject)
                {
                    continue;
                }

                // stop scanning for object is case of ExplodeSelf
                if (o != gameObject && ExplodeSelf && DisableRadiusScan)
                {
                    continue;
                }

//                UnityEngine.Debug.DrawLine(mainCentroid, mainCentroid + Vector2.up*100, Color.green, 100);

                var distance2 = (Exploder2DUtils.GetCentroid(o) - mainCentroid).sqrMagnitude;

                if (distance2 < Radius * Radius)
                {
                    var meshData = GetMeshData(o);
                    var meshDataLen = meshData.Count;

                    for (var i = 0; i < meshDataLen; i++)
                    {
                        var centroid = meshData[i].centroid;
                        var distance = (centroid - mainCentroid).magnitude;

//                    UnityEngine.Debug.Log("Distance: " + distance + " " + meshData[i].gameObject.name);

                        var layer = FragmentOptions.InheritSortingLayer
                            ? meshData[i].spriteRenderer.sortingLayerName
                            : FragmentOptions.SortingLayer;
                        var order = FragmentOptions.InheritOrderInLayer
                            ? meshData[i].spriteRenderer.sortingOrder
                            : FragmentOptions.OrderInLayer;
                        var color = FragmentOptions.InheritSpriteRendererColor
                            ? meshData[i].spriteRenderer.color
                            : FragmentOptions.SpriteRendererColor;

                        list.Add(new CutMesh
                        {
                            spriteMesh = meshData[i].spriteMesh,
                            centroidLocal = meshData[i].gameObject.transform.InverseTransformPoint(centroid),
                            transform = meshData[i].gameObject.transform,

                            sprite = meshData[i].sprite,
                            parent = meshData[i].gameObject.transform.parent,
                            position = meshData[i].gameObject.transform.position,
                            rotation = meshData[i].gameObject.transform.rotation,
                            localScale = meshData[i].gameObject.transform.localScale,

                            distance = distance,
                            level = GetLevel(distance, Radius),
                            original = meshData[i].parentObject,

                            sortingLayer = layer,
                            orderInLayer = order,
                            color = color,

                            option = o.GetComponent<Exploder2DOption>(),
                        });
                    }
                }
            }

            if (list.Count == 0)
            {
#if DBG
            Exploder2DUtils.Log("No explodable objects found!");
#endif
                return list;
            }

            list.Sort(delegate(CutMesh m0, CutMesh m1)
            {
                return (m0.level).CompareTo(m1.level);
            });

            // for the case when the count of objects is higher then target fragments
            if (list.Count > TargetFragments)
            {
                list.RemoveRange(TargetFragments - 1, list.Count - TargetFragments);
            }

            var levelMax = list[list.Count - 1].level;
            var fragmentsPerLevel = GetLevelFragments(levelMax, TargetFragments);

            int maxCount = 0;
            var listCount = list.Count;

            var levelCount = new int[levelMax + 1];
            foreach (var cutMesh in list)
            {
                levelCount[cutMesh.level]++;
            }

            for (int i = 0; i < listCount; i++)
            {
                var cutMesh = list[i];

                var curLevelRatio = levelMax + 1 - cutMesh.level;

                var fragments = (int)((curLevelRatio * fragmentsPerLevel) / levelCount[cutMesh.level]);

                cutMesh.fragments = fragments;

                maxCount += fragments;

                list[i] = cutMesh;

                if (maxCount >= TargetFragments)
                {
                    cutMesh.fragments -= maxCount - TargetFragments;
                    maxCount -= maxCount - TargetFragments;

                    list[i] = cutMesh;

                    break;
                }
            }

//        foreach (var cutMesh in list)
//        {
//            UnityEngine.Debug.Log(cutMesh.level + " " + cutMesh.distance + " " + cutMesh.fragments);
//        }

            return list;
        }

        void Update()
        {
            long cuttingTime = 0;

            switch (state)
            {
                case State.None:
                    break;

                case State.Preprocess:
                    {
                        timer.Reset();
                        timer.Start();
#if DBG
                var watch = new Stopwatch();
                watch.Start();
#endif

                        // get mesh data from game object in radius
                        var readyToCut = Preprocess();

#if DBG
                watch.Stop();
                preProcessingTime = watch.ElapsedMilliseconds;
                processingFrames = 0;
#endif

                        // nothing to explode
                        if (!readyToCut)
                        {
#if DBG
                    Exploder2DUtils.Log("Nothing to explode "  + mainCentroid);
#endif
                            OnExplosionFinished(false);
                        }
                        else
                        {
                            // continue to cutter, don't wait to another frame
                            state = State.ProcessCutter;
                            goto case State.ProcessCutter;
                        }
                    }
                    break;

                case State.ProcessCutter:
                    {
#if DBG
                processingFrames++;
                var watch = new Stopwatch();
                watch.Start();
#endif

                        // process main cutter
                        var cuttingFinished = false;

                        switch (CuttingStrategy)
                        {
                            case CutStrategy.Randomized:
                                cuttingFinished = ProcessCutterRandomized(out cuttingTime);
                                break;

//                            case CutStrategy.Grid:
//                                cuttingFinished = ProcessCutterGrid(out cuttingTime);
//                                break;

                            default: Exploder2DUtils.Assert(false, "Invalid cutting strategy");
                                break;
                        }

#if DBG
                watch.Stop();
                processingTime += watch.ElapsedMilliseconds;
#endif

                        if (cuttingFinished)
                        {
                            poolIdx = 0;
                            postList = new List<CutMesh>(meshSet);

                            // continue to next state if the cutter is finished
                            if (splitMeshIslands)
                            {
#if DBG
                        isolatingIslandsFrames = 0;
                        isolatingIslandsTime = 0.0f;
#endif
                                islands = new List<CutMesh>(meshSet.Count);

                                state = State.IsolateMeshIslands;
                                goto case State.IsolateMeshIslands;
                            }

                            state = State.PostprocessInit;
                            goto case State.PostprocessInit;
                        }
                    }
                    break;

                case State.IsolateMeshIslands:
                    {
#if DBG
                var watchPost = new Stopwatch();
                watchPost.Start();
#endif

                        var isolatingFinished = IsolateMeshIslands(ref cuttingTime);

#if DBG
                watchPost.Stop();
                isolatingIslandsFrames++;
                isolatingIslandsTime += watchPost.ElapsedMilliseconds;
#endif

                        if (isolatingFinished)
                        {
                            // goto next state
                            state = State.PostprocessInit;
                            goto case State.PostprocessInit;
                        }
                    }
                    break;

                case State.PostprocessInit:
                    {
                        InitPostprocess();

                        state = State.Postprocess;
                        goto case State.Postprocess;
                    }

                case State.Postprocess:
                    {
#if DBG
                var watchPost = new Stopwatch();
                watchPost.Start();
#endif
                        if (Postprocess(cuttingTime))
                        {
                            timer.Stop();
                        }
#if DBG
                watchPost.Stop();
                postProcessingTime += watchPost.ElapsedMilliseconds;
#endif
                    }
                    break;
            }
        }

        bool Preprocess()
        {
            //        Exploder.Profiler.Start("Preprocess - GetMeshList");

            // find meshes around exploder centroid
            var meshList = GetMeshList();

            //        Exploder.Profiler.End("Preprocess - GetMeshList");

            // nothing do destroy
            if (meshList.Count == 0)
            {
                return false;
            }

            //        Exploder.Profiler.Start("Preprocess - allocation");

            newFragments.Clear();
            meshToRemove.Clear();
            meshToCut.Clear();
            meshSet = new HashSet<CutMesh>(meshList);

            splitMeshIslands = SplitMeshIslands;

            var levelMax = meshList[meshList.Count - 1].level;
            levelCount = new int[levelMax + 1];

            //        Exploder.Profiler.End("Preprocess - allocation");

            foreach (var meshCut in meshSet)
            {
                levelCount[meshCut.level] += meshCut.fragments;
            }

//            ExploderUtils.ClearLog();
//            foreach (var cutMesh in meshList)
//            {
//                UnityEngine.Debug.Log(cutMesh.level);
//            }

            //
            // apply uniform distribution of fragments
            //
            if (UniformFragmentDistribution)
            {
                var buff = new int[64];
                foreach (var cutMesh in meshSet)
                {
                    buff[cutMesh.level] += 1;
                }

                var fragmentPerObject = TargetFragments / meshSet.Count;

                foreach (var cutMesh in meshSet)
                {
                    levelCount[cutMesh.level] = fragmentPerObject * buff[cutMesh.level];
                }
            }

            return true;
        }

        bool IsolateMeshIslands(ref long timeOffset)
        {
            var timer = new Stopwatch();
            timer.Start();

            var count = postList.Count;

            while (poolIdx < count)
            {
                var mesh = postList[poolIdx];
                poolIdx++;

                var islandsFound = false;

                if (SplitMeshIslands || (mesh.option && mesh.option.SplitMeshIslands))
                {
                    var meshIslands = MeshUtils.IsolateMeshIslands(mesh.spriteMesh);

                    if (meshIslands != null)
                    {
                        islandsFound = true;

                        foreach (var meshIsland in meshIslands)
                        {
                            islands.Add(new CutMesh
                            {
                                spriteMesh = meshIsland.mesh,
                                centroidLocal = meshIsland.centroidLocal,

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
                    }
                }

                if (!islandsFound)
                {
                    islands.Add(mesh);
                }

                if (timer.ElapsedMilliseconds + timeOffset > FrameBudget)
                {
                    return false;
                }
            }

#if DBG
        Exploder2DUtils.Log("Replacing fragments: " + postList.Count + " by islands: " + islands.Count);
#endif

            // replace postList by island list
            postList = islands;

            return true;
        }

        void InitPostprocess()
        {
            var fragmentsNum = postList.Count;

            FragmentPool2D.Instance.Allocate(fragmentsNum);
            FragmentPool2D.Instance.SetDeactivateOptions(DeactivateOptions, FadeoutOptions, DeactivateTimeout);
            FragmentPool2D.Instance.SetExplodableFragments(ExplodeFragments, DontUseTag);
            FragmentPool2D.Instance.SetFragmentPhysicsOptions(FragmentOptions);
            FragmentPool2D.Instance.SetSFXOptions(SFXOptions);

            poolIdx = 0;
            pool = FragmentPool2D.Instance.GetAvailableFragments(fragmentsNum);

            if (ExplosionCallback != null)
            {
                ExplosionCallback(timer.ElapsedMilliseconds, ExplosionState.ExplosionStarted);
            }

            // run sfx
            if (SFXOptions.ExplosionSoundClip)
            {
                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                audioSource.PlayOneShot(SFXOptions.ExplosionSoundClip);
            }

#if DBG
        postProcessingFrames = 0;
#endif
        }

        void PostCrackExplode(OnExplosion callback)
        {
            if (callback != null)
            {
                callback(0.0f, ExplosionState.ExplosionStarted);
            }

            var count = postList.Count;
            poolIdx = 0;

            while (poolIdx < count)
            {
                var fragment = pool[poolIdx];
                var mesh = postList[poolIdx];

                poolIdx++;

                if (mesh.original != gameObject)
                {
                    Exploder2DUtils.SetActiveRecursively(mesh.original, false);
                }
                else
                {
                    Exploder2DUtils.EnableCollider(mesh.original, false);
                    Exploder2DUtils.SetVisible(mesh.original, false);
                }

                fragment.Explode();
            }

            if (DestroyOriginalObject)
            {
                foreach (var mesh in postList)
                {
                    if (mesh.original && !mesh.original.GetComponent<Fragment2D>())
                    {
                        Object.Destroy(mesh.original);
                    }
                }
            }

            if (ExplodeSelf)
            {
                if (!DestroyOriginalObject)
                {
                    Exploder2DUtils.SetActiveRecursively(gameObject, false);
                }
            }

            if (HideSelf)
            {
                Exploder2DUtils.SetActiveRecursively(gameObject, false);
            }

#if DBG
        Exploder2DUtils.Log("Crack finished! " + postList.Count + postList[0].original.transform.gameObject.name);
#endif
            ExplosionCallback = callback;
            OnExplosionFinished(true);
        }

        bool Postprocess(long timeOffset)
        {
            var postTimer = new Stopwatch();
            postTimer.Start();

            var count = postList.Count;

#if DBG
        postProcessingFrames++;
#endif

            while (poolIdx < count)
            {
                var fragment = pool[poolIdx];
                var mesh = postList[poolIdx];

                poolIdx++;

                if (!mesh.original)
                {
                    continue;
                }

                if (crack)
                {
                    Exploder2DUtils.SetActiveRecursively(fragment.gameObject, false);
                }

                fragment.CreateSprite(mesh.spriteMesh, mesh.sprite, mesh.original.transform, mesh.sortingLayer, mesh.orderInLayer, mesh.color);

                var oldParent = fragment.transform.parent;
                fragment.transform.parent = mesh.parent;
                fragment.transform.position = new Vector3(mesh.position.x, mesh.position.y, mesh.original.transform.position.z);
                fragment.transform.rotation = mesh.rotation;
                fragment.transform.localScale = mesh.localScale;
                fragment.transform.parent = null;
                fragment.transform.parent = oldParent;

                if (!crack)
                {
                    if (mesh.original != gameObject)
                    {
                        Exploder2DUtils.SetActiveRecursively(mesh.original, false);
                    }
                    else
                    {
                        Exploder2DUtils.EnableCollider(mesh.original, false);
                        Exploder2DUtils.SetVisible(mesh.original, false);
                    }
                }

                if (!FragmentOptions.DisableColliders)
                {
                    Core.MeshUtils.GeneratePolygonCollider(fragment.polygonCollider2D, mesh.spriteMesh);
                }

                if (mesh.option)
                {
                    mesh.option.DuplicateSettings(fragment.options);
                }

                if (!crack)
                {
                    fragment.Explode();
                }

                var force = Force;
                if (mesh.option && mesh.option.UseLocalForce)
                {
                    force = mesh.option.Force;
                }

                // apply force to rigid body
                fragment.ApplyExplosion2D(mesh.transform, mesh.centroidLocal, mainCentroid, FragmentOptions, UseForceVector,
                                        ForceVector, force, mesh.original, TargetFragments);

#if SHOW_DEBUG_LINES
            UnityEngine.Debug.DrawLine(mainCentroid, forceVector * Force, Color.yellow, 3);
#endif

                if (postTimer.ElapsedMilliseconds + timeOffset > FrameBudget)
                {
                    return false;
                }
            }

#if DBG
        var watch = new Stopwatch();
        watch.Start();
#endif

            if (!crack)
            {
                if (DestroyOriginalObject)
                {
                    foreach (var mesh in postList)
                    {
                        if (mesh.original && !mesh.original.GetComponent<Fragment2D>())
                        {
                            Object.Destroy(mesh.original);
                        }
                    }
                }

                if (ExplodeSelf)
                {
                    if (!DestroyOriginalObject)
                    {
                        Exploder2DUtils.SetActiveRecursively(gameObject, false);
                    }
                }

                if (HideSelf)
                {
                    Exploder2DUtils.SetActiveRecursively(gameObject, false);
                }

#if DBG
            Exploder2DUtils.Log("Explosion finished! " + postList.Count + postList[0].original.transform.gameObject.name);
#endif
                OnExplosionFinished(true);
            }
            else
            {
                cracked = true;

                if (CrackedCallback != null)
                {
                    CrackedCallback();
                }
            }

#if DBG
        postProcessingTimeEnd = watch.ElapsedMilliseconds;
#endif

            return true;
        }

        void OnExplosionFinished(bool success)
        {
            if (ExplosionCallback != null)
            {
                if (!success)
                {
                    ExplosionCallback(timer.ElapsedMilliseconds, ExplosionState.ExplosionStarted);
                    OnExplosionStarted();
                }

                ExplosionCallback(timer.ElapsedMilliseconds, ExplosionState.ExplosionFinished);
            }

            state = State.None;

            queue.OnExplosionFinished(explosionID);
        }

        void OnExplosionStarted()
        {

        }

#if DBG
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 50, 300, 30), "Explosion time: " + timer.ElapsedMilliseconds + " [ms]");

        GUI.Label(new Rect(10, 80, 500, 30), "Preprocessing time: " + preProcessingTime + " [ms]");
        GUI.Label(new Rect(10, 100, 500, 30), "Processing time AVG: " + processingTime / processingFrames + " [ms]" + " frames: " + processingFrames);
        GUI.Label(new Rect(10, 120, 500, 30), "Isolating islands: " + isolatingIslandsTime / isolatingIslandsFrames + " [ms]" + " frames: " + isolatingIslandsFrames);
        GUI.Label(new Rect(10, 140, 500, 30), "Postprocessing time: " + postProcessingTime / postProcessingFrames + " [ms] " + " postFrames: " + postProcessingFrames);
        GUI.Label(new Rect(10, 160, 500, 30), "Postprocessing time end: " + postProcessingTimeEnd + " [ms] ");
        GUI.Label(new Rect(10, 180, 500, 30), "Cuts: " + cuts);

#if PROFILING
        var results = Exploder.Profiler.PrintResults();
        var y = 180;

        foreach (var result in results)
        {
            GUI.Label(new Rect(10, y, 500, 30), "Profiler: " + result);
            y += 20;
        }
#endif

    }
#endif
    }
}
