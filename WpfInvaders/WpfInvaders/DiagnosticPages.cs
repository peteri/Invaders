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

        internal static void ShowShiftedInvaders(MainWindow mainWindow,int explode)
        {
            mainWindow.StopIsr();
            mainWindow.ClearScreen();
            DrawShiftedAliens(0x1f, "L TO R ; R TO L");

            DrawShiftedAliens(0x1d, "\x60\x61\x60\x61\x60\x61\x20;\x20\x60\x61\x60\x61\x60\x61");
            DrawShiftedAliens(0x1b, "\x66\x67\x66\x67\x60\x61\x20;\x20\x68\x69\x60\x61\x60\x61");
            DrawShiftedAliens(0x19, "\x66\x67\x66\x67\x60\x61\x20;\x20\x68\x69\x68\x69\x60\x61");

            DrawShiftedAliens(0x17, "\x66\x67\x66\x67\x66\x67\x20;\x20\x68\x69\x68\x69\x68\x69");
            DrawShiftedAliens(0x15, "\x62\x63\x6A\x67\x66\x67\x20;\x62\x63\x65\x68\x69\x68\x69");
            DrawShiftedAliens(0x13, "\x62\x63\x64\x63\x6A\x67\x20;\x62\x63\x64\x63\x65\x68\x69");

            DrawShiftedAliens(0x11, "\x62\x63\x64\x63\x64\x63\x65;\x62\x63\x64\x63\x64\x63\x65");
            DrawShiftedAliens(0x0f, "\x20\x68\x6B\x63\x64\x63\x65;\x66\x67\x62\x63\x64\x63\x65");
            DrawShiftedAliens(0x0d, "\x20\x68\x69\x68\x6B\x63\x65;\x66\x67\x66\x67\x62\x63\x65");

            DrawShiftedAliens(0x0b, "\x20\x68\x69\x68\x69\x68\x69;\x66\x67\x66\x67\x66\x67\x20");
            DrawShiftedAliens(0x09, "\x20\x60\x61\x68\x69\x68\x69;\x60\x61\x66\x67\x66\x67\x20");
            DrawShiftedAliens(0x07, "\x20\x60\x61\x60\x61\x68\x69;\x60\x61\x60\x61\x66\x67\x20");
            DrawShiftedAliens(0x05, "\x20\x60\x61\x60\x61\x60\x61;\x60\x61\x60\x61\x60\x61\x20");

            mainWindow.RenderScreen();
        }

        private static void DrawShiftedAliens(int y, string text)
        {
            string LtoR = text.Substring(0, 7);
            string RtoL = text.Substring(8, 7);
            int curOffset = y;
            foreach (char c in RtoL)
            {
                if ((c >= 0x60) && (c <= 0x69))
                    LineRender.Screen[curOffset] = (byte)((c & 0x0f) + 0x30);
                if ((c >= 0x6a) && (c <= 0x6f))
                    LineRender.Screen[curOffset] = (byte)((c & 0x0f) + 0x37);
                curOffset += LineRender.ScreenWidth;
            }
            // Draw the invaders
            foreach (char c in RtoL)
            {
                LineRender.Screen[curOffset] = (byte)c;
                curOffset += LineRender.ScreenWidth;
            }
            foreach (char c in LtoR)
            {
                LineRender.Screen[curOffset] = (byte)c;
                curOffset += LineRender.ScreenWidth;
            }
            foreach (char c in LtoR)
            {
                if ((c >= 0x60) && (c <= 0x69))
                    LineRender.Screen[curOffset] = (byte)((c & 0x0f) + 0x30);
                if ((c >= 0x6a) && (c <= 0x6f))
                    LineRender.Screen[curOffset] = (byte)((c & 0x0f) + 0x37);
                curOffset += LineRender.ScreenWidth;
            }
        }

    }
}
