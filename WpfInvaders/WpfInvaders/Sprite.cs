using System;

namespace WpfInvaders
{
    public class Sprite
    {
        public bool Visible;
        ushort[,,] data;
        public int X;
        public int Y;
        public int Image;
        int width;
        byte collided;

        public Sprite(byte[] data, int images)
        {
            Visible = false;
            Image = 0;
            width = data.Length / images;
            this.data = new ushort[images, 8, width];
            for (int i = 0; i < images; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        ushort d = data[i * width + k];
                        this.data[i, j, k] = (ushort)(d << j);
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
                byte c1 = SpriteData.BitFlip((byte)(data[Image, Y & 0x7, x] & 0xff));
                byte c2 = SpriteData.BitFlip((byte)(data[Image, Y & 0x7, x] >> 8));
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

        }
    }
}
