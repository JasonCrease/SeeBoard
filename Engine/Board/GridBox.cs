using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Engine.Board
{
    /// <summary>
    ///  Points must be clockwise from top-left
    /// </summary>
    public class GridQuad
    {
        public float Area
        {
            get
            {
                return 0.5f * Math.Abs(
                    ((p[2].X - p[0].X) * (p[3].Y - p[1].Y)) -
                    ((p[3].X - p[1].X) * (p[2].Y - p[0].Y))
                    );
            }
        }

        public PointF[] p;

        public GridQuad(System.Drawing.PointF[] points)
        {
            this.p = points;
        }

        public float Width
        {
            get
            {
                return Math.Max(p[1].X, p[2].X) - Math.Min(p[0].X, p[3].X);
            }
        }

        public PointF Centroid
        {
            get
            {
                return new PointF((p[0].X + p[1].X + p[2].X + p[3].X) / 4, (p[0].Y + p[1].Y + p[2].Y + p[3].Y) / 4);
            }
        }

        public float Height
        {
            get
            {
                return Math.Max(p[2].Y, p[3].Y) - Math.Min(p[0].Y, p[1].Y);
            }
        }
    }
}
