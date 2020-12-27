using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    internal class AlienShotPlunger : AlienShot
    {
        private static readonly byte[] PlungerShotSprite = { 0x04, 0xfc, 0x4, 0x10, 0xfc, 0x10, 0x20, 0xfc, 0x20, 0x80, 0xfc, 0x80 };
        private static readonly byte[] shotColumns = { 0x01, 0x07, 0x01, 0x01, 0x01, 0x04, 0x0B, 0x01, 0x06, 0x03, 0x01, 0x01, 0x0B, 0x09, 0x02, 0x08 };
        private int currentShotColumn;
        
        internal AlienShotPlunger(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, PlungerShotSprite)
        {
            ResetShotData();
        }

        internal override void Action()
        {
            if (gameData.PlungerShotActive == false)
                return;
            if (gameData.ShotSync != 1)
                return;
            bool resetRequired = HandleAlienShot(gameData.AlienShotSquigly, gameData.AlienShotRolling);
            if (resetRequired)
                ResetShotData();

        }

        protected override int ShotColumn()
        {
            int shotColumn = shotColumns[currentShotColumn];
            currentShotColumn++;
            if (currentShotColumn > shotColumns.Length)
                currentShotColumn = 0;
            return shotColumn;
        }

        protected override void ResetShotData()
        {
            base.ResetShotData();
            currentShotColumn = 0; 
            mainWindow.CurrentPlayer.CountAliens();
            if (mainWindow.CurrentPlayer.NumAliens == 1)
            {
                gameData.PlungerShotActive = false;
            }
        }
    }
}
