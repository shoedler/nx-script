using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxScript.src
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int max)
        {
            return string.IsNullOrEmpty(s) ? s :
            s.Length <= max ? s :
                string.Concat(s.AsSpan(0, max), "...");
        }

        public static string InRed(this string s)
        {
            return StyleRed + s + Reset;
        }

        public static string InGreen(this string s)
        {
            return StyleGreen + s + Reset;
        }

        public static string InBlue(this string s)
        {
            return StyleBlue + s + Reset;
        }

        public static string InMagenta(this string s)
        {
            return StyleMagenta + s + Reset;
        }

        public static string InCyan(this string s)
        {
            return StyleCyan + s + Reset;
        }

        public static string InWhite(this string s)
        {
            return StyleWhite + s + Reset;
        }

        public static string InYellow(this string s)
        {
            return StyleYellow + s + Reset;
        }

        public static string Bold(this string s)
        {
            return StyleBold + s + Reset;
        }

        public static string Underlined(this string s)
        {
            return StyleUnderline + s + Reset;
        }

        public static string ColorSwap(this string s)
        {
            return StyleReverse + s + Reset;
        }

        public static string Dimmed(this string s)
        {
            return StyleDim + s + Reset;
        }

        public static string WithStyle(this string s, string style)
        {
            return $"{style}{s}{Reset}";
        }

        public const char Esc = '\x1b';
        public const string Reset = "\x1b[0m";
        public const string StyleBright = "\x1b[1m";
        public const string StyleDim = "\x1b[2m";
        public const string StyleUnderscore = "\x1b[4m";
        public const string StyleBlink = "\x1b[5m";
        public const string StyleReverse = "\x1b[7m";
        public const string StyleHidden = "\x1b[8m";

        public const string StyleBlack = "\x1b[30m";
        public const string StyleRed = "\x1b[31m";
        public const string StyleGreen = "\x1b[32m";
        public const string StyleYellow = "\x1b[33m";
        public const string StyleBlue = "\x1b[34m";
        public const string StyleMagenta = "\x1b[35m";
        public const string StyleCyan = "\x1b[36m";
        public const string StyleWhite = "\x1b[37m";

        public const string StyleBGBlack = "\x1b[40m";
        public const string StyleBGRed = "\x1b[41m";
        public const string StyleBGGreen = "\x1b[42m";
        public const string StyleBGYellow = "\x1b[43m";
        public const string StyleBGBlue = "\x1b[44m";
        public const string StyleBGMagenta = "\x1b[45m";
        public const string StyleBGCyan = "\x1b[46m";
        public const string StyleBGWhite = "\x1b[47m";

        public const string StyleBold = "\u001b[1m";
        public const string StyleUnderline = "\u001b[4m";
        public const string StyleReversed = "\u001b[7m";
    }
}
