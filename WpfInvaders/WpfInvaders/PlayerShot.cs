﻿namespace WpfInvaders
{
    internal class PlayerShot : TimerObject
    {
        private static readonly byte[] shotSprite = { 0x0f };
        private static readonly byte[] shotExplodeSprite = { 0x99, 0x3C, 0x7E, 0x3D, 0xBC, 0x3E, 0x7C, 0x99 };
        private readonly GameData gameData;
        private readonly MainWindow mainWindow;
        internal readonly Sprite ShotSprite;
        internal readonly Sprite ShotExplodeSprite;
        private int explosionTimer=0x10;

        internal enum ShotStatus { Available, Initiated, NormalMove, HitSomething, AlienExploded, AlienExploding };

        internal ShotStatus Status;
        internal PlayerShot(MainWindow mainWindow, GameData gameData) : base(true, 0)
        {
            this.gameData = gameData;
            this.mainWindow = mainWindow;
            ShotSprite = new Sprite(shotSprite, 1);
            LineRender.Sprites.Add(ShotSprite);
            ShotExplodeSprite = new Sprite(shotExplodeSprite, 1);
            LineRender.Sprites.Add(ShotExplodeSprite);
        }

        internal override void Action()
        {
            if (((ShotSprite.X+0x20) & 0x80) != gameData.VblankStatus)
                return;
            switch (Status)
            {
                case ShotStatus.Available:
                    ShotSprite.Visible = false;
                    break;
                case ShotStatus.Initiated:
                    ShotSprite.Visible = true;
                    ShotSprite.Y = 0x28;
                    ShotSprite.X = gameData.PlayerBase.PlayerX + 8;
                    Status = ShotStatus.NormalMove;
                    break;
                case ShotStatus.NormalMove:
                    ShotSprite.Y += 4;
                    if (ShotSprite.Collided())
                        gameData.AlienExploding = true;
                    break;
                case ShotStatus.HitSomething:
                    explosionTimer--;
                    if (explosionTimer == 0)
                    {
                        ShotExplodeSprite.Visible = false;
                        EndBlowUp();
                    }
                    if (explosionTimer == 0x0f)
                    {
                        ShotSprite.Visible = false;
                        ShotSprite.BattleDamage();
                        ShotExplodeSprite.X = ShotSprite.X-3;
                        ShotExplodeSprite.Y = ShotSprite.Y-2;
                        ShotExplodeSprite.Visible = true;
                    }
                    break;
                case ShotStatus.AlienExploding:
                    break;
                case ShotStatus.AlienExploded:
                default:
                    EndBlowUp();
                    break;
            }
        }

        private void EndBlowUp()
        {
            if (Status == ShotStatus.HitSomething)
            {
                // Do battle damage to any shields
                // Originally would have been done by
                // erasing the sprite...
                ShotExplodeSprite.BattleDamage();
            }
            Status = ShotStatus.Available;
            ShotSprite.Y = 0x28;
            ShotSprite.X = 0x00;
            explosionTimer = 0x10;
            gameData.IncremeentSaucerScoreAndShotCount();
        }
    }
}
