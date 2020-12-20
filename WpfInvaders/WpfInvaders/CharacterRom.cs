﻿using System;

namespace WpfInvaders
{
    public class CharacterRom
    {
        private static readonly byte[][] _characterSprites = new byte[][]
        {
            // 8 Bit sprites start here in ASCII order...
            new byte[] {0x00,0x3E,0x45,0x49,0x51,0x3E,0x00,0x00 }, // 0
            new byte[] {0x00,0x00,0x21,0x7F,0x01,0x00,0x00,0x00 }, // 1
            new byte[] {0x00,0x23,0x45,0x49,0x49,0x31,0x00,0x00 }, // 2
            new byte[] {0x00,0x42,0x41,0x49,0x59,0x66,0x00,0x00 }, // 3
            new byte[] {0x00,0x0C,0x14,0x24,0x7F,0x04,0x00,0x00 }, // 4
            new byte[] {0x00,0x72,0x51,0x51,0x51,0x4E,0x00,0x00 }, // 5
            new byte[] {0x00,0x1E,0x29,0x49,0x49,0x46,0x00,0x00 }, // 6
            new byte[] {0x00,0x40,0x47,0x48,0x50,0x60,0x00,0x00 }, // 7
            new byte[] {0x00,0x36,0x49,0x49,0x49,0x36,0x00,0x00 }, // 8
            new byte[] {0x00,0x31,0x49,0x49,0x4A,0x3C,0x00,0x00 }, // 9
            new byte[] {0x00,0x22,0x14,0x7F,0x14,0x22,0x00,0x00 }, // Should be colon is asterix
            new byte[] {0x00,0x08,0x08,0x08,0x08,0x08,0x00,0x00 }, // Should be semi-colon is minus 
            new byte[] {0x00,0x08,0x14,0x22,0x41,0x00,0x00,0x00 }, // <
            new byte[] {0x00,0x14,0x14,0x14,0x14,0x14,0x00,0x00 }, // =
            new byte[] {0x00,0x00,0x41,0x22,0x14,0x08,0x00,0x00 }, // >
            new byte[] {0x00,0x20,0x40,0x4D,0x50,0x20,0x00,0x00 }, // ?
            new byte[] {0x00,0x03,0x04,0x78,0x04,0x03,0x00,0x00 }, // Should be at symbol (@) but is ⅄ (upside down Y)
            new byte[] {0x00,0x1F,0x24,0x44,0x24,0x1F,0x00,0x00 }, // A
            new byte[] {0x00,0x7F,0x49,0x49,0x49,0x36,0x00,0x00 }, // B
            new byte[] {0x00,0x3E,0x41,0x41,0x41,0x22,0x00,0x00 }, // C
            new byte[] {0x00,0x7F,0x41,0x41,0x41,0x3E,0x00,0x00 }, // D
            new byte[] {0x00,0x7F,0x49,0x49,0x49,0x41,0x00,0x00 }, // E
            new byte[] {0x00,0x7F,0x48,0x48,0x48,0x40,0x00,0x00 }, // F
            new byte[] {0x00,0x3E,0x41,0x41,0x45,0x47,0x00,0x00 }, // G
            new byte[] {0x00,0x7F,0x08,0x08,0x08,0x7F,0x00,0x00 }, // H
            new byte[] {0x00,0x00,0x41,0x7F,0x41,0x00,0x00,0x00 }, // I 
            new byte[] {0x00,0x02,0x01,0x01,0x01,0x7E,0x00,0x00 }, // J 
            new byte[] {0x00,0x7F,0x08,0x14,0x22,0x41,0x00,0x00 }, // K 
            new byte[] {0x00,0x7F,0x01,0x01,0x01,0x01,0x00,0x00 }, // L 
            new byte[] {0x00,0x7F,0x20,0x18,0x20,0x7F,0x00,0x00 }, // M 
            new byte[] {0x00,0x7F,0x10,0x08,0x04,0x7F,0x00,0x00 }, // N 
            new byte[] {0x00,0x3E,0x41,0x41,0x41,0x3E,0x00,0x00 }, // O 
            new byte[] {0x00,0x7F,0x48,0x48,0x48,0x30,0x00,0x00 }, // P 
            new byte[] {0x00,0x3E,0x41,0x45,0x42,0x3D,0x00,0x00 }, // Q 
            new byte[] {0x00,0x7F,0x48,0x4C,0x4A,0x31,0x00,0x00 }, // R 
            new byte[] {0x00,0x32,0x49,0x49,0x49,0x26,0x00,0x00 }, // S 
            new byte[] {0x00,0x40,0x40,0x7F,0x40,0x40,0x00,0x00 }, // T
            new byte[] {0x00,0x7E,0x01,0x01,0x01,0x7E,0x00,0x00 }, // U
            new byte[] {0x00,0x7C,0x02,0x01,0x02,0x7C,0x00,0x00 }, // V
            new byte[] {0x00,0x7F,0x02,0x0C,0x02,0x7F,0x00,0x00 }, // W
            new byte[] {0x00,0x63,0x14,0x08,0x14,0x63,0x00,0x00 }, // X
            new byte[] {0x00,0x60,0x10,0x0F,0x10,0x60,0x00,0x00 }, // Y
            new byte[] {0x00,0x43,0x45,0x49,0x51,0x61,0x00,0x00 }, // Z
            new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 }, // [ - Alien Explosion bit
            new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 }, // \ - Alien Explosion bit
            new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 }, // ] - Unused
            new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 }, // ^ - Unused
            new byte[] {0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01 }  // _ - Used for line at bottom of screen
        };

        public static readonly byte[] Shield = { 0xFF,0x0F,0xFF,0x1F,0xFF,0x3F,0xFF,0x7F,0xFF,0xFF,0xFC,0xFF,0xF8,0xFF,0xF0,0xFF,0xF0,0xFF,0xF0,0xFF,0xF0,0xFF,
                                        0xF0,0xFF,0xF0,0xFF,0xF0,0xFF,0xF8,0xFF,0xFC,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0x7F,0xFF,0x3F,0xFF,0x1F,0xFF,0x0F};
        private static readonly byte[] Saucer = { 0x00, 0x00, 0x00, 0x00, 0x04, 0x0C, 0x1E, 0x37, 0x3E, 0x7C, 0x74, 0x7E, 0x7E, 0x74, 0x7C, 0x3E, 0x37, 0x1E, 0x0C, 0x04, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] SaucerExp = { 0x00, 0x22, 0x00, 0xA5, 0x40, 0x08, 0x98, 0x3D, 0xB6, 0x3C, 0x36, 0x1D, 0x10, 0x48, 0x62, 0xB6, 0x1D, 0x98, 0x08, 0x42, 0x90, 0x08, 0x00, 0x00 };
        private static readonly byte[] Player = { 0x00, 0x00, 0x0F, 0x1F, 0x1F, 0x1F, 0x1F, 0x7F, 0xFF, 0x7F, 0x1F, 0x1F, 0x1F, 0x1F, 0x0F, 0x00 };
        private static readonly byte[] PlayerExp1 = { 0x00, 0x04, 0x01, 0x13, 0x03, 0x07, 0xB3, 0x0F, 0x2F, 0x03, 0x2F, 0x49, 0x04, 0x03, 0x00, 0x01 };
        private static readonly byte[] PlayerExp2 = { 0x40, 0x08, 0x05, 0xA3, 0x0A, 0x03, 0x5B, 0x0F, 0x27, 0x27, 0x0B, 0x4B, 0x40, 0x84, 0x11, 0x48 };
        private static readonly byte[] InvaderExp = { 0x00, 0x08, 0x49, 0x22, 0x14, 0x81, 0x42, 0x00, 0x42, 0x81, 0x14, 0x22, 0x49, 0x08, 0x00, 0x00 };
        private static readonly byte[] InvaderA1 = { 0x00, 0x00, 0x39, 0x79, 0x7A, 0x6E, 0xEC, 0xFA, 0xFA, 0xEC, 0x6E, 0x7A, 0x79, 0x39, 0x00, 0x00 };
        private static readonly byte[] InvaderB1 = { 0x00, 0x00, 0x00, 0x78, 0x1D, 0xBE, 0x6C, 0x3C, 0x3C, 0x3C, 0x6C, 0xBE, 0x1D, 0x78, 0x00, 0x00 };
        private static readonly byte[] InvaderC1 = { 0x00, 0x00, 0x00, 0x00, 0x19, 0x3A, 0x6D, 0xFA, 0xFA, 0x6D, 0x3A, 0x19, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] InvaderA2 = { 0x00, 0x00, 0x38, 0x7A, 0x7F, 0x6D, 0xEC, 0xFA, 0xFA, 0xEC, 0x6D, 0x7F, 0x7A, 0x38, 0x00, 0x00 };
        private static readonly byte[] InvaderB2 = { 0x00, 0x00, 0x00, 0x0E, 0x18, 0xBE, 0x6D, 0x3D, 0x3C, 0x3D, 0x6D, 0xBE, 0x18, 0x0E, 0x00, 0x00 };
        private static readonly byte[] InvaderC2 = { 0x00, 0x00, 0x00, 0x00, 0x1A, 0x3D, 0x68, 0xFC, 0xFC, 0x68, 0x3D, 0x1A, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] Characters = new byte[256 * 8];

        static CharacterRom()
        {
            // 0x00-0x3f Are bitmaps
            // 0x20      Space
            // 0x21-0x30 Alien explosion shifted 0,2,4,6
            // 0x40-0x5f Uppercase mostly ASCII
            int i = 0;
            foreach (var sprite in _characterSprites)
            {
                int charOffset = (i + 48) * 8;
                foreach (byte b in sprite)
                    Characters[charOffset++] = BitFlip(b);
                i++;
            }
            GenerateExplosionsShifted(0x21);
            // 0x60-0x6f Invaders Row 1 / 2.
            GenerateAlienShifts(0x60, InvaderA1, InvaderA2);
            // 0x70-0x7f Invaders Row 3 / 4 
            GenerateAlienShifts(0x70, InvaderB1, InvaderB2);
            // 0x80-0x8f Invaders Row 5
            GenerateAlienShifts(0x80, InvaderC1, InvaderC2);
            // 0x90-0x97 Unused..
            // 0x98-0xa8 Saucer shifts 0,2,4,6
            GenerateSpritesShifted(0x98, Saucer, 2);
            // 0xa8-b7 Saucer explosion shifts 0,2,4,6
            GenerateSpritesShifted(0xa8, SaucerExp, 2);
            // 0xb8-d0 Player shifted by 1 pixel up to 8
            GenerateSpritesShifted(0xb8, Player, 1);
            // 0xd0-e8 Play Exp2 shifted by 1 pixel up to 8
            GenerateSpritesShifted(0xd0, PlayerExp1, 1);
            // 0xe8-ff Play Exp1 shifted by 1 pixel up to 8
            GenerateSpritesShifted(0xe8, PlayerExp2, 1);
        }

        private static void GenerateExplosionsShifted(int offset)
        {
            offset *= 8;
            for (int i = 0; i < 8; i++)
            {
                // Explosion shifted by 0
                Characters[offset + i + 0x00] = BitFlip(InvaderExp[i + 0]);                            // 0x21 !
                Characters[offset + i + 0x08] = BitFlip(InvaderExp[i + 8]);                            // 0x22 "
                // Explosion shifted by 2
                Characters[offset + i + 0x10] = BitFlip(i > 2 ? InvaderExp[i - 2] : (byte)0x00);       // 0x23 #
                Characters[offset + i + 0x18] = BitFlip(InvaderExp[i + 6]);                            // 0x24 $
                // Explosion shifted by 4
                Characters[offset + i + 0x20] = BitFlip(i > 4 ? InvaderExp[i - 4] : (byte)0x00);       // 0x25 %
                Characters[offset + i + 0x28] = BitFlip(InvaderExp[i + 4]);                            // 0x26 & 
                Characters[offset + i + 0x30] = BitFlip(i < 4 ? InvaderExp[i + 12] : (byte)0x00);      // 0x27 '
                // Explosion shifted by 6
                Characters[offset + i + 0x38] = BitFlip(i > 6 ? InvaderExp[i - 6] : (byte)0x00);       // 0x28 (
                Characters[offset + i + 0x40] = BitFlip(InvaderExp[i + 2]);                            // 0x29 ) 
                Characters[offset + i + 0x48] = BitFlip(i < 6 ? InvaderExp[i + 10] : (byte)0x00);      // 0x2a *
                // Explosion shifted by 4 with bits of type A1 alien
                Characters[offset + i + 0x50] = BitFlip(i < 4 ? InvaderExp[i + 12] : InvaderA1[i - 4]);// 0x2b +
                Characters[offset + i + 0x58] = BitFlip(i < 4 ? InvaderA1[i + 12] : InvaderExp[i - 4]);// 0x2c , 
                // Explosion shifted by 4 with bits of type B1 alien
                Characters[offset + i + 0x60] = BitFlip(i < 4 ? InvaderExp[i + 12] : InvaderB1[i - 4]);// 0x2d -
                Characters[offset + i + 0x68] = BitFlip(i < 4 ? InvaderB1[i + 12] : InvaderExp[i - 4]);// 0x2e . 
                // Explosion shifted by 4 with bits of type C1 alien. Because the C1 
                // Alien has more space to either side, we don't need two characters.
                Characters[offset + i + 0x70] = BitFlip(i < 4 ? InvaderC1[i + 12] : InvaderExp[i - 4]);// 0x2f / 
                // Explosion shifted by 6 catches a lot of alien
                Characters[0x5b * 8 + i] = BitFlip(i < 6 ? InvaderA2[i + 10] : InvaderExp[i - 6]);// 0x5b [ 
                Characters[0x5c * 8 + i] = BitFlip(i < 6 ? InvaderB2[i + 10] : InvaderExp[i - 6]);// 0x5c \ 
                Characters[0x5d * 8 + i] = BitFlip(i < 6 ? InvaderC2[i + 10] : InvaderExp[i - 6]);// 0x5d ] 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="sprite"></param>
        /// <param name="step"></param>
        private static void GenerateSpritesShifted(int offset, byte[] sprite, int step)
        {
            offset *= 8;
            for (int i = 0; i < 8; i += step)
            {
                int stride = sprite.Length + 8;
                for (int j = 0; j < stride; j++)
                {
                    Characters[offset + j] = 0x00;
                    if ((j >= i) && ((j - i) < sprite.Length))
                    {
                        Characters[offset + j] = BitFlip(sprite[j - i]);
                    }
                }
                offset += stride;
            }
        }

        /// <summary>
        /// Generates the shifted characters necessary for an alien.
        /// </summary>
        /// <param name="offset">Offset in the character table to start storing.</param>
        /// <param name="invader1">Pattern for invader type 1.</param>
        /// <param name="invader2">Pattern for invader type 2.</param>
        private static void GenerateAlienShifts(int offset, byte[] invader1, byte[] invader2)
        {
            offset *= 8;
            for (int i = 0; i < 8; i++)
            {
                // Invader patterns as they move across the screen for
                // row 1 and 2 assuming invaders set to start at 0x60
                // L to R  | R to L
                // `a`a`a  |  `a`a`a
                // fg`a`a  |  hi`a`a
                // fgfg`a  |  hihi`a
                // fgfgfg  |  hihihi
                // bcjgfg  | bcehihi
                // bcdcjg  | bcdcehi
                // bcdcdce | bcdcdce
                //  hkcdce | fgbcdce
                //  hihkce | fgfgbce
                //  hihihi | fgfgfg 
                //  `ahihi | `afgfg 
                //  `a`ahi | `a`afg 
                //  `a`a`a | `a`a`a 
                // Solo invaders type 1 is in shift postions 0 & 4
                Characters[offset + i + 0x00] = BitFlip(invader1[i + 0]);                            // 0x00 `
                Characters[offset + i + 0x08] = BitFlip(invader1[i + 8]);                            // 0x01 a
                // Invader type 1 in shift pos 4 has two endings either BCE for a single
                // Invader BCDCE for two aliens and BCDCDCE for three. 
                Characters[offset + i + 0x10] = BitFlip(i > 4 ? invader1[i - 4] : (byte)0x00);       // 0x02 b
                Characters[offset + i + 0x18] = BitFlip(invader1[i + 4]);                            // 0x03 c 
                Characters[offset + i + 0x20] = BitFlip(i < 4 ? invader1[i + 12] : invader1[i - 4]); // 0x04 d
                Characters[offset + i + 0x28] = BitFlip(i < 4 ? invader1[i + 12] : (byte)0x00);      // 0x05 e
                // Solo invaders type 2 is in shift postions 2 
                Characters[offset + i + 0x30] = BitFlip(i > 2 ? invader2[i - 2] : (byte)0x00);       // 0x06 f 
                Characters[offset + i + 0x38] = BitFlip(invader2[i + 6]);                            // 0x07 g 
                // Solo invaders type 2 is in shift postions 6 
                Characters[offset + i + 0x40] = BitFlip(invader2[i + 2]);                            // 0x08 h
                Characters[offset + i + 0x48] = BitFlip(i < 6 ? invader2[i + 10] : (byte)0x00);      // 0x09 i
                // Couple of weird ones as we go left to right, shifted by four needs
                // some extra aliens as it overlaps with two and six shifts.
                Characters[offset + i + 0x50] = BitFlip(i < 4 ? invader1[i + 12] : invader2[i - 2]); // 0x0a j
                Characters[offset + i + 0x58] = BitFlip(i < 6 ? invader2[i + 10] : invader1[i - 4]); // 0x0b k
                // Invader 2 in shift postion 0.
                Characters[offset + i + 0x60] = BitFlip(invader2[i + 0]);                            // 0x0c l
                Characters[offset + i + 0x68] = BitFlip(invader2[i + 8]);                            // 0x0d m
                // Alien explosion shifted by 4 could have some bits of the next door alien
                // From the two and six bit shifts
                Characters[offset + i + 0x70] = BitFlip(i < 4 ? InvaderExp[i + 12] : invader2[i - 2]);// 0x0e n
                Characters[offset + i + 0x78] = BitFlip(i < 6 ? invader2[i + 10] : InvaderExp[i - 4]);// 0x0f o
            }
        }

        /// <summary>
        /// Flips a byte around to match the video hardware.
        /// </summary>
        /// <param name="b">Byte to flip.</param>
        /// <returns>Flipped byte of data.</returns>
        public static byte BitFlip(byte b)
        {
            return (byte)(
                ((b & 0x01) != 0 ? 0x80 : 0x00) |
                ((b & 0x02) != 0 ? 0x40 : 0x00) |
                ((b & 0x04) != 0 ? 0x20 : 0x00) |
                ((b & 0x08) != 0 ? 0x10 : 0x00) |
                ((b & 0x10) != 0 ? 0x08 : 0x00) |
                ((b & 0x20) != 0 ? 0x04 : 0x00) |
                ((b & 0x40) != 0 ? 0x02 : 0x00) |
                ((b & 0x80) != 0 ? 0x01 : 0x00));
        }
    }
}