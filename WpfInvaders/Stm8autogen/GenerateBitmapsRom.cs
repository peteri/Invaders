using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Stm8autogen
{
    public static class GenerateBitmapsRom
    {
        static List<string> asmLines = new List<string>();
        static List<string> incLines = new List<string>();

        internal static void Generate(List<(string name, byte[] bitmap)> bitmaps)
        {
            asmLines.Add("stm8/");
            asmLines.Add(";=============================================");
            asmLines.Add("; Generated file from the WPF invaders");
            asmLines.Add("; Contains basic invaders, used to fill the");
            asmLines.Add("; fast alien user defined graphics.");
            asmLines.Add(";=============================================");
            incLines.Add(";=============================================");
            incLines.Add("; Generated include file for the invaders.");
            incLines.Add(";=============================================");
            asmLines.Add("\tsegment \'rom\'");
            List<string> labelNames = new List<string>();
            foreach (var bm in bitmaps)
            {
                for (int sh = 0; sh < 8; sh++)
                {
                    var name = $"{bm.name}_sh{sh}";
                    labelNames.Add(name);
                    asmLines.Add(name);
                    for (int j = 0; j < sh; j++)
                        asmLines.Add("\tdc.b\t %" + ByteToStr(0));
                    for (int n = 0; n < bm.bitmap.Length; n++)
                        asmLines.Add("\tdc.b\t %" + ByteToStr(bm.bitmap[n]));
                    for (int j = sh; j < 8; j++)
                        asmLines.Add("\tdc.b\t %" + ByteToStr(0));
                }
            }
            AddLabel("invader","lookup");
            foreach (var l in labelNames)
            {
                asmLines.Add("\tdc.w\t" + l);
            }
            asmLines.Add("\tend");
            File.WriteAllLines("aliensrom.asm", asmLines);
            File.WriteAllLines("aliensrom.inc", incLines);
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
            for (int i = 0; i < 8; i++)
            {
                retVal += (b & 0x80) == 0 ? "0" : "1";
                b = (byte)(b << 1);
            }
            return retVal;
        }

    }
}
