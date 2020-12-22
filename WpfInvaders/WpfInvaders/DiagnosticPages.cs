using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    internal static class DiagnosticPages
    {
        internal static void ShowExplodedInvaders(MainWindow mainWindow)
        {
            mainWindow.StopIsr();
            mainWindow.ClearScreen();
            mainWindow.WriteText(31, 10, "EXPLOSIONS");
            mainWindow.WriteText(29, 10, "\x21\x22  ; 0");
            mainWindow.WriteText(28, 10, "\x23\x24  ; 2");
            mainWindow.WriteText(27, 10, "\x25\x26\x27 ; 4");
            mainWindow.WriteText(26, 10, "\x28\x29\x2a ; 6");
            mainWindow.WriteText(24, 0, "\x21\x22\x60\x61\x60\x61 \x60\x61\x21\x22\x60\x61 \x60\x61\x60\x61\x21\x22 ; 0");
            mainWindow.WriteText(22, 0, "\x23\x24\x66\x67\x66\x67 \x66\x67\x23\x24\x66\x67 \x66\x67\x66\x67\x23\x24 ; 2");
            mainWindow.WriteText(20, 0, "\x25\x26\x2b\x63\x64\x63\x65\x62\x63\x2c\x26\x2b\x63\x65\x62\x63\x64\x63\x2c\x26\x27; 4A");
            mainWindow.WriteText(18, 0, "\x25\x26\x2d\x73\x74\x73\x75\x72\x73\x2e\x26\x2d\x73\x75\x72\x73\x74\x73\x2e\x26\x27; 4B");
            mainWindow.WriteText(16, 0, "\x25\x26\x27\x83\x84\x83\x85\x82\x83\x2f\x26\x27\x83\x85\x82\x83\x84\x83\x2f\x26\x27; 4C");
            mainWindow.WriteText(14, 0, "\x28\x29\x2a\x68\x69\x68\x69 \x68\x5b\x29\x2a\x68\x69 \x68\x69\x68\x5b\x29\x2a; 6A");
            mainWindow.WriteText(12, 0, "\x28\x29\x2a\x78\x79\x78\x79 \x78\x5c\x29\x2a\x78\x79 \x78\x79\x78\x5c\x29\x2a; 6B");
            mainWindow.WriteText(10, 0, "\x28\x29\x2a\x88\x89\x88\x89 \x88\x5d\x29\x2a\x88\x89 \x88\x89\x88\x5d\x29\x2a; 6C");
            mainWindow.RenderScreen();
        }

        internal static void ShowShiftedInvaders(MainWindow mainWindow,int alienType)
        {
            mainWindow.StopIsr();
            mainWindow.ClearScreen();
            DrawShiftedAliens(alienType, 0x1f, "#R#TO#L#L#TO#R#");

            DrawShiftedAliens(alienType, 0x1d, "#010101-010101#");
            DrawShiftedAliens(alienType, 0x1b, "7890101-230101#");
            DrawShiftedAliens(alienType, 0x19, "78E8901-232301#");

            DrawShiftedAliens(alienType, 0x17, "78E8E89-232323#");
            DrawShiftedAliens(alienType, 0x15, "45D8E89-45B323#");
            DrawShiftedAliens(alienType, 0x13, "45A5D89-45A5B3#");

            DrawShiftedAliens(alienType, 0x11, "45A5A56-45A5A56");
            DrawShiftedAliens(alienType, 0x0f, "2345A56-78C5A56");
            DrawShiftedAliens(alienType, 0x0d, "2323456-78E8C56");

            DrawShiftedAliens(alienType, 0x0b, "232323#-78E8E89");
            DrawShiftedAliens(alienType, 0x09, "012323#-#0F8E89");
            DrawShiftedAliens(alienType, 0x07, "010123#-#010F89");
            DrawShiftedAliens(alienType, 0x05, "010101#-#010101");

            mainWindow.RenderScreen();
        }

        private static void DrawShiftedAliens(int alienType, int y, string text)
        {
            string RtoL = text.Substring(0, 7);
            string LtoR = text.Substring(8, 7);
            int curOffset = y;
            foreach (char c in RtoL)
            {
                if ((c >= 0x30) && (c <= 0x47))
                    LineRender.Screen[curOffset] = (byte)c;
                curOffset += LineRender.ScreenWidth;
            }

            // Draw the invaders
            foreach (char c in RtoL)
            {
                byte d = (byte)c;
                if ((c >= 0x30) && (c <= 0x47))
                {
                    d = (byte)(c - '0');
                    if (d > 0x9) d -= 7;
                    d += (byte)alienType;
                }
                LineRender.Screen[curOffset] = d;
                curOffset += LineRender.ScreenWidth;
            }

            foreach (char c in LtoR)
            {
                byte d = (byte)c;
                if ((c >= 0x30) && (c <= 0x47))
                {
                    d = (byte)(c - '0');
                    if (d > 0x9) d -= 7;
                    d += (byte)alienType;
                }
                LineRender.Screen[curOffset] = d;
                curOffset += LineRender.ScreenWidth;
            }

            foreach (char c in LtoR)
            {
                if ((c >= 0x30) && (c <= 0x47))
                    LineRender.Screen[curOffset] = (byte)c;
                curOffset += LineRender.ScreenWidth;
            }
        }

        internal static void ShowExplodedInvaders(MainWindow mainWindow, GameData gameData, int explode, int alienTypeStart)
        {
            mainWindow.StopIsr();
            mainWindow.ClearScreen();
            var player = new PlayerData();
            var aliens = new Aliens2(gameData, player);
            for (int i = 0; i < 55; i++)
                player.Aliens[i] = (byte)((i < 3) ? 1 : 0);

            gameData.AlienCharacterStart = alienTypeStart;
            gameData.AlienCurIndex = 1;

            for (int i = 0; i < 12; i++)
            {
                int y = 29 - i * 2;
                DrawAlienRow(gameData, aliens, 0x10, y, i + 1, true);
                DrawAlienRow(gameData, aliens, 0x70, y, i + 1, false);
            }
            // This chunk displays the low nybble underneath
            //for (int i = 0; i < 12; i++)
            //{
            //    int y = 30 - i * 2;
            //    for (int x = 0; x < LineRender.ScreenHeight; x++)
            //    {
            //        int offs = y + x * LineRender.ScreenWidth;
            //        int c = LineRender.Screen[offs];
            //        if (c > 0x20)
            //        {
            //            c = (c & 0x0f) + 0x30;
            //            if (c > 0x39) c += 7;
            //            LineRender.Screen[offs - 1] = (byte)c;
            //        }
            //    }
            //}
            //for (int i = 0; i < 12; i++)
            //{
            //    int y = 30 - i * 2;
            //    int xOffs = (i / 3) + 1;
            //    EraseAlien(gameData, aliens, 0x10, y, xOffs, explode, true);
            //    EraseAlien(gameData, aliens, 0x70, y, xOffs, explode, false);
            //    EraseAlien(gameData, aliens, 0x58, y, xOffs, 0, true);
            //    EraseAlien(gameData, aliens, 0xb8, y, xOffs, 0, false);
            //}
            mainWindow.RenderScreen();
        }

        private static void EraseAlien(GameData gameData, Aliens2 aliens, int x, int y, int xOffs, int explode, bool rightToLeft)
        {
            gameData.AlienExplodeY = y;
            gameData.RackDirectionRightToLeft = rightToLeft;
            int deltaX = (rightToLeft ? -2 : 2) * xOffs;
            gameData.AlienExplodeX = (x + deltaX + explode * 0x10) >> 3;
            gameData.AlienExplodeXOffset = (x + deltaX + explode * 0x10) & 0x07;
            aliens.ExplodeAlien();
        }

        private static void DrawAlienRow(GameData gameData, Aliens2 aliens, int x, int y, int aliensToMove, bool rightToLeft)
        {
            gameData.AlienCharacterCurY = y;
            gameData.RackDirectionRightToLeft = rightToLeft;
            int deltaX = rightToLeft ? -2 : 2;

            for (int i = 0; i < 3; i++)
            {
                gameData.AlienCharacterCurX = (x + i * 0x10) >> 3;
                gameData.AlienCharacterCurXOffset = (x + i * 0x10) & 0x07;
                aliens.DrawAlien();
            }

            gameData.AlienCharacterCurX = (x + 0x48) >> 3;
            gameData.AlienCharacterCurXOffset = (x + 0x48) & 0x07;
            aliens.DrawAlien();

            while (aliensToMove > 0)
            {
                x = x + deltaX;
                for (int i = 0; i < 3; i++)
                {
                    gameData.AlienCharacterCurX = (x + i * 0x10) >> 3;
                    gameData.AlienCharacterCurXOffset = (x + i * 0x10) & 0x07;
                    aliens.DrawAlien();
                    gameData.AlienCharacterCurX = (x + 0x48) >> 3;
                    gameData.AlienCharacterCurXOffset = (x + 0x48) & 0x07;
                    aliens.DrawAlien();
                    aliensToMove--;
                    if (aliensToMove == 0) break;
                }
            }
        }

    }
}
