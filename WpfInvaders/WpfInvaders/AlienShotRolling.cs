using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class AlienShotRolling : AlienShot
    {
        private static byte[] RollingShotSprite = { 0x00, 0xfe, 0x00, 0x24, 0xfe, 0x12, 0x00, 0xfe, 0x00, 0x48, 0xfe, 0x90 };
        private bool fireShot;
        public AlienShotRolling(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, RollingShotSprite)
        {
            ResetShotData();
        }

        public override void Action()
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
            return 3;
        }

        private void ResetShotData()
        {
            ExtraCount = 2;
            ShotStepCount = 0;
            fireShot = false;
        }
    }
}
