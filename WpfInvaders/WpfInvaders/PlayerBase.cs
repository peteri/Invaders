using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class PlayerBase : TimerObject
    {
        private enum Command { None, Right, Left }
        // 1=right, 2=left
        private static readonly Command[] demoCmds =
        {
            Command.Right,
            Command.Right,
            Command.None,
            Command.None,
            Command.Right,
            Command.None,
            Command.Left,
            Command.Right,
            Command.None,
            Command.Left,
            Command.Right,
            Command.None
        };

        public enum PlayerAlive { Alive, BlowUpOne, BlowUpTwo }
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        private static int demoCommand = 0;
        public int PlayerX;
        public int PlayerY;
        public PlayerAlive Alive { get; set; }

        public PlayerBase(MainWindow mainWindow, GameData gameData) : base(true, 128)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            Alive = PlayerAlive.Alive;
            PlayerX = 0x10;
            PlayerY = 0x20;
        }

        public override void Action()
        {
            int spriteBase = 0xb8;
            if (Alive != PlayerAlive.Alive)
            {
                // Make the player die...
                throw new NotImplementedException();
            }
            else
            {
                Command command = 0;
                gameData.PlayerOk = true;
                // Enable alien shots after 30 ticks....
                if (!gameData.AlienShotsEnabled)
                {
                    gameData.AlienFireDelay--;
                    if (gameData.AlienFireDelay == 0)
                        gameData.AlienShotsEnabled = true;
                }

                // In demo mode decide which direction to go.
                // We always fire a shot, when it explodes we change direction.
                if (gameData.DemoMode)
                {
                    command = demoCmds[demoCommand];
                }
                else
                {
                    if ((mainWindow.switchState & MainWindow.SwitchState.Right) != 0)
                        command = Command.Right;
                    else if ((mainWindow.switchState & MainWindow.SwitchState.Left) != 0)
                        command = Command.Left;
                }

                // Move the player
                switch (command)
                {
                    case Command.Left: if (PlayerX > 0x10) PlayerX--; break;
                    case Command.Right: if (PlayerX < 0xb9) PlayerX++; break;
                }
            }
            int screenX = PlayerX >> 3;
            int screenY = PlayerY >> 3; // Constant really
            int screenOffset = screenX * LineRender.ScreenWidth + screenY;
            int playerSprite = spriteBase + ((PlayerX & 0x07) * 3);
            for (int i = 0; i < 3; i++)
            {
                LineRender.Screen[screenOffset] = (byte)(playerSprite + i);
                screenOffset += LineRender.ScreenWidth;
            }
        }

        internal void IncrementDemoCommand()
        {
            demoCommand++;
            if (demoCommand >= demoCmds.Length)
                demoCommand = 0;
        }
    }
}
