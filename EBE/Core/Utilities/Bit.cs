using System;

namespace EBE.Core.Utilities
{
    public static class Bit
    {
        public static byte[] GetBytes(string hex)
        {
            //http://stackoverflow.com/questions/321370/convert-hex-string-to-byte-array

            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        private static readonly uint[] HighestBit_b = {0x2, 0xC, 0xF0, 0xFF00, 0xFFFF0000};
        private static readonly uint[] HighestBit_S = {1, 2, 4, 8, 16};


        //https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
        public static int GetHighestBit(uint v)
        {
            int i;

            uint r = 0; // result of log2(v) will go here
            for (i = 4; i >= 0; i--) // unroll for speed...
            {
                if ((v & HighestBit_b[i]) > 0)
                {
                    v >>= (int)HighestBit_S[i];
                    #pragma warning disable 0675
                    r |= HighestBit_S[i];
                    #pragma warning restore 0675
                } 
            }

            return (int)r;
        }
    }
}

