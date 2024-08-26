using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace q_common
{
    public static class q_IO
    {
        public static bool CheckFileExists(string filePath, string dataType = ".txt")
        {
            if (File.Exists(filePath + dataType))
            {
                //Debug.Log("File found at: " + filePath + dataType);
                return true;
            }
            else
            {
                //Debug.Log("File not found at: " + filePath + dataType);
                return false;
            }
        }

        public static void SaveVector3ArrayToFile(Vector3[] vector3Array, string fileName)
        {
            string data = "";
            foreach (Vector3 vector3 in vector3Array)
                data += vector3.x + "\t" + vector3.y + "\t" + vector3.z + "\n";
            // data += vector3.x.ToString("0.000000").PadRight(10) + "\t" + vector3.y.ToString("0.000000").PadRight(10) + "\t" + vector3.z.ToString("0.000000").PadRight(10) + "\n";
            // data += vector3.x.ToString("0.000000").PadRight(10) + "," + vector3.y.ToString("0.000000").PadRight(10) + "," + vector3.z.ToString("0.000000").PadRight(10) + "\n";
            string filePath = fileName + ".txt";

            File.WriteAllText(filePath, data);
            Debug.Log("Vector3 array saved to: " + filePath);
        }

        public static void SaveVector4ArrayToFile(Vector4[] vector4Array, string fileName)
        {
            string data = "";
            foreach (Vector4 vector4 in vector4Array)
                data += vector4.x + "\t" + vector4.y + "\t" + vector4.z + "\t" + vector4.w + "\n";
            //data += vector4.x.ToString("0.000000").PadRight(10) + "\t" + vector4.y.ToString("0.000000").PadRight(10) + "\t" + vector4.z.ToString("0.000000").PadRight(10) + "\t" + vector4.w.ToString("0.000000").PadRight(10) + "\n";
            //data += vector4.x.ToString("0.000000").PadRight(10) + "," + vector4.y.ToString("0.000000").PadRight(10) + "," + vector4.z.ToString("0.000000").PadRight(10) + "," + vector4.w.ToString("0.000000").PadRight(10) + "\n";
            string filePath = fileName + ".txt";

            File.WriteAllText(filePath, data);
            Debug.Log("Vector4 array saved to: " + filePath);
        }

        public static void SaveIntegerArrayToFile(int[] intArray, string fileName)
        {
            if (fileName == null)
            {
                Debug.Log("intArray is null");
                return;
            }

            string data = "";
            foreach (int perInt in intArray)
                data += perInt + "\n";
            string filePath = fileName + ".txt";


            if (filePath == null || filePath.Equals(' '))
            {
                Debug.Log("filePath is null");
                return;
            }

            File.WriteAllText(filePath, data);
            Debug.Log("Int array saved to: " + filePath);
        }

        public static void SaveFloatArrayToFile(float[] floatArray, string fileName)
        {
            if (fileName == null)
            {
                Debug.Log("floatArr is null");
                return;
            }

            string data = "";
            foreach (float perF in floatArray)
                data += perF + "\n";
            string filePath = fileName + ".txt";


            if (filePath == null || filePath.Equals(' '))
            {
                Debug.Log("filePath is null");
                return;
            }

            File.WriteAllText(filePath, data);
            Debug.Log("float array saved to: " + filePath);
        }

        unsafe public static Vector3[] ConvertPointerToArray(Vector3* vector3Pointer, int length)
        {
            Vector3[] vector3Array = new Vector3[length];

            for (int i = 0; i < length; i++)
                vector3Array[i] = *(vector3Pointer + i);

            return vector3Array;
        }

        unsafe public static int[] ConvertPointerToArray(int* ptr, int length)
        {
            int[] intArray = new int[length];

            for (int i = 0; i < length; i++)
                intArray[i] = *(ptr + i); // Copy value from pointer to array

            return intArray;
        }

        unsafe public static float[] ConvertPointerToArray(float* ptr, int length)
        {
            float[] floatArray = new float[length];

            for (int i = 0; i < length; i++)
                floatArray[i] = *(ptr + i); // Copy value from pointer to array

            return floatArray;
        }

        unsafe public static Vector4[] ConvertPointerToArray(Vector4* vector4Pointer, int length)
        {
            Vector4[] vector4Array = new Vector4[length];

            for (int i = 0; i < length; i++)
                vector4Array[i] = *(vector4Pointer + i);

            return vector4Array;
        }

        public static List<Vector3> LoadVector3ArrayFromFile(string fileName)
        {
            string filePath = fileName + ".txt";
            List<Vector3> vectorList = new List<Vector3>();
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] values = line.Split('\t');
                        if (values.Length == 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
                        {
                            Vector3 vector = new Vector3(x, y, z);
                            vectorList.Add(vector);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log("An error occurred while reading the file: " + e.Message);
            }
            return vectorList;
        }
    }
}
