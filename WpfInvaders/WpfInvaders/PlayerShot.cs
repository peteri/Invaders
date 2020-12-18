using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class PlayerShot : TimerObject
    {
        private static byte[] shotSprite = { 0x0f };
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        private readonly Sprite playerShot;
        public enum ShotStatus { Available, Initiated, NormalMove, HitSomething, AlienExploding, AlienExploded };

        public ShotStatus Status;
        public PlayerShot(MainWindow mainWindow, GameData gameData) : base(true, 0)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            playerShot = new Sprite(shotSprite, 1);
            LineRender.AddSprite(playerShot);
        }

        public override void Action()
        {
            switch (Status)
            {
                case ShotStatus.Available:
                    playerShot.Visible = false;
                    break;
                case ShotStatus.Initiated:
                    playerShot.Visible = true;
                    playerShot.Y = 0x28;
                    playerShot.X = gameData.PlayerBase.PlayerX + 8;
                    mainWindow.StopIsr();
                    Status = ShotStatus.NormalMove;
                    break;
                case ShotStatus.NormalMove:
                    playerShot.Y += 4;
                    break;
            }
        }
    }
}
