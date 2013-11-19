using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public struct GridBox
    {
        public int W;
        public int H;
        public int X;
        public int Y;
        public int Area;

        public GridBox(int w, int h, int x, int y)
        {
            W = w;
            H = h;
            X = x;
            Y = y;
            Area = w * h;
        }
    }
}
