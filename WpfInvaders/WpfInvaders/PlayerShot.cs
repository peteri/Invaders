using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class PlayerShot : TimerObject
    {
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        public enum ShotStatus {Available };

        public ShotStatus Status;
        public PlayerShot(MainWindow mainWindow, GameData gameData) : base(false, 0)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
        }

        public override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
