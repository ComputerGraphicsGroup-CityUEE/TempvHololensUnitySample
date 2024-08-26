using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace q_common
{
    public static class q_common
    {
        public static string ReadLine(BinaryReader reader)
        {
            char nextChar;
            string line = "";

            while ((nextChar = reader.ReadChar()) != '\n')
            {
                line += nextChar;
            }

            return line;
        }
        public static float ReverseBytes(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
        public static Vector3 Unitize(Vector3 vertex, Vector3 center, Vector3 max, Vector3 min)
        {
            Vector3 distance = max - min;
            float scale = 2 / Mathf.Max(distance.x, Mathf.Max(distance.y, distance.z));

            vertex -= center;
            vertex *= scale;

            return vertex;
        }
        public static void GetBounds(List<Vector3> vertex, ref Vector3 max, ref Vector3 min, ref Vector3 center)
        {
            max = vertex[0];
            min = vertex[0];

            // Find the max and min
            for (int i = 1; i < vertex.Count; i++)
            {
                max.x = Mathf.Max(vertex[i].x, max.x);
                max.y = Mathf.Max(vertex[i].y, max.y);
                max.z = Mathf.Max(vertex[i].z, max.z);

                min.x = Mathf.Min(vertex[i].x, min.x);
                min.y = Mathf.Min(vertex[i].y, min.y);
                min.z = Mathf.Min(vertex[i].z, min.z);
            }

            //Debug.Log(max + "," + min);
            center = (max + min) / 2.0f;

            Vector3 distance = max - min;
            float scale = 2 / Mathf.Max(distance.x, Mathf.Max(distance.y, distance.z));
        }

        public static Vector3[] FilpZAxis(Vector3[] src)
        {
            Vector3[] des = new Vector3[src.Length];
            for (int i = 0; i < src.Length; i++)
                des[i] = new Vector3(src[i].x, src[i].y, -src[i].z);
            return des;
        }

        public static List<Vector3> FilpZAxis(List<Vector3> src)
        {
            List<Vector3> des = new List<Vector3>(src);
            for (int i = 0; i < src.Count; i++)
                des[i] = new Vector3(src[i].x, src[i].y, -src[i].z);
            return des;
        }

        public static Vector3[] GetFaceNormal(Vector3[] m_vPos, int[] m_vtIdx, int m_nTriangle)
        {
            List<Vector3> m_tn = new();
            for (int i = 0; i < m_nTriangle; i++)
            {
                m_tn.Add(Vector3.Cross(m_vPos[m_vtIdx[i * 3 + 1]] - m_vPos[m_vtIdx[i * 3 + 0]],
                                       m_vPos[m_vtIdx[i * 3 + 2]] - m_vPos[m_vtIdx[i * 3 + 1]]).normalized);
            }

            return m_tn.ToArray();
        }
    }
}
