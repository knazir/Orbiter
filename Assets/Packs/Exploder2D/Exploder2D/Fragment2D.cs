// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using UnityEngine;
using UnityEngine.Sprites;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Exploder2D
{
    /// <summary>
    /// options for deactivating the fragment
    /// </summary>
    public enum DeactivateOptions
    {
        /// <summary>
        /// fragments remain active until they are needed for next explosion
        /// </summary>
        Never,

        /// <summary>
        /// fragment will be deactivated if it is not visible by main camera
        /// </summary>
        OutsideOfCamera,

        /// <summary>
        /// fragment will be deactivated after timeout
        /// </summary>
        Timeout,
    }

    /// <summary>
    /// options for fadeout fragments
    /// </summary>
    public enum FadeoutOptions
    {
        /// <summary>
        /// fragments will be fully visible until deactivation
        /// </summary>
        None,

        /// <summary>
        /// fragments will fadeout on deactivateTimeout
        /// </summary>
        FadeoutAlpha,

        /// <summary>
        /// fragments will scale down to zero on deactivateTimeout
        /// </summary>
        ScaleDown,
    }

    /// <summary>
    /// script component for fragment game object
    /// the only logic here is visibility test against main camera and timeout sleeping for rigidbody
    /// </summary>
    public class Fragment2D : MonoBehaviour
    {
        /// <summary>
        /// is this fragment explodable
        /// </summary>
        public bool explodable;

        /// <summary>
        /// options for deactivating the fragment after explosion
        /// </summary>
        public DeactivateOptions deactivateOptions;

        /// <summary>
        /// deactivate timeout, valid only if DeactivateOptions == DeactivateTimeout
        /// </summary>
        public float deactivateTimeout = 10.0f;

        /// <summary>
        /// options for fading out fragments after explosion
        /// </summary>
        public FadeoutOptions fadeoutOptions = FadeoutOptions.None;

        /// <summary>
        /// maximum velocity of fragment
        /// </summary>
        public float maxVelocity = 1000;

        /// <summary>
        /// disable colliders
        /// </summary>
        public bool disableColliders = false;

        /// <summary>
        /// timeout to re-enable disabled colliders
        /// </summary>
        public float disableCollidersTimeout;

        /// <summary>
        /// flag if this fragment is visible from main camera
        /// </summary>
        public bool visible;

        /// <summary>
        /// is this fragment active
        /// </summary>
        public bool activeObj;

        /// <summary>
        /// minimum size of fragment bounding box to be explodable (if explodable flag is true)
        /// </summary>
        public float minSizeToExplode = 0.5f;

        /// <summary>
        /// optional audio source on this fragment
        /// </summary>
        public AudioSource audioSource;

        /// <summary>
        /// optional audio clip for this fragment
        /// </summary>
        public AudioClip audioClip;

        /// <summary>
        /// optional particle emitter for fragment
        /// </summary>
        private ParticleSystem[] particleSystems;

        private SpriteRenderer spriteRenderer;

        private GameObject particleChild;

        public PolygonCollider2D polygonCollider2D;

        public Rigidbody2D rigid2D;

        public bool IsSleeping()
        {
            return rigid2D.IsSleeping();
        }

        public void Sleep()
        {
            rigid2D.Sleep();
        }

        public void WakeUp()
        {
            rigid2D.WakeUp();
        }

        public void SetConstraints(RigidbodyConstraints constraints)
        {
        }

        public void SetSFX(Exploder2DObject.SFXOption sfx, bool allowParticle)
        {
            audioClip = sfx.FragmentSoundClip;

            if (audioClip && !audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (sfx.FragmentEmitter && allowParticle)
            {
                if (!particleChild)
                {
                    var dup = Instantiate(sfx.FragmentEmitter) as GameObject;

                    if (dup)
                    {
                        dup.transform.position = Vector3.zero;
                        particleChild = new GameObject("Particles");
                        particleChild.transform.parent = gameObject.transform;

                        dup.transform.parent = particleChild.transform;
                    }
                }

                if (particleChild)
                {
                    particleSystems = particleChild.GetComponentsInChildren<ParticleSystem>();
                }
            }
            else
            {
                if (particleChild)
                {
                    Destroy(particleChild);
                }
            }
        }

        void OnCollisionEnter2D()
        {
            var pool = FragmentPool2D.Instance;

            if (pool.CanPlayHitSound())
            {
                if (audioClip && audioSource)
                {
                    audioSource.PlayOneShot(audioClip);
                }
                
                pool.OnFragmentHit();
            }
        }

        public void SetFragmentPhysicsOptions(Exploder2DObject.FragmentOption opt)
        {
            if (gameObject)
            {
                gameObject.layer = LayerMask.NameToLayer(opt.Layer);

//                if (spriteRenderer)
//                {
//                    spriteRenderer.sortingLayerName = opt.SortingLayer;
//                    spriteRenderer.sortingOrder = opt.OrderInLayer;
//                }

                DisableColliders(opt.DisableColliders);
            }
        }

        public void DisableColliders(bool disable)
        {
            if (disable)
            {
                Object.Destroy(polygonCollider2D);
            }
            else
            {
                if (!polygonCollider2D)
                {
                    polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                }
            }
        }

        public void CreateSprite(Core.SpriteMesh spriteMesh, Sprite parentSprite, Transform parentTransform, string layer, int order, Color color)
        {
            var fragmentSprite = Sprite.Instantiate(parentSprite, parentTransform.position, parentTransform.rotation) as Sprite;

            var min = parentSprite.bounds.min;
            var max = parentSprite.bounds.max;

            var verts = new Vector2[spriteMesh.vertices.Length];

            for (int i = 0; i < spriteMesh.vertices.Length; i++)
            {
                var v = spriteMesh.vertices[i];
                var nx = (v.x - min.x)/(max.x - min.x);
                var ny = (v.y - min.y)/(max.y - min.y);

                nx = Mathf.Clamp01(nx);
                ny = Mathf.Clamp01(ny);

                verts[i] = new Vector2(nx * parentSprite.rect.width, ny * parentSprite.rect.height);
            }

            fragmentSprite.OverrideGeometry(verts, spriteMesh.uTriangles);
            spriteRenderer.sprite = fragmentSprite;

            spriteRenderer.sortingLayerName = layer;
            spriteRenderer.sortingOrder = order;
			spriteRenderer.color = color;
        }

        /// <summary>
        /// apply physical explosion to fragment piece (2D case)
        /// </summary>
        public void ApplyExplosion2D(Transform meshTransform, Vector2 centroid, Vector2 mainCentroid,
                              Exploder2DObject.FragmentOption fragmentOption,
                              bool useForceVector, Vector2 ForceVector, float force, GameObject original,
                              int targetFragments)
        {
            var rigid = rigid2D;

            // apply fragment mass and velocity properties
            var parentVelocity = Vector2.zero;
            var parentAngularVelocity = 0.0f;
            var mass = fragmentOption.Mass;
            var gravityScale = fragmentOption.GravityScale;

            // inherit velocity and mass from original object
            if (fragmentOption.InheritParentPhysicsProperty)
            {
                if (original && original.GetComponent<Rigidbody2D>())
                {
                    var parentRigid = original.GetComponent<Rigidbody2D>();

                    parentVelocity = parentRigid.velocity;
                    parentAngularVelocity = parentRigid.angularVelocity;
                    mass = parentRigid.mass / targetFragments;
                    gravityScale = parentRigid.gravityScale;
                }
            }

            var centroidTransformed = meshTransform.TransformPoint(centroid);
            Vector2 forceVector = (new Vector2(centroidTransformed.x, centroidTransformed.y) - mainCentroid).normalized;
            float angularVelocity = fragmentOption.AngularVelocity * (fragmentOption.RandomAngularVelocityVector ? Random.insideUnitCircle.x : fragmentOption.AngularVelocityVector.y);

            if (useForceVector)
            {
                forceVector = ForceVector;
            }

            rigid.velocity = forceVector * force + parentVelocity;
            rigid.angularVelocity = angularVelocity + parentAngularVelocity;
            rigid.mass = mass;
            maxVelocity = fragmentOption.MaxVelocity;
            rigid.gravityScale = gravityScale;
        }

        /// <summary>
        /// options component for faster access
        /// </summary>
        public Exploder2DOption options;

        /// <summary>
        /// rigidbody component for faster access
        /// </summary>
        public Rigidbody rigidBody;

        /// <summary>
        /// refresh local members components objects
        /// </summary>
        public void RefreshComponentsCache()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            options = GetComponent<Exploder2DOption>();
            rigid2D = GetComponent<Rigidbody2D>();
            polygonCollider2D = GetComponent<PolygonCollider2D>();
        }

        /// <summary>
        /// this is called from exploder class to start the explosion
        /// </summary>
        public void Explode()
        {
            activeObj = true;
            Exploder2DUtils.SetActiveRecursively(gameObject, true);
            visibilityCheckTimer = 0.1f;
            visible = true;
            deactivateTimer = deactivateTimeout;
            originalScale = transform.localScale;

            if (explodable)
            {
                tag = Exploder2DObject.Tag;
            }

            Emit(true);
        }

        public void Emit(bool centerToBound)
        {
            if (particleSystems != null)
            {
                if (centerToBound)
                {
                    if (particleChild && spriteRenderer)
                    {
                        particleChild.transform.position = spriteRenderer.bounds.center;
                    }
                }

                foreach (var psystem in particleSystems)
                {
                    psystem.Clear();
                    psystem.Play();
                }
            }
        }

        /// <summary>
        /// deactivate this fragment piece
        /// </summary>
        public void Deactivate()
        {
            Exploder2DUtils.SetActive(gameObject, false);
            visible = false;
            activeObj = false;

            // turn off particles
            if (particleSystems != null)
            {
                foreach (var psystem in particleSystems)
                {
                    psystem.Clear();
                }
            }
        }

        private Vector2 originalScale;
        private float visibilityCheckTimer;
        private float deactivateTimer;

        void Start()
        {
            visibilityCheckTimer = 1.0f;
            RefreshComponentsCache();
            visible = false;
        }

        void FixedUpdate()
        {
            
        }

        void OnDestroy()
        {
            
        }

        void Update()
        {
            if (activeObj)
            {
                //
                // clamp velocity
                //
                if (rigidBody)
                {
                    if (rigidBody.velocity.sqrMagnitude > maxVelocity * maxVelocity)
                    {
                        var vel = rigidBody.velocity.normalized;
                        rigidBody.velocity = vel * maxVelocity;
                    }
                }
                else if (rigid2D)
                {
                    if (rigid2D.velocity.sqrMagnitude > maxVelocity * maxVelocity)
                    {
                        var vel = rigid2D.velocity.normalized;
                        rigid2D.velocity = vel * maxVelocity;
                    }
                }

                if (deactivateOptions == DeactivateOptions.Timeout)
                {
                    deactivateTimer -= Time.deltaTime;

                    if (deactivateTimer < 0.0f)
                    {
                        Sleep();
                        activeObj = false;
                        Exploder2DUtils.SetActiveRecursively(gameObject, false);

                        // return fragment to previous fadout state
                        switch (fadeoutOptions)
                        {
                            case FadeoutOptions.FadeoutAlpha:
                                break;
                        }
                    }
                    else
                    {
                        var t = deactivateTimer/deactivateTimeout;

                        switch (fadeoutOptions)
                        {
                            case FadeoutOptions.FadeoutAlpha:
                                var color = spriteRenderer.color;
                                color.a = t;
                                spriteRenderer.color = color;
                                break;

                            case FadeoutOptions.ScaleDown:
                                gameObject.transform.localScale = originalScale*t;
                                break;
                        }
                    }
                }

                visibilityCheckTimer -= Time.deltaTime;

                if (visibilityCheckTimer < 0.0f && UnityEngine.Camera.main)
                {
                    var viewportPoint = UnityEngine.Camera.main.WorldToViewportPoint(transform.position);

                    if (viewportPoint.z < 0 || viewportPoint.x < 0 || viewportPoint.y < 0 ||
                        viewportPoint.x > 1 || viewportPoint.y > 1)
                    {
                        if (deactivateOptions == DeactivateOptions.OutsideOfCamera)
                        {
                            Sleep();
                            activeObj = false;
                            Exploder2DUtils.SetActiveRecursively(gameObject, false);
                        }

                        visible = false;
                    }
                    else
                    {
                        visible = true;
                    }

                    visibilityCheckTimer = Random.Range(0.1f, 0.3f);

                    if (explodable)
                    {
                        var size = GetComponent<Collider>().bounds.size;

                        if (Mathf.Max(size.x, size.y, size.z) < minSizeToExplode)
                        {
                            tag = string.Empty;
                        }
                    }
                }
            }
        }
    }
}
