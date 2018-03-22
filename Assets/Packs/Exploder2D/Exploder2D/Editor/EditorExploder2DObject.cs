// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using Exploder2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (Exploder2DObject)), CanEditMultipleObjects]
public class EditorExploder2DObject : UnityEditor.Editor
{
    SerializedProperty radius;
    SerializedProperty force;
    SerializedProperty targetFragments;
    SerializedProperty frameBudget;
    SerializedProperty useForceVector;
    SerializedProperty forceVector;
    SerializedProperty ignoreTag;
    SerializedProperty explodeSelf;
    SerializedProperty disableRadiusScan;
    SerializedProperty hideSelf;
    SerializedProperty deleteOriginalObjects;
    SerializedProperty uniformDistribution;
    SerializedProperty splitMeshIslands;
    SerializedProperty disableQueue;
    SerializedProperty cuttingStragegy;
    SerializedProperty deactivateOn;
    SerializedProperty deactivateTimeout;
    SerializedProperty fadeout;
    SerializedProperty explodableFragments;
    SerializedProperty poolSize;
    SerializedProperty layer;
    SerializedProperty sortingLayer;
    SerializedProperty orderInLayer;
    SerializedProperty maxVelocity;
    SerializedProperty inheritParentPhysics;
    SerializedProperty mass;
    SerializedProperty gravityScale;
    SerializedProperty disableColliders;
    SerializedProperty angularVelocity;
    SerializedProperty randomAngularVelocity;
    SerializedProperty angularVelocityVector;
    SerializedProperty explosionSound;
    SerializedProperty fragmentHitSound;
    SerializedProperty fragmentHitSoundTimeout;
    SerializedProperty fragmentParticles;
    SerializedProperty fragmentParticlesMax;
    SerializedProperty inheritSortingLayer;
    SerializedProperty inheritOrderInLayer;
    SerializedProperty inheritSpriteRendererColor;
    SerializedProperty spriteRendererColor;

