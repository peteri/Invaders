using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace WpfInvaders
{
    public static class LineRender
    {
        public const int ScreenWidth = 32;
        public const int ScreenHeight = 28;
        public static byte[] Screen = new byte[ScreenWidth * ScreenHeight];
        public static byte[] BitmapChar = new byte[32 * 8];

        public static byte[] RenderLine(int line)
        {
            var returnData = new byte[ScreenWidth];
            int startOfLine = (line / 8) * ScreenWidth;
            int cellLine = line & 0x07;

            for (int i = 0; i < ScreenWidth; i++)
            {
                byte c = Screen[startOfLine++];
                int index = c * 8 + cellLine;
                byte data = (c < 32) ? BitmapChar[index] : SpriteData.Characters[index];
                if ((line==8)||(line == 214) && (data == 0))
                    returnData[i] = 0x55;
                else
                    returnData[i] = data;
             }
            return returnData;
        }
    }
}
