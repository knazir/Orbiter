// to crack and explode use this macro
// crack by left mouse button, explode after by right mouse button
//#define ENABLE_CRACK_AND_EXPLODE
//#define TEST_SCENE_LOAD

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Exploder2D.Demo
{
    public class DemoClickExplode2D : MonoBehaviour
    {
        private Exploder2DObject exploder;
        public Camera Camera;

        void Start()
        {
            exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;
        }

        bool IsExplodable(GameObject obj)
        {
            if (exploder.DontUseTag)
            {
                return obj.GetComponent<Explodable2D>() != null;
            }
            else
            {
                return obj.CompareTag(Exploder2DObject.Tag);
            }
        }

        void Update()
        {
            // we hit the mouse button
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                //var hit = Physics2D.Raycast(, Vector2.zero);
                var hit = Physics2D.GetRayIntersection(Camera.ScreenPointToRay(Input.mousePosition));

                // we hit the object
                if (hit)
                {
                    var obj = hit.collider.gameObject;

                    // explode this object!
                    if (IsExplodable(obj))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            ExplodeObject(obj);
                        }
                        else
                        {
                            ExplodeAfterCrack();
                        }
                    }
                }
            }
        }

        void ExplodeObject(GameObject obj)
        {
            // activate exploder
            Exploder2DUtils.SetActive(exploder.gameObject, true);

            // move exploder object to the same position
            exploder.transform.position = Exploder2DUtils.GetCentroid(obj);

            // decrease the radius so the exploder is not interfering other objects
            exploder.Radius = 0.1f;

            // DONE!
#if ENABLE_CRACK_AND_EXPLODE
            exploder.Crack(OnCracked);
#else
            exploder.Explode(OnExplosion);

#endif
        }

        void OnExplosion(float time, Exploder2DObject.ExplosionState state)
        {
            if (state == Exploder2DObject.ExplosionState.ExplosionFinished)
            {
                //Utils.Log("Exploded");
            }
        }

        void OnCracked()
        {
            //Utils.Log("Cracked");
        }

        void ExplodeAfterCrack()
        {
#if ENABLE_CRACK_AND_EXPLODE
            exploder.ExplodeCracked(OnExplosion);
#endif
        }

        private void OnGUI()
        {
#if TEST_SCENE_LOAD
        if (GUI.Button(new Rect(10, 50, 100, 30), "NextScene"))
        {
            UnityEngine.Application.LoadLevel(1);
        }
#endif
        }
    }
}
