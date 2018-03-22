using UnityEngine;

namespace Exploder2D
{
    /// <summary>
    /// simple script for enabling slow motion physics
    /// </summary>
    public class Exploder2DSlowMotion : MonoBehaviour
    {
        public float slowMotionTime = 1.0f;
        public Exploder2DObject Exploder2D;

        private float slowMotionSpeed = 1.0f;
        private bool slowmo;

        /// <summary>
        /// slow motion mode ... just slow time
        /// this mode is using mesh colliders so use it wisely!
        /// </summary>
        /// <param name="status"></param>
        public void EnableSlowMotion(bool status)
        {
            slowmo = status;

            if (slowmo)
            {
                slowMotionSpeed = 0.05f;
            }
            else
            {
                slowMotionSpeed = 1.0f;
            }

            slowMotionTime = slowMotionSpeed;
        }

        public void Update()
        {
            slowMotionSpeed = slowMotionTime;
            Time.timeScale = slowMotionSpeed;
            Time.fixedDeltaTime = slowMotionSpeed*0.02f;

            if (Input.GetKeyDown(KeyCode.T))
            {
                slowmo = !slowmo;
                EnableSlowMotion(slowmo);
            }
        }
    }
}
