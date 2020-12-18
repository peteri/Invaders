using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class AlienPlungerShot : AlienShot
    {
        private static byte[] PlungerShotSprite = { 0x04, 0xfc, 0x4, 0x10, 0xfc, 0x10, 0x20, 0xfc, 0x20, 0x80, 0xfc, 0x80 };
        public AlienPlungerShot(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, PlungerShotSprite)
        {
        }
    }
}
