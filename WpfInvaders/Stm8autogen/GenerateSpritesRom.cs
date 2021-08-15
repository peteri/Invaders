﻿using System;
using System.Collections.Generic;
using System.IO;
using WpfInvaders;

namespace Stm8autogen
{
    public static class GenerateSpritesRom
    {
        static List<string> asmLines = new List<string>();
        static List<string> incLines = new List<string>();
        internal static void Generate(List<(string name, Sprite sprite)> sprites)
        {
            asmLines.Add("stm8/");
            asmLines.Add(";=============================================");
            asmLines.Add("; Generated file from the WPF invaders");
            asmLines.Add("; Contains pre-shifted sprites.");
            asmLines.Add(";=============================================");
            incLines.Add(";=============================================");
            incLines.Add("; Generated include file for the sprites.");
            incLines.Add(";=============================================");
            asmLines.Add("\tsegment \"rom\"");
            foreach (var sprite in sprites)
            {
                AddLabel(sprite.name, "data");
                int imageCount = sprite.sprite.data.GetUpperBound(0) + 1;
                for (int image = 0; image < imageCount; image++)
                {
                    for (int shift = 0; shift < 8; shift++)
                    {
                        AddLabel(sprite.name, $"img{image}_shift{shift}");
                        for (int n = 0; n < sprite.sprite.width; n++)
                            asmLines.Add("\tdc.w\t %" +
                                ByteToStr(sprite.sprite.data[image, shift, n, 1]) +
                                ByteToStr(sprite.sprite.data[image, shift, n, 0])
                                );
                    }
                }
            }
            asmLines.Add("\tend");
            File.WriteAllLines("spritedata.asm", asmLines);
            File.WriteAllLines("spritedata.inc", incLines);
        }

        private static string AddLabel(string prefix, string suffix)
        {
            string retVal = $"{prefix}_{suffix}";
            asmLines.Add($".{retVal}.w");
            incLines.Add($"\tEXTERN {retVal}.w");
            return retVal;
        }

        private static string ByteToStr(byte b)
        {
            string retVal = "";
            for(int i=0;i<8;i++)
            {
                retVal += (b & 0x01) == 0 ? "0" : "1";
                b = (byte) (b >> 1);
            }
            return retVal;
        }
    }
}
