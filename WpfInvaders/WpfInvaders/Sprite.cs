using System;

namespace WpfInvaders
{
    public class Sprite
    {
        public bool Visible;
        byte[,,,] data;
        public int X;
        public int Y;
        public int Image;
        int width;
        byte collided;

        public Sprite(byte[] spriteImages, int images)
        {
            Visible = false;
            Image = 0;
            width = spriteImages.Length / images;
            this.data = new byte[images, 8, width, 2];
            for (int i = 0; i < images; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        ushort d = spriteImages[i * width + k];
                        data[i, j, k, 0] = CharacterRom.BitFlip((byte)((d << j) & 0xff));
                        data[i, j, k, 1] = CharacterRom.BitFlip((byte)((d << j) >> 8));
                    }
                }
            }
        }

        public void Draw(int line, byte[] lineData)
        {
            if ((line >= X) && (line < X + width))
            {
                int x = line - X;
                int y = Y >> 3;
                byte c1 = data[Image, Y & 0x7, x, 0];
                byte c2 = data[Image, Y & 0x7, x, 1];
                collided |= (byte)(lineData[y] & c1);
                collided |= (byte)(lineData[y + 1] & c2);
                lineData[y] |= c1;
                lineData[y + 1] |= c2;
            }
        }

        public bool Collided()
        {
            if (!Visible)
                return false;
            return collided != 0;
        }

        public void ClearCollided()
        {
            collided = 0;
        }

        /// <summary>
        /// Do battle damage to any shields...
        /// </summary>
        internal void BattleDamage()
        {
            int xOffs = X & 0x07;
            int cellOffs = (Y >> 3) + (X >> 3) * LineRender.ScreenWidth;

            for (int i = 0; i < width; i++)
            {
                byte c1 = data[Image, Y & 0x7, i, 0];
                byte c2 = data[Image, Y & 0x7, i, 1];
                byte b = LineRender.Screen[cellOffs];
                if (b < 0x20)
                    LineRender.BitmapChar[b * 8 + xOffs] &= (byte)(~c1);
                b = LineRender.Screen[cellOffs+1];
                if (b < 0x20)
                    LineRender.BitmapChar[b * 8 + xOffs] &= (byte)(~c2);
                xOffs++;
                if (xOffs == 8)
                {
                    xOffs = 0;
                    cellOffs++;
                };
            }
        }
    }
}
