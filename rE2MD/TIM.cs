using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace rE2MD
{
    class TIM
    {
        public static Bitmap load(string fileName)
        {
            FileStream data = new FileStream(fileName, FileMode.Open);
            BinaryReader input = new BinaryReader(data);

            uint magic = input.ReadUInt32() & 0xff;
            if (magic != 0x10) throw new Exception("Não é um arquivo TIM!");
            uint format = input.ReadUInt32();
            uint imageOffset = input.ReadUInt32();

            switch (format)
            {
                case 9:
                    ushort palOrgX = input.ReadUInt16();
                    ushort palOrgY = input.ReadUInt16();
                    input.ReadUInt16();
                    ushort palCount = input.ReadUInt16();

                    Color[,] CLUT = new Color[palCount, 256];
                    for (int i = 0; i < palCount; i++)
                    {
                        for (int j = 0; j < 256; j++)
                        {
                            ushort val = input.ReadUInt16();
                            byte b = (byte)((val >> 10) & 0x1f);
                            byte g = (byte)((val >> 5) & 0x1f);
                            byte r = (byte)((val) & 0x1f);

                            r = (byte)((r << 3) | (r >> 2));
                            g = (byte)((g << 3) | (g >> 2));
                            b = (byte)((b << 3) | (b >> 2));

                            CLUT[i, j] = Color.FromArgb(r, g, b);
                        }
                    }

                    data.Seek(8, SeekOrigin.Current);
                    int width = input.ReadUInt16() * 2;
                    int height = input.ReadUInt16();

                    Bitmap img = new Bitmap(width, height);
                    for (int y = 0; y < img.Height; y++)
                    {
                        for (int x = 0; x < img.Width; x++)
                        {
                            img.SetPixel(x, y, CLUT[x / 128, input.ReadByte()]);
                        }
                    }
                    return img;

                default: throw new Exception(String.Format("Formato {0} TIM não implementado!", format.ToString()));
            }
        }
    }
}
