using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CASimulator
{
    /// <summary>
    /// Contains a number of preset settings for popular C.A. implementations.
    /// </summary>
    public static class CAPresets
    {
        public static List<Color> ColorsBinary { get; private set; }
        public static List<Color> ColorsWireWorld { get; private set; }
        public static List<Color> ColorsHighLife { get; private set; }
        public static List<Color> ColorsRainbow { get; private set; }
        public static List<Color> ColorsGrayscale8 { get; private set; }
        public static List<Color> ColorsGrayscale32 { get; private set; }
        public static List<Color> ColorsGrayscale256 { get; private set; }

        public const string RulesLife = "tm3=1|tm2=x";
        public const string RulesHighLife = "tm3=1|tm6=1|tm2=x";
        public const string RulesLiveFreeOrDie = "tm2=1|tm0=x";
        public const string RulesCustomLinkBreaker = "tb2=1|tb1=x";
        public const string Rules90 = "01011010";

        static CAPresets()
        {
            ColorsBinary = new List<Color>()
            {
                Colors.White,
                Colors.Black
            };

            ColorsWireWorld = new List<Color>()
            {
                Colors.Black,
                Colors.Red,
                Colors.Yellow,
                Colors.Blue
            };

            ColorsHighLife = new List<Color>()
            {
                Colors.Black,
                Colors.OrangeRed,
            };

            ColorsGrayscale8 = new List<Color>();
            ColorsGrayscale32 = new List<Color>();
            ColorsGrayscale256 = new List<Color>();
            for (int i = 0; i < 8; i++)
            {
                int diff = 32; //256 divided by 8.
                byte value = (byte)(diff * i);

                ColorsGrayscale8.Add(
                    Color.FromRgb(value, value, value));
            }
            for (int i = 0; i < 32; i++)
            {
                int diff = 8; //256 divided by 32.
                byte value = (byte)(diff * i);

                ColorsGrayscale32.Add(
                    Color.FromRgb(value, value, value));
            }
            for (int i = 0; i < 256; i++)
            {
                byte ibyte = (byte)i;

                ColorsGrayscale256.Add(
                    Color.FromRgb(ibyte, ibyte, ibyte));
            }
            ColorsRainbow = new List<Color>()
            {
                Colors.Red,
                Colors.Blue,
                Colors.Gold,
                Colors.Green,
                Colors.Purple,
                Colors.Brown,
                Colors.Pink
            };
        }
    }
}
