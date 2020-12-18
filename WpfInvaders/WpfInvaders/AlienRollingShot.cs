using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class AlienRollingShot : AlienShot
    {
        private static byte[] RollingShotSprite = { 0x00, 0xfe, 0x00, 0x24, 0xfe, 0x12, 0x00, 0xfe, 0x00, 0x48, 0xfe, 0x90 };

        public AlienRollingShot(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, RollingShotSprite)
        {
        }
    }
}
