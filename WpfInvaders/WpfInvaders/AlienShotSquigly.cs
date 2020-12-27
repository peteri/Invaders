using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    internal class AlienShotSquigly : AlienShot
    {
        private static readonly byte[] SquiglyShotSprite = { 0x44, 0xaa, 0x10, 0x88, 0x54, 0x22, 0x10, 0xaa, 0x44, 0x22, 0x54, 0x88 };
        private static readonly byte[] shotColumns = { 0x0B, 0x01, 0x06, 0x03, 0x01, 0x01, 0x0B, 0x09, 0x02, 0x08, 0x02, 0x0B, 0x04, 0x07, 0x0A };
        private int currentShotColumn;
        internal AlienShotSquigly(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, SquiglyShotSprite)
        {
            ResetShotData();
        }

        internal override void Action()
        {
            if (gameData.ShotSync != 2)
                return;
            bool resetRequired = HandleAlienShot(gameData.AlienShotPlunger, gameData.AlienShotRolling);
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
        }
    }
}
