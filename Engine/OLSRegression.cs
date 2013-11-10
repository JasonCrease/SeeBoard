using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    internal class OLSRegression
    {
        internal double R2 { get; private set; }
        internal double A { get; private set; }
        internal double B { get; private set; }

        internal OLSRegression()
        {

        }

        public void Perform(FullLine[] data)
        {
            double n = data.Length;
            double Sx = data.Sum(x => x.m);
            double Sy = data.Sum(x => x.c);
            double Sxx = data.Sum(x => x.m * x.m);
            double Syy = data.Sum(x => x.c * x.c);
            double Sxy = data.Sum(x => x.m * x.c);

            B = ((n * Sxy) - (Sx * Sy)) /
                ((n * Sxx) - (Sx * Sx));
            A = (Sy / n) - (B * Sx / n);
        }
    }
}
