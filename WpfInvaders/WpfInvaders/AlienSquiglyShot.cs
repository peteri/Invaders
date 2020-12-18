using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class AlienSquiglyShot : AlienShot
    {
        private static byte[] SquiglyShotSprite = { 0x44, 0xaa, 0x10, 0x88, 0x54, 0x22, 0x10, 0xaa, 0x44, 0x22, 0x54, 0x88 };

        public AlienSquiglyShot(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, SquiglyShotSprite)
        {
        }
    }
}
