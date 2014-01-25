using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class PieceFinder
    {
        Image<Bgr, byte> m_Image;
        Board.GridQuad[,] m_Grid;
        Image<Bgr, byte>[,] m_PieceImages = new Image<Bgr, byte>[8, 8];

        public Image<Bgr, byte>[,] PieceImages
        {
            get
            {
                return m_PieceImages;
            }
        }

        public PieceFinder(Image<Bgr, byte> image, Board.GridQuad[,] grid)
        {
            m_Image = image;
            m_Grid = grid;
        }

        public void GoFind()
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    FindPiece(x, y);
                }
        }

        private void FindPiece(int x, int y)
        {
            Board.GridQuad quad = m_Grid[x, y];
            float topSkew = ((quad.p[0].X + quad.p[1].X) - (quad.p[2].X + quad.p[3].X));

            PointF[] srcs = new PointF[4];
            srcs[0] = new PointF(quad.p[3].X, quad.p[0].Y - quad.Height);
            srcs[1] = new PointF(quad.p[2].X, quad.p[1].Y - quad.Height);
            srcs[2] = quad.p[2];
            srcs[3] = quad.p[3];

            PointF[] dsts = new PointF[4];
            dsts[0] = new PointF(0, 0);
            dsts[1] = new PointF(64, 0);
            dsts[2] = new PointF(64, 128);
            dsts[3] = new PointF(0, 128);

            HomographyMatrix warpMat = CameraCalibration.GetPerspectiveTransform(srcs, dsts);
            m_PieceImages[x, y] = m_Image.WarpPerspective(warpMat, 64, 128,
                Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
                Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                new Bgr(0, 0, 0));
        }

    }
}
