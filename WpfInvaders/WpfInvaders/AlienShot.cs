namespace WpfInvaders
{
    internal abstract class AlienShot : TimerObject
    {
        protected readonly MainWindow mainWindow;
        protected readonly GameData gameData;
        private static readonly byte[] alienShotExplosion = { 0x4A, 0x15, 0xBE, 0x3F, 0x5E, 0x25 };
        internal bool ShotActive = false;
        internal bool ShotBlowingUp = false;
        protected int ShotStepCount;
        private int ShotBlowCount;
        internal Sprite Shot;
        internal Sprite ShotExplosion;
        internal int DeltaY;

        internal AlienShot(MainWindow mainWindow, GameData gameData, byte[] sprite) : base(true, 0)
        {
            Shot = new Sprite(sprite, 4);
            LineRender.Sprites.Add(Shot);
            ShotExplosion = new Sprite(alienShotExplosion, 1);
            LineRender.Sprites.Add(ShotExplosion);
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            ShotBlowCount = 4;
        }

        internal virtual void ResetShotData()
        {
            ShotBlowCount = 4;
            ShotActive = false;
            ShotBlowingUp = false;
            ShotStepCount = 0;
            Shot.Image = 0;
        }

        protected bool HandleAlienShot(AlienShot otherShot1, AlienShot otherShot2)
        {
            if (ShotActive)
            {
                MoveShot();
            }
            else
            {
                if (gameData.SplashMajorState == MainWindow.SplashMajorState.AnimateCoinExplodeFireBullet)
                {
                    ActivateShot();
                }
                else
                {
                    if (gameData.AlienShotsEnabled == true)
                    {
                        
                        ShotStepCount = 0;
                        if ((otherShot1.ShotStepCount != 0) && (otherShot1.ShotStepCount <= gameData.AlienShotReloadRate))
                            return (ShotBlowCount == 0);
                        if ((otherShot2.ShotStepCount != 0) && (otherShot2.ShotStepCount <= gameData.AlienShotReloadRate))
                            return (ShotBlowCount == 0);
                        int alienIndex = ShotColumn() - 1;
                        Shot.X = gameData.RefAlienX + alienIndex * 0x10 + 7;
                        Shot.Y = gameData.RefAlienY - 0x0a;
                        while (mainWindow.CurrentPlayer.Aliens[alienIndex] != 1)
                        {
                            alienIndex += 11;
                            Shot.Y += 0x10;
                            if (alienIndex >= 55)
                                return (ShotBlowCount == 0);
                        }
                        ActivateShot();
                    }
                }
            }
            return (ShotBlowCount == 0);
        }

        private void MoveShot()
        {
            if (((Shot.X+0x20) & 0x80) != gameData.VblankStatus)
                return;
            if (ShotBlowingUp == false)
            {
                ShotStepCount++;
                Shot.Image++;
                if (Shot.Image >= 4)
                    Shot.Image = 0;
                Shot.Y += gameData.AlienShotDeltaY;
                Shot.Visible = true;
                if (Shot.Y < 0x15)
                    ShotBlowingUp = true;
                if (Shot.Collided())
                {
                    if ((Shot.Y >= 0x1e) && (Shot.Y <= 0x27))
                        gameData.PlayerBase.Alive = PlayerBase.PlayerAlive.BlowUpOne;
                    ShotBlowingUp = true;
                }
            }
            else
            {
                ShotBlowCount--;
                if (ShotBlowCount == 3)
                {
                    Shot.Visible = false;
                    Shot.BattleDamage();
                    ShotExplosion.X = Shot.X - 2;
                    ShotExplosion.Y = Shot.Y - 2;
                    ShotExplosion.Visible = true;
                }
                if (ShotBlowCount == 0)
                {
                    ShotExplosion.BattleDamage();
                    ShotExplosion.Visible = false;
                }
            }
        }

        private void ActivateShot()
        {
            ShotActive = true;
            ShotStepCount++;
        }

        protected abstract int ShotColumn();
    }
}
