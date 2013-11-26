using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class BoardFinder
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

        internal void BuildLineSets(LineSegment2D[] vertLines, LineSegment2D[]  horizLines,
            double vertTol, double horizTol)
        {
            const double MINLENGTH = 30.0;
            //const double ANGLETOLHORIZ = ; //;
            //const double ANGLETOLVERT = ;  // Math.PI / 10.0;

            HorizLines = horizLines.Where(line => GetAngle(line) < horizTol && GetAngle(line) > -horizTol).
                Where(x => x.Length > MINLENGTH).
                OrderBy(x => x.P1.Y).ToArray();

            VertLines = vertLines.Where(line => GetAngle(line) > (Math.PI / 2) - vertTol || GetAngle(line) < -(Math.PI / 2) + vertTol).
                Where(x => x.Length > MINLENGTH).
                OrderBy(x => x.P1.X).ToArray();
        }

        internal OLSRegression GetBoardRegression()
        {
            List<FullLine> fullLines = new List<FullLine>();

            foreach (LineSegment2D lineSegment in VertLines)
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
                float m = (float)(p2.X - p1.X) / (float)(p2.Y - p1.Y);
                float c = (float)p1.X - ((float)p1.Y * m);

                FullLine fullLine = new FullLine(m, c);
                fullLines.Add(fullLine);
            }

            OLSRegression olsRegression = new OLSRegression();
            olsRegression.Perform(fullLines.ToArray());

            return olsRegression;
        }

        private static double GetAngle(LineSegment2D line)
        {
            return Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X);
        }
    }

    internal struct FullLine
    {
        internal float m;
        internal float c;

        internal FullLine(float m, float c)
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
