using UnityEngine;

namespace Exploder2D.Examples
{
    /// <summary>
    /// example how to explode every explodable (tagged) objects in the scene at once
    /// </summary>
    public class ExplodeAllObjects : MonoBehaviour
    {
        private GameObject[] DestroyableObjects;

//        private int counter;
//        private int counterFinished;

        private void Start()
        {
            DestroyableObjects = GameObject.FindGameObjectsWithTag("Exploder2D");
        }

        private void Update()
        {
            // press enter to start explosions
            if (Input.GetKeyDown(KeyCode.Return))
            {
                foreach (var o in DestroyableObjects)
                {
                    ExplodeObject(o);
                }
            }
        }

        private void ExplodeObject(GameObject gameObject)
        {
            var exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;

            exploder.transform.position = Exploder2DUtils.GetCentroid(gameObject);
            exploder.Radius = 1.0f;
            exploder.Explode();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(200, 10, 300, 30), "Hit enter to explode everything!");
        }
    }
}
