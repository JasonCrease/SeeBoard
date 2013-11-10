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
            const double MINLENGTH = 20.0;
            const double ANGLETOLHORIZ = Math.PI / 360.0;
            const double ANGLETOLVERT = Math.PI / 10.0;

            HorizLines = lines.Where(line => GetAngle(line) < ANGLETOLHORIZ && GetAngle(line) > -ANGLETOLHORIZ).Where(x => x.Length > MINLENGTH).OrderBy(x => x.P1.Y).ToArray();

            VertLines = lines.Where(line => GetAngle(line) > (Math.PI / 2) - ANGLETOLVERT || GetAngle(line) < -(Math.PI / 2) + ANGLETOLVERT).Where(x => x.Length > MINLENGTH).OrderBy(x => x.P1.X).ToArray();

            CalculateBoardAngle(VertLines);
        }

        private void CalculateBoardAngle(LineSegment2D[] VertLines)
        {

        }

        private static double GetAngle(LineSegment2D line)
        {
            return Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X);
        }
    }

}
