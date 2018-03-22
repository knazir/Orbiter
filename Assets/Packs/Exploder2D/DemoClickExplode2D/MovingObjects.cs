using UnityEngine;
using System.Collections.Generic;
using Exploder2D.Utils;

namespace Exploder2D.Demo
{
    public class MovingObjects : MonoBehaviour
    {
        public List<GameObject> rows;

        public float Speed = 10.0f;
        public float yStart = 22.5f;
        public float yLimit = 0.3f;

        private void Start()
        {
            sprites = new List<GameObject>();

            foreach (var row in rows)
            {
                var list = row.GetComponentsInChildren<SpriteRenderer>();

                foreach (var spriteRenderer in list)
                {
                    sprites.Add(spriteRenderer.gameObject);
                }
            }
        }

        private void Update()
        {
            foreach (var row in rows)
            {
                row.transform.position -= new Vector3(0.0f, Speed*Time.deltaTime, 0.0f);

                if (row.transform.position.y < yLimit)
                {
                    row.transform.position = new Vector3(row.transform.position.x, yStart);

//                    var sprites = row.GetComponentsInChildren<SpriteRenderer>(true);
//
//                    foreach (var sprite in sprites)
//                    {
//                        Exploder2DUtils.SetActiveRecursively(sprite.gameObject, true);
//                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExplodeList();
            }
        }

        private List<GameObject> sprites;
        private int index;

        void ExplodeList()
        {
            if (index >= sprites.Count)
            {
                return;
            }

            var exploder = Exploder2DSingleton.Exploder2DInstance;

            // move exploder object to the same position
            exploder.transform.position = Exploder2DUtils.GetCentroid(sprites[index]);

            // decrease the radius so the exploder is not interfering other objects
            exploder.Radius = 1.0f;

            exploder.Explode(OnExplosion);
        }

        public void OnExplosion(float timeMS, Exploder2DObject.ExplosionState state)
        {
            if (state == Exploder2DObject.ExplosionState.ExplosionFinished)
            {
                index++;
                ExplodeList();
            }
        }
    }
}

