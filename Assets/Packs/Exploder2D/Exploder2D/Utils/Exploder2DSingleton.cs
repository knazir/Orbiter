// Version 1.0.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace Exploder2D.Utils
{
    /// <summary>
    /// utility class for easy accessing single exploder object in the scene
    /// assign this class to exploder game object
    /// </summary>
    public class Exploder2DSingleton : MonoBehaviour
    {
        /// <summary>
        /// instance of the exploder object
        /// </summary>
        public static Exploder2DObject Exploder2DInstance;

        void Awake()
        {
            Exploder2DInstance = gameObject.GetComponent<Exploder2DObject>();
        }
    }
}
