using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Board
    {
        public LineSegment2D[] HorizLines
        {
            get;
            private set;
        }
        public LineSegment2D[] VertLines
        {
            get;
            private set;
        }

        internal void FindCorners(LineSegment2D[] lines)
        {
            const double MINLENGTH = 2.0;
            const double ANGLETOLHORIZ = Math.PI / 360.0;
            const double ANGLETOLVERT = Math.PI / 8.0;

            HorizLines = lines.Where(line => GetAngle(line) < ANGLETOLHORIZ && GetAngle(line) > -ANGLETOLHORIZ).
                Where(x => x.Length > MINLENGTH).
                OrderBy(x => x.P1.X).ToArray();

            VertLines = lines.Where(line => GetAngle(line) > (Math.PI / 2) - ANGLETOLVERT || GetAngle(line) < -(Math.PI / 2) + ANGLETOLVERT).
                Where(x => x.Length > MINLENGTH).
                OrderBy(x => x.P1.Y).ToArray();

            CalculateBoardAngle(VertLines);
        }

        private void CalculateBoardAngle(LineSegment2D[] vertLines)
        {
            List<FullLine> lines = new List<FullLine>();
            StringBuilder sb = new StringBuilder();

            foreach (LineSegment2D lineSegment in vertLines)
            {
                // Set lowest point as p1
                var p1 = lineSegment.P1;
                var p2 = lineSegment.P2;
                if (p1.Y > p2.Y)
                {
                    p2 = lineSegment.P1;
                    p1 = lineSegment.P2;
                }

                // Calculate crossing of x-axis (call it c)
                double m = (double)(p2.X - p1.X) / (double)(p2.Y - p1.Y);
                double c = (double)p1.X - ((double)p1.Y * m);

                FullLine fullLine = new FullLine(m, c);
                lines.Add(fullLine);

                sb.AppendLine(fullLine.ToString());
            }

            OlsRegression = new OLSRegression();
            OlsRegression.Perform(lines.ToArray());

            string s = sb.ToString();
        }

        internal OLSRegression OlsRegression { get; private set; }

        private static double GetAngle(LineSegment2D line)
        {
            return Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X);
        }
    }

    internal struct FullLine
    {
        internal double m;
        internal double c;

        internal FullLine(double m, double c)
        {
            this.m = m;
            this.c = c;
        }
        public override string ToString()
        {
            return m.ToString() + "," + c.ToString();
        }
    }

}
