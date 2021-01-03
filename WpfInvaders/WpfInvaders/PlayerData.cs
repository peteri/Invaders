﻿using System;

namespace WpfInvaders
{
    internal class PlayerData
    {
        internal ushort Score;
        internal int ShipsRem;

        internal byte[] Sheild1 = new byte[6 * 8];
        internal byte[] Sheild2 = new byte[8 * 8];
        internal byte[] Sheild3 = new byte[6 * 8];
        internal byte[] Sheild4 = new byte[8 * 8];
        internal byte[] Aliens = new byte[55];
        internal int RefAlienDeltaX;
        internal int RefAlienY;
        internal int RefAlienX;
        internal int RackCount;
        internal int NumAliens;
        internal bool ExtraShipAvailable;

        internal void Reset()
        {
            Score = 0;
            ShipsRem = 3;
            InitAliens();
            RefAlienDeltaX = 2;
            RackCount = 0;
            RefAlienX = 0x18;
            RefAlienY = 0x78;
            ExtraShipAvailable = true;
            ResetShields();
        }

        internal void ResetShields()
        {
            for (int i = 0; i < 22; i++)
            {
                Sheild1[i] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2 + 1]);
                Sheild1[i + 24] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2]);

                Sheild2[i + 5] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2 + 1]);
                Sheild2[i + 32 + 5] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2]);

                Sheild3[i + 2] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2 + 1]);
                Sheild3[i + 24 + 2] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2]);

                Sheild4[i + 7] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2 + 1]);
                Sheild4[i + 32 + 7] = CharacterRom.BitFlip(CharacterRom.Shield[i * 2]);
            }
            CopyShieldToBitmapChar();
        }

        internal void CopyShieldToBitmapChar()
        {
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    LineRender.BitmapChar[j + i * 8] = Sheild1[j + i * 8];
                    LineRender.BitmapChar[j + (i + 6 + 8) * 8] = Sheild3[j + i * 8];
                }
                for (int i = 0; i < 8; i++)
                {
                    LineRender.BitmapChar[j + (i + 6) * 8] = Sheild2[j + i * 8];
                    LineRender.BitmapChar[j + (i + 6 + 8 + 6) * 8] = Sheild4[j + i * 8];
                }
            }
        }

        internal void CopyBitmapCharToShield()
        {
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    Sheild1[j + i * 8] = LineRender.BitmapChar[j + i * 8];
                    Sheild3[j + i * 8] = LineRender.BitmapChar[j + (i + 6 + 8) * 8];
                }
                for (int i = 0; i < 8; i++)
                {
                    Sheild2[j + i * 8] = LineRender.BitmapChar[j + (i + 6) * 8];
                    Sheild4[j + i * 8] = LineRender.BitmapChar[j + (i + 6 + 8 + 6) * 8];
                }
            }
        }

        internal void CountAliens()
        {
            NumAliens = 0;
            for (int i = 0; i < Aliens.Length; i++)
                NumAliens += Aliens[i];
        }

        internal void InitAliens()
        {
            for (int i = 0; i < Aliens.Length; i++)
                Aliens[i] = 1;
            CountAliens();
        }
    }
}
