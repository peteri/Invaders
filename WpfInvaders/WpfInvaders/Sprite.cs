namespace WpfInvaders
{
    internal class Sprite
    {
        internal readonly byte[,,,] data;
        internal readonly int width;
        internal bool Visible;
        internal int X;
        internal int Y;
        internal int Image;

        internal Sprite(byte[] spriteImages, int images)
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

        internal void Draw(int line, byte[] lineData)
        {
            if ((line >= X) && (line < X + width))
            {
                int x = line - X;
                int y = Y >> 3;
                byte c1 = data[Image, Y & 0x7, x, 0];
                byte c2 = data[Image, Y & 0x7, x, 1];
                lineData[y] |= c1;
                lineData[y + 1] |= c2;
            }
        }

        internal bool Collided()
        {
            if (!Visible)
                return false;

            for (int i = 0; i < width; i++)
            {
                int line = X + i;
                int myX = line - X;
                int xOffs = line & 0x07;
                int cellOffs = (Y >> 3) + (line >> 3) * LineRender.ScreenWidth;

                // Did we collide with the stuff on screen?
                byte myC1 = data[Image, Y & 0x7, myX, 0];
                byte b = LineRender.Screen[cellOffs];
                if (b < 0x20)
                    b = LineRender.BitmapChar[b * 8 + xOffs];
                else
                    b = CharacterRom.Characters[b * 8 + xOffs];
                if ((byte)(b & myC1) != 0) return true;

                // Not yet skip up a cell.
                byte myC2 = data[Image, Y & 0x7, myX, 1];
                cellOffs += 1;
                b = LineRender.Screen[cellOffs];
                if (b < 0x20)
                    b = LineRender.BitmapChar[b * 8 + xOffs];
                else
                    b = CharacterRom.Characters[b * 8 + xOffs];
                if ((byte)(b & myC2) != 0) return true;

                // Okay what about the other sprites?
                foreach (var sprite in LineRender.Sprites)
                {
                    // Not us
                    if (sprite == this)
                        continue;
                    // Got to be visible
                    if (sprite.Visible == false)
                        continue;
                    // Better be on the same lines as us
                    if ((line < sprite.X) || (line >= sprite.X + sprite.width))
                        continue;
                    // And the overlap our Y position.
                    if ((Y < sprite.Y) || (Y >= sprite.Y + 8))
                        continue;
                    // Potentially in the box, so do we overlap?
                    if ((sprite.data[sprite.Image, sprite.Y & 0x7, line - sprite.X, 0] & myC1) != 0)
                        return true;
                    if ((sprite.data[sprite.Image, sprite.Y & 0x7, line - sprite.X, 1] & myC2) != 0)
                        return true;
                }
            }
            return false;
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
                b = LineRender.Screen[cellOffs + 1];
                if (b < 0x20)
                    LineRender.BitmapChar[b * 8 + xOffs] &= (byte)(~c2);
                xOffs++;
                if (xOffs == 8)
                {
                    xOffs = 0;
                    cellOffs += LineRender.ScreenWidth;
                };
            }
        }
    }
}
