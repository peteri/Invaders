using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class PlayerShot : TimerObject
    {
        private static byte[] shotSprite = { 0x0f };
        private static byte[] shotExplodeSprite = { 0x99, 0x3C, 0x7E, 0x3D, 0xBC, 0x3E, 0x7C, 0x99 };
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        public readonly Sprite ShotSprite;
        public readonly Sprite ShotExplodeSprite;
        private int explosionTimer=0x10;

        public enum ShotStatus { Available, Initiated, NormalMove, HitSomething, AlienExploding, AlienExploded };

        public ShotStatus Status;
        public PlayerShot(MainWindow mainWindow, GameData gameData) : base(true, 0)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            ShotSprite = new Sprite(shotSprite, 1);
            LineRender.AddSprite(ShotSprite);
            ShotExplodeSprite = new Sprite(shotExplodeSprite, 1);
            LineRender.AddSprite(ShotExplodeSprite);
        }

        public override void Action()
        {
            switch (Status)
            {
                case ShotStatus.Available:
                    ShotSprite.Visible = false;
                    break;
                case ShotStatus.Initiated:
                    ShotSprite.Visible = true;
                    ShotSprite.Y = 0x28;
                    ShotSprite.X = gameData.PlayerBase.PlayerX + 8;
                    mainWindow.StopIsr();
                    Status = ShotStatus.NormalMove;
                    break;
                case ShotStatus.NormalMove:
                    ShotSprite.Y += 4;
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
                        ShotExplodeSprite.X = ShotSprite.X-2;
                        ShotExplodeSprite.Y = ShotSprite.Y-3;
                        ShotExplodeSprite.Visible = true;
                    }
                    break;
                case ShotStatus.AlienExploding:
                    break;
                default:
                    EndBlowUp();
                    break;
            }
        }

        private void EndBlowUp()
        {
            if (Status == ShotStatus.HitSomething)
            {
                // Do battle damage to any shields+
                ShotSprite.BattleDamage();
                Status = ShotStatus.Available;

            }
            explosionTimer = 0x10;
            gameData.IncremeentSaucerScoreAndShotCount();
        }
    }
}
