using System;

namespace WpfInvaders
{
    internal class CharacterRom
    {
        private static readonly byte[][] _characterSprites = new byte[][]
        {
            // 8 Bit sprites start here in some sort of order...
            new byte[] {0x00,0x03,0x04,0x78,0x04,0x03,0x00,0x00 }, // ⅄ (upside down Y) =0xf7
            new byte[] {0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01 }, // _ = 0xf3
            new byte[] {0x00,0x3E,0x45,0x49,0x51,0x3E,0x00,0x00 }, // 0 = 0x30
            new byte[] {0x00,0x00,0x21,0x7F,0x01,0x00,0x00,0x00 }, // 1 = 0x31
            new byte[] {0x00,0x23,0x45,0x49,0x49,0x31,0x00,0x00 }, // 2 = 0x32
            new byte[] {0x00,0x42,0x41,0x49,0x59,0x66,0x00,0x00 }, // 3 = 0x33
            new byte[] {0x00,0x0C,0x14,0x24,0x7F,0x04,0x00,0x00 }, // 4 = 0x34
            new byte[] {0x00,0x72,0x51,0x51,0x51,0x4E,0x00,0x00 }, // 5 = 0x35
            new byte[] {0x00,0x1E,0x29,0x49,0x49,0x46,0x00,0x00 }, // 6 = 0x36
            new byte[] {0x00,0x40,0x47,0x48,0x50,0x60,0x00,0x00 }, // 7 = 0x37
            new byte[] {0x00,0x36,0x49,0x49,0x49,0x36,0x00,0x00 }, // 8 = 0x38
            new byte[] {0x00,0x31,0x49,0x49,0x4A,0x3C,0x00,0x00 }, // 9 = 0x39
            new byte[] {0x00,0x1F,0x24,0x44,0x24,0x1F,0x00,0x00 }, // A = 0x3a
            new byte[] {0x00,0x7F,0x49,0x49,0x49,0x36,0x00,0x00 }, // B = 0x3b
            new byte[] {0x00,0x3E,0x41,0x41,0x41,0x22,0x00,0x00 }, // C = 0x3c
            new byte[] {0x00,0x7F,0x41,0x41,0x41,0x3E,0x00,0x00 }, // D = 0x3d
            new byte[] {0x00,0x7F,0x49,0x49,0x49,0x41,0x00,0x00 }, // E = 0x3e
            new byte[] {0x00,0x7F,0x48,0x48,0x48,0x40,0x00,0x00 }, // F = 0x3f
            new byte[] {0x00,0x3E,0x41,0x41,0x45,0x47,0x00,0x00 }, // G = 0x40
            new byte[] {0x00,0x7F,0x08,0x08,0x08,0x7F,0x00,0x00 }, // H = 0x41
            new byte[] {0x00,0x00,0x41,0x7F,0x41,0x00,0x00,0x00 }, // I = 0x42
            new byte[] {0x00,0x7F,0x01,0x01,0x01,0x01,0x00,0x00 }, // L = 0x43
            new byte[] {0x00,0x7F,0x20,0x18,0x20,0x7F,0x00,0x00 }, // M = 0x44
            new byte[] {0x00,0x7F,0x10,0x08,0x04,0x7F,0x00,0x00 }, // N = 0x45
            new byte[] {0x00,0x3E,0x41,0x41,0x41,0x3E,0x00,0x00 }, // O = 0x46
            new byte[] {0x00,0x7F,0x48,0x48,0x48,0x30,0x00,0x00 }, // P = 0x47
            new byte[] {0x00,0x7F,0x48,0x4C,0x4A,0x31,0x00,0x00 }, // R = 0x48
            new byte[] {0x00,0x32,0x49,0x49,0x49,0x26,0x00,0x00 }, // S = 0x49
            new byte[] {0x00,0x40,0x40,0x7F,0x40,0x40,0x00,0x00 }, // T = 0x4a
            new byte[] {0x00,0x7E,0x01,0x01,0x01,0x7E,0x00,0x00 }, // U = 0x4b
            new byte[] {0x00,0x7C,0x02,0x01,0x02,0x7C,0x00,0x00 }, // V = 0x4c
            new byte[] {0x00,0x60,0x10,0x0F,0x10,0x60,0x00,0x00 }, // Y = 0x4d
            new byte[] {0x00,0x22,0x14,0x7F,0x14,0x22,0x00,0x00 }, // * = 0x4e
            new byte[] {0x00,0x08,0x08,0x08,0x08,0x08,0x00,0x00 }, // - = 0x4f
            new byte[] {0x00,0x08,0x14,0x22,0x41,0x00,0x00,0x00 }, // < = 0x50
            new byte[] {0x00,0x14,0x14,0x14,0x14,0x14,0x00,0x00 }, // = = 0x51
            new byte[] {0x00,0x00,0x41,0x22,0x14,0x08,0x00,0x00 }, // > = 0x52
            new byte[] {0x00,0x20,0x40,0x4D,0x50,0x20,0x00,0x00 }, // ? = 0x53
        };

