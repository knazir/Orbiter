using UnityEngine;
using System.Collections;

namespace Exploder2D.Core
{
    public class SpriteMesh
    {
        public int[] triangles;
        public Vector2[] vertices;
        public Vector2 min, max;

        public ushort[] uTriangles 
        {
            get
            {
                ushort[] uTris = new ushort[triangles.Length];
                for (int i=0; i<triangles.Length; i++)
                {
                    uTris[i] = (ushort)triangles[i];
                }

                return uTris;
            }
        }

        public SpriteMesh()
        {
        }

        public SpriteMesh(UnityEngine.Sprite sprite)
        {
            min.x = float.MaxValue;
            min.y = float.MaxValue;
            max.x = float.MinValue;
            max.y = float.MinValue;

            var sTris = sprite.triangles;
            triangles = new int[sTris.Length];
            for (int i = 0; i < sTris.Length; i++)
            {
                triangles[i] = sTris[i];
            }

            vertices = sprite.vertices;

            for (var i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x > max.x)
                {
                    max.x = vertices[i].x;
                }

                if (vertices[i].y > max.y)
                {
                    max.y = vertices[i].y;
                }

                if (vertices[i].x < min.x)
                {
                    min.x = vertices[i].x;
                }

                if (vertices[i].y < min.y)
                {
                    min.y = vertices[i].y;
                }
            }
        }

        public Vector2 GetCentroidLocal()
        {
            Vector2 centroid = Vector2.zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                centroid += vertices[i];
            }

            return centroid / vertices.Length;
        }
    }
}
