﻿using System;

namespace WpfInvaders
{
    internal class PlayerBase : TimerObject
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
            Command.Left
        };

        private static readonly int[] PlayerBaseCharacters = { 0x56, 0x5c, 0x65, 0x6e, 0x77, 0xc0, 0xd0, 0xe0 };

        internal enum PlayerAlive { Alive, BlowUpOne, BlowUpTwo }
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        private static int demoCommand = 0;
        private int blowUpCounter;
        private int blowUpChanges;
        internal int PlayerX;
        internal int PlayerY;
        internal PlayerAlive Alive { get; set; }

        internal PlayerBase(MainWindow mainWindow, GameData gameData) : base(true, 128)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            Alive = PlayerAlive.Alive;
            PlayerX = 0x10;
            PlayerY = 0x20;
            blowUpCounter = 5;
            blowUpChanges = 0x0c;
        }

        private void ResetState()
        {
            Alive = PlayerAlive.Alive;
            PlayerX = 0x10;
            PlayerY = 0x20;
            blowUpCounter = 5;
            blowUpChanges = 0x0c;
            Ticks = 128;
        }

        internal override void Action()
        {
            if (Alive != PlayerAlive.Alive)
            {
                blowUpCounter--;
                if (blowUpCounter != 0)
                    return;
                gameData.PlayerOk = false;
                gameData.AlienShotsEnabled = false;
                gameData.AlienFireDelay = 0x30;
                blowUpCounter = 5;
                blowUpChanges--;
                if (blowUpChanges == 0)
                {
                    DrawPlayerSprite(0, true);
                    ResetState();

                    //if (gameData.Invaded)
                    //    return;
                    if (gameData.GameMode == false)
                        return;
                    mainWindow.PlayerShipBlownUp();
                }
                else
                {
                    int explodingSprite = PlayerBaseCharacters[PlayerX & 0x07];
                    int playerSize = (PlayerX & 0x07) == 0 ? 2 : 3;
                    if (Alive == PlayerAlive.BlowUpOne)
                    {
                        explodingSprite += playerSize * 2;
                        Alive = PlayerAlive.BlowUpTwo;
                    }
                    else
                    {
                        explodingSprite += playerSize;
                        Alive = PlayerAlive.BlowUpOne;
                    }
                    DrawPlayerSprite(explodingSprite,false);
                }
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
                DrawPlayerSprite(PlayerBaseCharacters[PlayerX & 0x07], false);
            }
        }

        private void DrawPlayerSprite(int playerSprite, bool erase)
        {
            int screenX = PlayerX >> 3;
            int screenY = PlayerY >> 3; // Constant really
            int screenOffset = screenX * LineRender.ScreenWidth + screenY;
            int count = (PlayerX & 0x07) == 0 ? 2 : 3;
            for (int i = 0; i < count; i++)
            {
                LineRender.Screen[screenOffset] = erase ? (byte)0x23 : (byte)(playerSprite + i);
                screenOffset += LineRender.ScreenWidth;
            }
        }

        internal static void IncrementDemoCommand()
        {
            demoCommand++;
            if (demoCommand >= demoCmds.Length)
                demoCommand = 0;
            System.Diagnostics.Debug.Print("DemoCommand is {0} DemoCommandIndex is {1} {2:x4}", demoCmds[demoCommand], demoCommand,demoCommand+0x1f74);
        }
    }
}
