using System;
using System.Collections.Generic;
using System.IO;
using WpfInvaders;

namespace Stm8autogen
{
    public static class GenerateCharacterRom
    {
        public static void Generate()
        {
            List<string> asmLines = new List<string>();
            List<string> incLines = new List<string>();
            var player = new PlayerData();
            player.Reset();
            asmLines.Add("stm8/");
            asmLines.Add(";=============================================");
            asmLines.Add("; Generated file from the WPF character map");
            asmLines.Add("; Contains bitmaps for the screen and a map from");
            asmLines.Add("; ASCII to the on screen character map.");
            asmLines.Add(";=============================================");
            incLines.Add(";=============================================");
            incLines.Add("; Generated include file for the character map.");
            incLines.Add(";=============================================");
            asmLines.Add("\tPUBLIC charactermap");
            incLines.Add("\tEXTERN charactermap");
            asmLines.Add("\tPUBLIC characterrom");
            incLines.Add("\tEXTERN characterrom");
            asmLines.Add("\tWORDS\t; The following addresses are 16 bits long");
            asmLines.Add("\tsegment byte at 8080-80FF \'charmap\'");
            asmLines.Add("charactermap.w");
            for (int i = 0; i < 128; i += 8)
            {
                asmLines.Add(String.Format("\tDC.B ${0:X2},${1:X2},${2:X2},${3:X2},${4:X2},${5:X2},${6:X2},${7:X2}  ; ${8:X2} - ${9:X2}",
                    CharacterRom.Map[(i + 0)],
                    CharacterRom.Map[(i + 1)],
                    CharacterRom.Map[(i + 2)],
                    CharacterRom.Map[(i + 3)],
                    CharacterRom.Map[(i + 4)],
                    CharacterRom.Map[(i + 5)],
                    CharacterRom.Map[(i + 6)],
                    CharacterRom.Map[(i + 7)],
                    i, i + 7
                    ));
            }
            asmLines.Add("\tWORDS\t; The following addresses are 16 bits long");
            asmLines.Add("\tsegment byte at 8100-88FF \'charset\'");
            incLines.Add("charrom\tequ\t$8100");
            asmLines.Add("characterrom.w");
            for (int j = 0; j < 8; j++)
            {
                asmLines.Add(";==============");
                asmLines.Add(String.Format("; Line {0}", j));
                asmLines.Add(";==============");
                for (int i = 0; i < 256; i += 8)
                {
                    if (i < 0x20)
                    {
                        asmLines.Add(String.Format("\tDC.B ${0:X2},${1:X2},${2:X2},${3:X2},${4:X2},${5:X2},${6:X2},${7:X2}  ; ${8:X2} - ${9:X2}",
                            LineRender.BitmapChar[(i + 0) * 8 + j],
                            LineRender.BitmapChar[(i + 1) * 8 + j],
                            LineRender.BitmapChar[(i + 2) * 8 + j],
                            LineRender.BitmapChar[(i + 3) * 8 + j],
                            LineRender.BitmapChar[(i + 4) * 8 + j],
                            LineRender.BitmapChar[(i + 5) * 8 + j],
                            LineRender.BitmapChar[(i + 6) * 8 + j],
                            LineRender.BitmapChar[(i + 7) * 8 + j],
                            i, i + 7
                            ));
                    }
                    else
                    {
                        asmLines.Add(String.Format("\tDC.B ${0:X2},${1:X2},${2:X2},${3:X2},${4:X2},${5:X2},${6:X2},${7:X2}  ; ${8:X2} - ${9:X2}",
                            CharacterRom.Characters[(i + 0) * 8 + j],
                            CharacterRom.Characters[(i + 1) * 8 + j],
                            CharacterRom.Characters[(i + 2) * 8 + j],
                            CharacterRom.Characters[(i + 3) * 8 + j],
                            CharacterRom.Characters[(i + 4) * 8 + j],
                            CharacterRom.Characters[(i + 5) * 8 + j],
                            CharacterRom.Characters[(i + 6) * 8 + j],
                            CharacterRom.Characters[(i + 7) * 8 + j],
                            i, i + 7
                            ));
                    }
                }
            }
            asmLines.Add("\tend");
            File.WriteAllLines("characterrom.asm", asmLines);
            File.WriteAllLines("characterrom.inc", incLines);
        }
    }
}
