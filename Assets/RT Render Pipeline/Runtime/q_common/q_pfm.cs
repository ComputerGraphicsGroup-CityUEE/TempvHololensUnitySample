using System;
using System.IO;
using UnityEngine;
using static q_common.q_common;
using System.Text;

namespace q_common
{
    public class q_pfm
    {
        int w;
        int h;

        float[] colorStreamRGBA;


        public float[] ColorStreamRGBA => colorStreamRGBA;
        public int W => w;
        public int H => h;

        public q_pfm() 
        {
            w = 0;
            h = 0;
        }
        public q_pfm(int width, int height, float[] m_arr)
        {
            w = width;
            h = height;
            colorStreamRGBA = m_arr;
        }
        public q_pfm(string filePath)
        {
            w = 0;
            h = 0;
            Load(filePath);
        }

        public void flip_horizontal()
        {
            for (int y = h-1; y > 0; y--) // Invert the image vertically
            {
                for (int x = 0; x < w/2; x++)
                {
                    float a, b, c, d;
                    a = colorStreamRGBA[ y*w*4 + x*4 + 0 ];
                    b = colorStreamRGBA[ y*w*4 + x*4 + 1 ];
                    c = colorStreamRGBA[ y*w*4 + x*4 + 2 ];
                    d = colorStreamRGBA[ y*w*4 + x*4 + 3 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 0 ] = colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 0 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 1 ] = colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 1 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 2 ] = colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 2 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 3 ] = colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 3 ];
                    colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 0 ] = a;
                    colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 1 ] = b;
                    colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 2 ] = c;
                    colorStreamRGBA[ y*w*4 + (w-1-x)*4 + 3 ] = d;
                }
            }
        }

        public void flip_vertical()
        {
            for (int y =0; y < h/2; y++) // Invert the image vertically
            {
                for (int x = 0; x < w; x++)
                {
                    float a, b, c, d;
                    a = colorStreamRGBA[ y*w*4 + x*4 + 0 ];
                    b = colorStreamRGBA[ y*w*4 + x*4 + 1 ];
                    c = colorStreamRGBA[ y*w*4 + x*4 + 2 ];
                    d = colorStreamRGBA[ y*w*4 + x*4 + 3 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 0 ] = colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 0 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 1 ] = colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 1 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 2 ] = colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 2 ];
                    colorStreamRGBA[ y*w*4 + x*4 + 3 ] = colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 3 ];
                    colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 0 ] = a;
                    colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 1 ] = b;
                    colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 2 ] = c;
                    colorStreamRGBA[ (h-1-y)*w*4 + x*4 + 3 ] = d;
                }
            }
        }

        public void rotate()
        {
            for (int y = 0; y < h; y++) // Invert the image vertically
            {
                for (int x = y; x < w; x++)
                {
                    float a, b, c, d;
                    a = colorStreamRGBA[y * w * 4 + x * 4 + 0];
                    b = colorStreamRGBA[y * w * 4 + x * 4 + 1];
                    c = colorStreamRGBA[y * w * 4 + x * 4 + 2];
                    d = colorStreamRGBA[y * w * 4 + x * 4 + 3];
                    colorStreamRGBA[y * w * 4 + x * 4 + 0] = colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 0];
                    colorStreamRGBA[y * w * 4 + x * 4 + 1] = colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 1];
                    colorStreamRGBA[y * w * 4 + x * 4 + 2] = colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 2];
                    colorStreamRGBA[y * w * 4 + x * 4 + 3] = colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 3];
                    colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 0] = a;
                    colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 1] = b;
                    colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 2] = c;
                    colorStreamRGBA[(h - 1 - y) * w * 4 + x * 4 + 3] = d;
                }
            }
        }

        public void transpose()
        {
            q_pfm mq = new q_pfm(w, h, colorStreamRGBA);
            Load(h, w);

            int i, j;
            for (j = 0; j < h; j++)
                for (i = 0; i < w; i++)
                {
                    colorStreamRGBA[j * w * 4 + i * 4 + 0] = mq.colorStreamRGBA[i * h * 4 + j * 4 + 0];
                    colorStreamRGBA[j * w * 4 + i * 4 + 1] = mq.colorStreamRGBA[i * h * 4 + j * 4 + 1];
                    colorStreamRGBA[j * w * 4 + i * 4 + 2] = mq.colorStreamRGBA[i * h * 4 + j * 4 + 2];
                    colorStreamRGBA[j * w * 4 + i * 4 + 3] = mq.colorStreamRGBA[i * h * 4 + j * 4 + 3];
                }
        }
        public void Load(int width, int height)
        {
            w = width;
            h = height;
            colorStreamRGBA = new float[w * h * 4];
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
                            colorStreamRGBA[ y*w*4 + x*4 + 3 ] = 1;
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
                    string str = $"PF\n{(int)w} {(int)h}\n-1.000000\n";

                    byte[] bytes = Encoding.ASCII.GetBytes(str);
                    writer.Write(bytes);

                    //for (int y = 0; y < h; y++) // Invert the image vertically
                    for (int y = h - 1; y > 0; y--)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            writer.Write( colorStreamRGBA[ y*w*4 + x*4 + 0 ] );
                            writer.Write( colorStreamRGBA[ y*w*4 + x*4 + 1 ] );
                            writer.Write( colorStreamRGBA[ y*w*4 + x*4 + 2 ] );
                        }
                    }
                }
                Debug.Log("The pfm is successfully saved to local: " + filePath);
            }
        }

    }
}
