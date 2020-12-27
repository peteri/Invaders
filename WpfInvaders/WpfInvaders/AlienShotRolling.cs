using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    internal class AlienShotRolling : AlienShot
    {
        private static readonly byte[] RollingShotSprite = { 0x00, 0xfe, 0x00, 0x24, 0xfe, 0x12, 0x00, 0xfe, 0x00, 0x48, 0xfe, 0x90 };
        private bool fireShot;
        internal AlienShotRolling(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, RollingShotSprite)
        {
            ResetShotData();
        }

        internal override void Action()
        {
            ExtraCount = 2;

            if (!fireShot)
            {
                fireShot = true;
            }
            else
            {
                if (HandleAlienShot(gameData.AlienShotPlunger, gameData.AlienShotSquigly))
                {
                    ResetShotData();
                }
            }
        }

        // Fire a shot at the player.
        protected override int ShotColumn()
        {
            int col = mainWindow.FindColumn(gameData.PlayerBase.PlayerX + 8);
            if (col == -1) return 1;
            if (col == 11) return 11;
            return col + 1;
        }

        protected override void ResetShotData()
        {
            base.ResetShotData();
            ExtraCount = 2;
            fireShot = false;
        }
    }
}
