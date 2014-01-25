using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Board
{
    internal class OLSRegression
    {
        internal float R2 { get; private set; }
        internal float A { get; private set; }
        internal float B { get; private set; }

        internal OLSRegression()
        {

        }

        public void Perform(FullLine[] data)
        {
            float n = data.Length;
            float Sx = data.Sum(x => x.m);
            float Sy = data.Sum(x => x.c);
            float Sxx = data.Sum(x => x.m * x.m);
            float Syy = data.Sum(x => x.c * x.c);
            float Sxy = data.Sum(x => x.m * x.c);

            B = ((n * Sxy) - (Sx * Sy)) /
                ((n * Sxx) - (Sx * Sx));
            A = (Sy / n) - (B * Sx / n);
        }
    }
}