    private void OnEnable()
    {
        radius = serializedObject.FindProperty("Radius");
        force = serializedObject.FindProperty("Force");
        targetFragments = serializedObject.FindProperty("TargetFragments");
        frameBudget = serializedObject.FindProperty("FrameBudget");
        useForceVector = serializedObject.FindProperty("UseForceVector");
        forceVector = serializedObject.FindProperty("ForceVector");
        ignoreTag = serializedObject.FindProperty("DontUseTag");
        explodeSelf = serializedObject.FindProperty("ExplodeSelf");
        disableRadiusScan = serializedObject.FindProperty("DisableRadiusScan");
        hideSelf = serializedObject.FindProperty("HideSelf");
        deleteOriginalObjects = serializedObject.FindProperty("DestroyOriginalObject");
        uniformDistribution = serializedObject.FindProperty("UniformFragmentDistribution");
        splitMeshIslands = serializedObject.FindProperty("SplitMeshIslands");
        disableQueue = serializedObject.FindProperty("DisableQueue");
        cuttingStragegy = serializedObject.FindProperty("CuttingStrategy");
        deactivateOn = serializedObject.FindProperty("DeactivateOptions");
        deactivateTimeout = serializedObject.FindProperty("DeactivateTimeout");
        fadeout = serializedObject.FindProperty("FadeoutOptions");
        explodableFragments = serializedObject.FindProperty("ExplodeFragments");
        poolSize = serializedObject.FindProperty("FragmentPoolSize");
        layer = serializedObject.FindProperty("FragmentOptions.Layer");
        sortingLayer = serializedObject.FindProperty("FragmentOptions.SortingLayer");
        inheritSortingLayer = serializedObject.FindProperty("FragmentOptions.InheritSortingLayer");
        orderInLayer = serializedObject.FindProperty("FragmentOptions.OrderInLayer");
        inheritOrderInLayer = serializedObject.FindProperty("FragmentOptions.InheritOrderInLayer");
        inheritSpriteRendererColor = serializedObject.FindProperty("FragmentOptions.InheritSpriteRendererColor");
        maxVelocity = serializedObject.FindProperty("FragmentOptions.MaxVelocity");
        inheritParentPhysics = serializedObject.FindProperty("FragmentOptions.InheritParentPhysicsProperty");
        mass = serializedObject.FindProperty("FragmentOptions.Mass");
        gravityScale = serializedObject.FindProperty("FragmentOptions.GravityScale");
        disableColliders = serializedObject.FindProperty("FragmentOptions.DisableColliders");
        angularVelocity = serializedObject.FindProperty("FragmentOptions.AngularVelocity");
        randomAngularVelocity = serializedObject.FindProperty("FragmentOptions.RandomAngularVelocityVector");
        angularVelocityVector = serializedObject.FindProperty("FragmentOptions.AngularVelocityVector");
        explosionSound = serializedObject.FindProperty("SFXOptions.ExplosionSoundClip");
        fragmentHitSound = serializedObject.FindProperty("SFXOptions.FragmentSoundClip");
        fragmentHitSoundTimeout = serializedObject.FindProperty("SFXOptions.HitSoundTimeout");
        fragmentParticles = serializedObject.FindProperty("SFXOptions.FragmentEmitter");
        fragmentParticlesMax = serializedObject.FindProperty("SFXOptions.EmitersMax");
        spriteRendererColor = serializedObject.FindProperty("FragmentOptions.SpriteRendererColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var exploder = this.target as Exploder2DObject;

        if (exploder)
        {
            var change = false;

            EditorExploder2DUtils.Separator("Main Settings", 20);
            EditorGUILayout.Space();

            GUI.enabled = !(exploder.ExplodeSelf && exploder.DisableRadiusScan);
            change |= EditorExploder2DUtils.SliderEdit("Radius", "Explosion radius.", 0.0f, 100, radius);
            GUI.enabled = true;

            change |= EditorExploder2DUtils.SliderEdit("Force", "Force of explosion.", 0.0f, 100, force);
            change |= EditorExploder2DUtils.SliderEdit("Target Fragments", "Number of target fragments.", 0, 500, targetFragments);
            change |= EditorExploder2DUtils.SliderEdit("Frame Budget [ms]", "Time budget in [ms] for processing explosion calculation in one frame.", 0.0f, 60.0f, frameBudget);
            change |= EditorExploder2DUtils.Toggle("Use Force Vector", "Use this vector as a direction of explosion.", useForceVector);

            if (exploder.UseForceVector)
            {
                change |= EditorExploder2DUtils.Vector2("", "Use this vector as a direction of explosion.", forceVector);
            }

            change |= EditorExploder2DUtils.Toggle("Ignore Tag", "Ignore Exploder tag on object, use Explodable component instead.", ignoreTag);

            change |= EditorExploder2DUtils.Toggle("Explode self", "Explode this game object.", explodeSelf);
//            else
//            {
//                change |= EditorExploder2DUtils.Toggle2("Explode self", "Explode this game object.", "", "Disable radius",
//                                                      explodeSelf, disableRadiusScan);
//            }

            if (exploder.ExplodeSelf)
            {
                change |= EditorExploder2DUtils.Toggle("Disable radius", "Disable scanning for objects in radius.", disableRadiusScan);
            }

            change |= EditorExploder2DUtils.Toggle("Hide self", "Hide this game object after explosion.", hideSelf);
            change |= EditorExploder2DUtils.Toggle("Delete original object", "Delete original game object after explosion.", deleteOriginalObjects);
            change |= EditorExploder2DUtils.Toggle("Uniform distribution", "Uniformly distribute fragments inside the radius.", uniformDistribution);
            change |= EditorExploder2DUtils.Toggle("Split mesh islands", "Split non-connecting part of the mesh into separate fragments.", splitMeshIslands);
            change |= EditorExploder2DUtils.Toggle("Disable queue", "Allow only one explosion at time.", disableQueue);
            EditorExploder2DUtils.EnumSelection("Cutting strategy", "Change shape of the target fragments.",
                exploder.CuttingStrategy, cuttingStragegy, ref change);

            EditorGUILayout.Space();
            EditorExploder2DUtils.Separator("Deactivation options", 20);
            EditorGUILayout.Space();

            EditorExploder2DUtils.EnumSelection("Deactivate on", "Options for fragment deactivation.", exploder.DeactivateOptions, deactivateOn, ref change);

            if (exploder.DeactivateOptions == DeactivateOptions.Timeout)
            {
                change |= EditorExploder2DUtils.SliderEdit("Deactivate Timeout [s]", "Time in [s] to deactivate fragments.", 0.0f, 60.0f, deactivateTimeout);
                EditorExploder2DUtils.EnumSelection("FadeOut", "Option for fragments to fadeout during deactivation timeout.", exploder.FadeoutOptions, fadeout, ref change);
            }

            EditorGUILayout.Space();
            EditorExploder2DUtils.Separator("Fragment options", 20);
            EditorGUILayout.Space();

            change |= EditorExploder2DUtils.Toggle("Explodable fragments", "Enable fragments to be exploded again.", explodableFragments);
            change |= EditorExploder2DUtils.SliderEdit("Pool Size", "Size of the fragment pool, this value should be higher than TargetFragments.", 0, 500, poolSize);

            change |= EditorExploder2DUtils.String("Layer", "Layer of the fragment game object.", layer);

            change |= EditorExploder2DUtils.Toggle("Inherit SortingLayer", "Use Sorting Layer from parent sprite.", inheritSortingLayer);

            if (!inheritSortingLayer.boolValue)
            {
                change |= EditorExploder2DUtils.String("SortingLayer", "Layer of the fragment sprite.", sortingLayer);
            }

            change |= EditorExploder2DUtils.Toggle("Inherit OrderInLayer", "Use Order In Layer from parent sprite.", inheritOrderInLayer);

            if (!inheritOrderInLayer.boolValue)
            {
                change |= EditorExploder2DUtils.IntEdit("OrderInLayer", "Order in the sprite layer.", -1000, 1000, orderInLayer);
            }

            change |= EditorExploder2DUtils.Toggle("Inherit SpriteRendererColor", "Use color from parent sprite renderer.", inheritSpriteRendererColor);

            if (!inheritSpriteRendererColor.boolValue)
            {
                change |= EditorExploder2DUtils.ColorSelection("Sprite Renderer Color", spriteRendererColor);
            }

            change |= EditorExploder2DUtils.SliderEdit("MaxVelocity", "Maximal velocity that fragment can have.", 0.0f, 100.0f, maxVelocity);
            change |= EditorExploder2DUtils.Toggle("Inherit parent physics", "Use the same physics settings as in original game object.", inheritParentPhysics);
            change |= EditorExploder2DUtils.SliderEdit("Mass", "Mass property of every fragment.", 0.0f, 100.0f, mass);
            change |= EditorExploder2DUtils.SliderEdit("Gravity scale", "Apply gravity to fragment.", 0.0f, 1.0f, gravityScale);

            change |= EditorExploder2DUtils.Toggle("Disable colliders", "Disable colliders of all fragments.", disableColliders);

            change |= EditorExploder2DUtils.SliderEdit("Angular velocity", "Angular velocity of fragments.", 0.0f, 100.0f, angularVelocity);
            change |= EditorExploder2DUtils.Toggle("Random angular vector", "Randomize rotation of fragments.", randomAngularVelocity);

            if (!exploder.FragmentOptions.RandomAngularVelocityVector)
            {
                change |= EditorExploder2DUtils.Vector2("", "Use this vector as a angular velocity vector.", angularVelocityVector);
            }

            EditorGUILayout.Space();
            EditorExploder2DUtils.Separator("SFX options", 20);
            EditorGUILayout.Space();

            change |= EditorExploder2DUtils.ObjectSelection<AudioClip>("Explosion sound", "Sound effect played on explosion.", explosionSound);
            change |= EditorExploder2DUtils.ObjectSelection<AudioClip>("Fragment hit sound", "Sound effect when the fragment hits another collider (wall, floor).", fragmentHitSound);

            if (exploder.SFXOptions.FragmentSoundClip)
            {
                change |= EditorExploder2DUtils.SliderEdit("Hit sound timeout", "Timeout between sound effects.",
                                                         0.0f, 1.0f, fragmentHitSoundTimeout);
            }

            change |= EditorExploder2DUtils.ObjectSelection<GameObject>("Fragment particles", "Particle effect that will start to emit from each fragment after explosion.", fragmentParticles);

            if (exploder.SFXOptions.FragmentEmitter)
            {
                change |= EditorExploder2DUtils.IntEdit("Maximum emitters", "Maximumal number of emmiters.", 0, 1000,
                                                      fragmentParticlesMax);
            }

            if (change)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(exploder);
            }

            EditorGUILayout.Separator();
        }
    }
}
