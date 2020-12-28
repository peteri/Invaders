using System.Collections.Generic;

namespace WpfInvaders
{
    internal static class LineRender
    {
        internal const int ScreenWidth = 32;
        internal const int ScreenHeight = 28;
        internal static byte[] Screen = new byte[ScreenWidth * ScreenHeight];
        internal static byte[] BitmapChar = new byte[32 * 8];
        internal static List<Sprite> Sprites = new List<Sprite>();

        internal static byte[] RenderLine(int line)
        {
            var returnData = new byte[ScreenWidth];
            int startOfLine = (line / 8) * ScreenWidth;
            int cellLine = line & 0x07;

            for (int i = 0; i < ScreenWidth; i++)
            {
                byte c = Screen[startOfLine++];
                int index = c * 8 + cellLine;
                byte data = (c < 32) ? BitmapChar[index] : CharacterRom.Characters[index];
                returnData[i] = data;
            }
            foreach (var sprite in Sprites)
                if (sprite.Visible)
                    sprite.Draw(line, returnData);
            return returnData;
        }
    }
}
