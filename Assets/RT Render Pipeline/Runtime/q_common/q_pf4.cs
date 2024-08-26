using System;
using System.IO;
using UnityEngine;
using static q_common.q_common;
using System.Text;
using Unity.Mathematics;

namespace q_common
{
    public class q_pf4
    {
        int w;
        int h;

        float[] colorStreamRGBA;

        public int W => w;
        public int H => h;
        public float[] ColorStreamRGBA => colorStreamRGBA;

        public q_pf4() 
        {
            w = 0;
            h = 0;
        }

        public q_pf4(int m_w, int m_h, float[] source)
        {
            this.w = m_w;
            this.h = m_h;

            colorStreamRGBA = source;

        }

        public q_pf4(string filePath)
        {
            w = 0;
            h = 0;
            Load(filePath);
        }

        public void Load(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    // Read the file header
                    string magicNumber = new string(reader.ReadChars(2));
                    reader.ReadChar();
                    if (magicNumber != "PF" && magicNumber != "pf")
                    {
                        throw new Exception("Invalid PFM file format.");
                    }

                    string dimensionLine = ReadLine(reader);
                    string[] dimensions = dimensionLine.Trim().Split(' ');
                    w = int.Parse(dimensions[0]);
                    h = int.Parse(dimensions[1]);

                    // Check endianness
                    float scale = float.Parse(ReadLine(reader));
                    bool littleEndian = (scale < 0.0f);
                    // Calculate the absolute scale
                    scale = Math.Abs(scale);

                    // Read and parse pixel data
                    colorStreamRGBA = new float[w * h * 4];

                    for (int y = h-1; y > 0; y--) // Invert the image vertically
                    {
                        for (int x = 0; x < w; x++)
                        {
                            colorStreamRGBA[ y*w*4 + x*4 + 0 ] = littleEndian ? reader.ReadSingle() : ReverseBytes(reader.ReadSingle());
                            colorStreamRGBA[ y*w*4 + x*4 + 1 ] = littleEndian ? reader.ReadSingle() : ReverseBytes(reader.ReadSingle());
                            colorStreamRGBA[ y*w*4 + x*4 + 2 ] = littleEndian ? reader.ReadSingle() : ReverseBytes(reader.ReadSingle());
                            colorStreamRGBA[ y*w*4 + x*4 + 3 ] = littleEndian ? reader.ReadSingle() : ReverseBytes(reader.ReadSingle());
                        }
                    }

                }
            }
        }

        public void Save(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    string str = $"pf\n{(int)w} {(int)h}\n-1.000000\n";

                    byte[] bytes = Encoding.ASCII.GetBytes(str);
                    writer.Write(bytes);

                    //for (int y = 0; y < h; y++) // Invert the image vertically
                    for (int y = h - 1; y > 0; y--)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            writer.Write(colorStreamRGBA[ y*w*4 + x*4 + 0 ]);
                            writer.Write(colorStreamRGBA[ y*w*4 + x*4 + 1 ]);
                            writer.Write(colorStreamRGBA[ y*w*4 + x*4 + 2 ]);
                            writer.Write(colorStreamRGBA[ y*w*4 + x*4 + 3 ]);
                        }
                    }
                }
                Debug.Log("The pf4 is successfully saved to local: " + filePath);
            }
        }

    }
}
