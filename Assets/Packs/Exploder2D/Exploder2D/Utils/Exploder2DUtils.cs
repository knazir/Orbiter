// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Exploder2D
{
    public static class Exploder2DUtils
    {
        /// <summary>
        /// just assert ...
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, string message="")
        {
            if (!condition)
            {
                UnityEngine.Debug.LogError("Assert! " + message);
                UnityEngine.Debug.Break();
            }
        }

        /// <summary>
        /// just warning ...
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Warning(bool condition, string message)
        {
            if (!condition)
            {
                UnityEngine.Debug.LogWarning("Warning! " + message);
            }
        }

        /// <summary>
        /// unity log
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// get centroid of the object (based on sprite bounds)
        /// </summary>
        public static Vector2 GetCentroid(GameObject obj)
        {
            var sprite = obj.GetComponent<SpriteRenderer>();

            if (sprite && sprite.sprite)
            {
                Vector2 centroid = Vector2.zero;
                var verts = sprite.sprite.vertices;

                for (int i = 0; i < verts.Length; i++)
                {
                    centroid += verts[i];
                }

                return obj.transform.TransformPoint(centroid / verts.Length);
            }

            return obj.transform.position;
        }

        /// <summary>
        /// set this object visible to render
        /// </summary>
        public static void SetVisible(GameObject obj, bool status)
        {
            if (obj)
            {
                var renderers = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in renderers)
                {
                    meshRenderer.enabled = status;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// clear console
        /// </summary>
        public static void ClearLog()
        {
            Assembly assembly = Assembly.GetAssembly(typeof (UnityEditor.SceneView));

            Type type = assembly.GetType("UnityEditorInternal.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
#else
        public static void ClearLog() {}
#endif

        /// <summary>
        /// unity version specific isActive (to suppress warnings)
        /// </summary>
        public static bool IsActive(GameObject obj)
        {
            return obj && obj.activeSelf;
        }

        /// <summary>
        /// unity version specific SetActive (to suppress warnings)
        /// </summary>
        public static void SetActive(GameObject obj, bool status)
        {
            if (obj)
            {
                obj.SetActive(status);
            }
        }

        /// <summary>
        /// unity version specific SetActiveRecursively (to suppress warnings)
        /// </summary>
        public static void SetActiveRecursively(GameObject obj, bool status)
        {
            if (obj)
            {
                var childCount = obj.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    SetActiveRecursively(obj.transform.GetChild(i).gameObject, status);
                }
                obj.SetActive(status);
            }
        }

        /// <summary>
        /// enable colliders in object hiearchy
        /// </summary>
        public static void EnableCollider(GameObject obj, bool status)
        {
            if (obj)
            {
                var colliders = obj.GetComponentsInChildren<Collider>();

                foreach (var collider in colliders)
                {
                    collider.enabled = status;
                }
            }
        }

        /// <summary>
        /// returns true if the obj is valid explodable object
        /// </summary>
        public static bool IsExplodable(GameObject obj)
        {
            var explodable = obj.GetComponent<Explodable2D>() != null;

            if (!explodable)
            {
                explodable = obj.CompareTag(Exploder2DObject.Tag);
            }

            return explodable;
        }
    }
}