        internal static byte[] Map = new byte[128];

        internal static readonly byte[] Shield = { 0xFF,0x0F,0xFF,0x1F,0xFF,0x3F,0xFF,0x7F,0xFF,0xFF,0xFC,0xFF,0xF8,0xFF,0xF0,0xFF,0xF0,0xFF,0xF0,0xFF,0xF0,0xFF,
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
        internal static byte[] Characters = new byte[256 * 8];

        static CharacterRom()
        {
            // 0x00-0x3f Are bitmaps
            // 0x20      Space
            // 0x40-0x5f Uppercase mostly ASCII
            int i = 0;
            foreach (var sprite in _characterSprites)
            {
                int charOffset = (i + 0x2e) * 8;
                foreach (byte b in sprite)
                    Characters[charOffset++] = BitFlip(b);
                i++;
            }
            // 0x96-0x97 Unused..
            // 0x98-0xa8 Saucer shifts 0,2,4,6
            GenerateSpritesShifted(0x20, Saucer, 2);
            // 0xa8-b7 Saucer explosion shifts 0,2,4,6
            GenerateSpritesShifted(0xf0, SaucerExp, 2);
            // 0xb8-d0 Player shifted by 1 pixel up to 8
            GeneratePlayerShifted(0x56, Player, PlayerExp1, PlayerExp2, 0, 16);
            GeneratePlayerShifted(0x5c, Player, PlayerExp1, PlayerExp2, 1, 24);
            GeneratePlayerShifted(0x65, Player, PlayerExp1, PlayerExp2, 2, 24);
            GeneratePlayerShifted(0x6e, Player, PlayerExp1, PlayerExp2, 3, 24);
            GeneratePlayerShifted(0x77, Player, PlayerExp1, PlayerExp2, 4, 24);
            GeneratePlayerShifted(0xc0, Player, PlayerExp1, PlayerExp2, 5, 24);
            GeneratePlayerShifted(0xd0, Player, PlayerExp1, PlayerExp2, 6, 24);
            GeneratePlayerShifted(0xe0, Player, PlayerExp1, PlayerExp2, 7, 24);
            GenerateAlienShifts(0x80, InvaderA1, InvaderA2);
            GenerateAlienShifts(0x90, InvaderB1, InvaderB2);
            GenerateAlienShifts(0xa0, InvaderC1, InvaderC2);
            GenerateExplosionsShifted2(0xb0);
            Array.Fill<byte>(Map, 0xff);
            i = 0x30;
            Map[0x20] = 0x23;   // Map space to a blank character
            foreach (char c in "0123456789ABCDEFGHILMNOPRSTUVY*-<=>?")
                Map[c] = (byte)(i++);
            // Deal with upside down Y & _
            for (i = 0; i < 8; i++)
            {
                Characters[(0xf7 * 8) + i] = BitFlip(_characterSprites[0][i]);
                Characters[(0xf3 * 8) + i] = BitFlip(_characterSprites[1][i]);
            }
            Map['_'] = 0xf3;
            Map['@'] = 0xf7;
        }

        private static void GeneratePlayerShifted(int offset, byte[] player, byte[] playerExp1, byte[] playerExp2, int step, int stride)
        {
            offset *= 8;
            for (int j = 0; j < stride; j++)
            {
                Characters[offset + j] = 0x00;
                if ((j >= step) && ((j - step) < player.Length))
                {
                    Characters[offset + j] = BitFlip(player[j - step]);
                    Characters[offset + stride + j] = BitFlip(playerExp1[j - step]);
                    Characters[offset + stride * 2 + j] = BitFlip(playerExp2[j - step]);
                }
            }
        }

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
        /// Flips a byte around to match the video hardware.
        /// </summary>
        /// <param name="b">Byte to flip.</param>
        /// <returns>Flipped byte of data.</returns>
        internal static byte BitFlip(byte b)
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

        private static void GenerateExplosionsShifted2(int offset)
        {
            offset *= 8;
            for (int i = 0; i < 8; i++)
            {
                // Explosion shifted by 0
                Characters[offset + i + 0x00] = BitFlip(InvaderExp[i + 0]);
                Characters[offset + i + 0x08] = BitFlip(InvaderExp[i + 8]);
                // Explosion shifted by 2
                Characters[offset + i + 0x10] = BitFlip(i > 2 ? InvaderExp[i - 2] : (byte)0x00);
                Characters[offset + i + 0x18] = BitFlip(InvaderExp[i + 6]);
                // Explosion shifted by 4
                Characters[offset + i + 0x20] = BitFlip(i > 4 ? InvaderExp[i - 4] : (byte)0x00);
                Characters[offset + i + 0x28] = BitFlip(InvaderExp[i + 4]);
                Characters[offset + i + 0x30] = BitFlip(i < 4 ? InvaderExp[i + 12] : (byte)0x00);
                // Explosion shifted by 6
                Characters[offset + i + 0x38] = BitFlip(i > 6 ? InvaderExp[i - 6] : (byte)0x00);
                Characters[offset + i + 0x40] = BitFlip(InvaderExp[i + 2]);
                Characters[offset + i + 0x48] = BitFlip(i < 6 ? InvaderExp[i + 10] : (byte)0x00);
                // Invader A2
                Characters[offset + i + 0x50] = BitFlip(InvaderA2[i + 0]);
                Characters[offset + i + 0x58] = BitFlip(InvaderA2[i + 8]);
                // Invader B2
                Characters[offset + i + 0x60] = BitFlip(InvaderB2[i + 0]);
                Characters[offset + i + 0x68] = BitFlip(InvaderB2[i + 8]);
                // Invader C2
                Characters[offset + i + 0x70] = BitFlip(InvaderC2[i + 0]);
                Characters[offset + i + 0x78] = BitFlip(InvaderC2[i + 8]);
                // A type one alien shifted by four pixels (0xn5) is flanked on either side
                // by bits of two aliens the ones on left (0x0a or 0x0c) need special case
                // handling (the ones on the right are done by adding 0x40)
                // 0xnA
                Characters[0x27 * 8 + i] = BitFlip(i < 4 ? InvaderA1[i + 12] : InvaderExp[i - 4]);
                Characters[0x28 * 8 + i] = BitFlip(i < 4 ? InvaderB1[i + 12] : InvaderExp[i - 4]);
                Characters[0x2b * 8 + i] = BitFlip(i < 4 ? InvaderC1[i + 12] : InvaderExp[i - 4]);
                // 0xnC
                Characters[0x2c * 8 + i] = BitFlip(i < 4 ? InvaderA2[i + 10] : InvaderExp[i - 4]);
                Characters[0x54 * 8 + i] = BitFlip(i < 4 ? InvaderB2[i + 10] : InvaderExp[i - 4]);
                Characters[0x55 * 8 + i] = BitFlip(i < 4 ? InvaderC2[i + 10] : InvaderExp[i - 4]);
                // When going Left to right a type two alien can have two bits of a type 1 
                // 0xnB
                Characters[0xc9 * 8 + i] = BitFlip(i < 2 ? InvaderA1[i + 12] : InvaderExp[i - 2]);
                Characters[0xd9 * 8 + i] = BitFlip(i < 2 ? InvaderB1[i + 12] : InvaderExp[i - 2]);
                Characters[0xe9 * 8 + i] = BitFlip(i < 2 ? InvaderC1[i + 12] : InvaderExp[i - 2]);
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
                // Solo invaders type 1 in shift postions 0 
                Characters[offset + i + 0x00] = BitFlip(invader1[i + 0]);                            // 0x00 
                Characters[offset + i + 0x08] = BitFlip(invader1[i + 8]);                            // 0x01 
                // Solo invaders type 2 in shift postions 2 
                Characters[offset + i + 0x10] = BitFlip(i > 2 ? invader2[i - 2] : (byte)0x00);       // 0x02  
                Characters[offset + i + 0x18] = BitFlip(invader2[i + 6]);                            // 0x03  
                // Solo Invader type 1 in shift pos 4 
                Characters[offset + i + 0x20] = BitFlip(i > 4 ? invader1[i - 4] : (byte)0x00);       // 0x04 
                Characters[offset + i + 0x28] = BitFlip(invader1[i + 4]);                            // 0x05  
                Characters[offset + i + 0x30] = BitFlip(i < 4 ? invader1[i + 12] : (byte)0x00);      // 0x06 
                // Solo invaders type 2 in shift postion 6
                Characters[offset + i + 0x38] = BitFlip(i < 6 ? (byte)0 : invader2[i - 6]);          // 0x07 
                Characters[offset + i + 0x40] = BitFlip(invader2[i + 2]);                            // 0x08 
                Characters[offset + i + 0x48] = BitFlip(i < 6 ? invader2[i + 10] : (byte)0x00);      // 0x09 

                // Overlaps for shifted by 4
                Characters[offset + i + 0x50] = BitFlip(i < 4 ? invader1[i + 12] : invader1[i - 4]); // 0x0a 
                Characters[offset + i + 0x58] = BitFlip(i < 2 ? invader1[i + 12] : invader2[i - 2]); // 0x0b 
                Characters[offset + i + 0x60] = BitFlip(i < 6 ? invader2[i + 10] : invader1[i - 4]); // 0x0c 
                Characters[offset + i + 0x68] = BitFlip(i < 4 ? invader1[i + 12] : (i < 6) ? (byte)0 : invader2[i - 6]); // 0x0d 
                // Overlaps for shifted by 6
                Characters[offset + i + 0x70] = BitFlip(i < 6 ? invader2[i + 10] : invader2[i - 6]); // 0x0e 
                Characters[offset + i + 0x78] = BitFlip(i < 6 ? invader1[i + 8] : invader2[i - 6]); // 0x0f 
                // Overlaps with explosions

                // Overlaps for shifted by 4
                Characters[offset + i + 0x250] = BitFlip(i < 4 ? InvaderExp[i + 12] : invader1[i - 4]); // 0x0a 
                Characters[offset + i + 0x258] = BitFlip(i < 2 ? InvaderExp[i + 12] : invader2[i - 2]); // 0x0b 
                Characters[offset + i + 0x260] = BitFlip(i < 6 ? InvaderExp[i + 10] : invader1[i - 4]); // 0x0c 
                Characters[offset + i + 0x268] = BitFlip(i < 4 ? invader1[i + 12] : (i < 6) ? (byte)0 : InvaderExp[i - 6]); // 0x0d 
                // Overlaps for shifted by 6
                Characters[offset + i + 0x270] = BitFlip(i < 6 ? invader2[i + 10] : InvaderExp[i - 6]); // 0x0e 
                Characters[offset + i + 0x278] = BitFlip(i < 6 ? invader1[i + 8] : InvaderExp[i - 6]); // 0x0f 
            }
        }
    }
}
