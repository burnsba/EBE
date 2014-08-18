using System;

namespace EBE.Core
{
    public static class Math
    {
        public static int IntegerPow(int x, int y)
        {
            switch (x)
            {
            case 0:
                switch (y)
                {
                    case 0:
                        return 1;
                    default:
                        return 0;
                }
            case 1:
                return 1;
            case 2:
                switch (y)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 2;
                    default:
                        return 1 << y;
                }
            case 3:
                switch (y)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 3;
                    case 3:
                        return 9;
                    case 4:
                        return 27;
                    case 5:
                        return 81;
                    default:
                        return (int)System.Math.Pow((double)x, (double)y);
                }
            case 4:
                switch (y)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 4;
                    case 2:
                        return 16;
                    case 3:
                        return 64;
                    default:
                        return (int)System.Math.Pow((double)x, (double)y);
                }
            case 5:
                switch (y)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 5;
                    case 2:
                        return 25;
                    case 3:
                        return 125;
                    default:
                        return (int)System.Math.Pow((double)x, (double)y);
                }
            default:
                return (int)System.Math.Pow((double)x, (double)y);
            }
        }
    }
}

