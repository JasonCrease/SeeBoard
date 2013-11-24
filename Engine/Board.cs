using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Board
    {
        public Image<Bgr, Byte> BoardImage
        {
            get; set;
        }
        public Image<Gray, Byte> GrayImage
        {
            get; private set;
        }
        public Image<Gray, Byte> CannyImage
        {
            get; private set;
        }
        public LineSegment2D[] Lines
        {
            get; private set;
        }
        public Image<Bgr, Byte> LinesImage
        {
            get; private set;
        }

        public Image<Bgr, byte> WarpedImage
        {
            get; private set;
        }
        public Image<Gray, byte> WarpedGrayImage
        {
            get; private set;
        }
        public Image<Gray, Byte> WarpedCannyImage
        {
            get; private set;
        }
        public LineSegment2D[] WarpedLines
        {
            get; private set;
        }
        public Image<Bgr, Byte> WarpedLinesImage
        {
            get; private set;
        }
        public Image<Bgr, Byte> GridQuadsImage
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> BoardImageWithBoxes
        {
            get;
            private set;
        }

        public void FindBoard()
        {
            // Convert the image to grayscale and filter out the noise
            GrayImage = BoardImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter
            CannyImage = GrayImage.Canny(170.0, 50.0);

            // Do Edge finder
            Lines = CannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                50, //threshold
                30, //min Line width
                20 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            BoardFinder boardLineFinder1 = new BoardFinder();
            boardLineFinder1.BuildLineSets(Lines, Math.PI / 10.0, Math.PI / 40.0);

            // Make lines image
            Image<Bgr, Byte> linesImage = BoardImage.CopyBlank();
            foreach (LineSegment2D line in Lines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in boardLineFinder1.HorizLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            foreach (LineSegment2D line in boardLineFinder1.VertLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            LinesImage = linesImage;

            // Remove perspective from image
            RemovePerspective(boardLineFinder1.GetBoardRegression());

            // Convert the warped image to grayscale and filter out the noise
            WarpedGrayImage = WarpedImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter on warped image
            WarpedCannyImage = WarpedGrayImage.Canny(160, 90);

            // Do Edge finder on warped image
            WarpedLines = WarpedCannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360, //Angle resolution measured in radians.
                50, //threshold
                20, //min Line width
                40 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            BoardFinder boardLineFinder2 = new BoardFinder();
            boardLineFinder2.BuildLineSets(WarpedLines, Math.PI / 100.0, Math.PI / 100.0);

            // Make lines image
            Image<Bgr, Byte> warpedLinesImage = WarpedImage.CopyBlank();
            Image<Gray, Byte> warpedGridLinesImage = WarpedGrayImage.CopyBlank();

            foreach (LineSegment2D line in WarpedLines)
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in boardLineFinder2.HorizLines)
            {
                warpedGridLinesImage.Draw(line, new Gray(100), 1);
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            }
            foreach (LineSegment2D line in boardLineFinder2.VertLines)
            {
                warpedGridLinesImage.Draw(line, new Gray(100), 1);
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            }
            WarpedLinesImage = warpedLinesImage;

            GridQuadsImage = WarpedImage.Copy();
            BoardImageWithBoxes = BoardImage.Copy();

            try
            {
                // Find grid quads

                List<GridQuad> quads = FindBoardQuads(boardLineFinder2);
                GridQuad[] quadsToChase = FilterOutBadQuads(quads);
                GridQuad[,] quadsToDraw = ChaseThe64Quads(quadsToChase);

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        if (quadsToDraw[i, j] != null)
                            DrawRectangle(GridQuadsImage, quadsToDraw[i, j].p, new Bgr(120, 190, 20), 2);
                    }

                DrawRectangle(GridQuadsImage, medianBox.p, new Bgr(220, 250, 20), 2);

                // Set up return array


                Quads = new GridQuad[8, 8];

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        if (quadsToDraw[i, j] != null)
                        {
                            Quads[i, j] = new GridQuad(quadsToDraw[i, j].p);
                            m_InverseWarpMatrix.ProjectPoints(Quads[i, j].p);
                            DrawRectangle(BoardImageWithBoxes, Quads[i, j].p, new Bgr(120, 190, 20), 2);
                        }
                    }

            }
            catch { }
        }

        public GridQuad[,] Quads { get; private set; }

        private GridQuad[,] ChaseThe64Quads(GridQuad[] quadsToChase)
        {
            List<GridQuad> quadTrain = new List<GridQuad>();
            GridQuad nextQuad = medianBox;

            // Go to the top right through the quads

            while (true)
            {
                GridQuad guessBox = EstimateQuadPosition(nextQuad, Dir.W);
                GridQuad guessQuad = GetMostSimilarQuad(guessBox, quadsToChase);

                if (guessQuad == null) break;
                nextQuad = guessQuad;
            }
            while (true)
            {
                GridQuad guessBox = EstimateQuadPosition(nextQuad, Dir.N);
                GridQuad guessQuad = GetMostSimilarQuad(guessBox, quadsToChase);

                if (guessQuad == null) break;
                nextQuad = guessQuad;
            }

            // We're now at the top right.  We can now iterate across the 8x8 grid to find all the 64 quads

            GridQuad[,] grid = new GridQuad[8, 8];

            grid[0, 0] = nextQuad;

            for (int row = 0; row < 8; row++)
            {
                if (row > 0)
                {
                    GridQuad guessBox = EstimateQuadPosition(grid[row - 1, 0], Dir.S);
                    nextQuad = GetMostSimilarQuad(guessBox, quadsToChase);
                    if (nextQuad == null) break;

                    grid[row, 0] = nextQuad;
                }

                for (int column = 1; column < 8; column++)
                {
                    if (nextQuad == null) break;
                    GridQuad guessBox = EstimateQuadPosition(nextQuad, Dir.E);
                    nextQuad = GetMostSimilarQuad(guessBox, quadsToChase);
                    grid[row, column] = nextQuad;
                }
            }

            return grid;
        }

        /// <summary>
        /// Find the quad most similar to quad, or null if no similar quads are found
        /// </summary>
        private GridQuad GetMostSimilarQuad(GridQuad quad, GridQuad[] quadsToChase)
        {
            GridQuad mostSimilarQuad = null;
            float mostSimilarityValue = 20f;

            foreach(GridQuad testQuad in quadsToChase)
            {
                if (QuadTotalSquaredError(testQuad, quad) < mostSimilarityValue)
                {
                    mostSimilarQuad = testQuad;
                    mostSimilarityValue = QuadTotalSquaredError(testQuad, quad);
                }
            }
            
            return mostSimilarQuad;
        }

        /// <summary>
        ///  Find total squared error between quads
        /// </summary>
        private float QuadTotalSquaredError(GridQuad x, GridQuad y)
        {
            return 
                    Math.Abs(x.p[0].X - y.p[0].X) + Math.Abs(x.p[0].Y - y.p[0].Y) +
                    Math.Abs(x.p[1].X - y.p[1].X) + Math.Abs(x.p[1].Y - y.p[1].Y) +
                    Math.Abs(x.p[2].X - y.p[2].X) + Math.Abs(x.p[2].Y - y.p[2].Y) +
                    Math.Abs(x.p[3].X - y.p[3].X) + Math.Abs(x.p[3].Y - y.p[3].Y);
        }

        /// <summary>
        /// Returns a quad the same as quad but proportionally in direction dir.
        /// </summary>
        private GridQuad EstimateQuadPosition(GridQuad quad, Dir dir)
        {
            PointF p0 = new PointF(quad.p[0].X, quad.p[0].Y);
            PointF p1 = new PointF(quad.p[1].X, quad.p[1].Y);
            PointF p2 = new PointF(quad.p[2].X, quad.p[2].Y);
            PointF p3 = new PointF(quad.p[3].X, quad.p[3].Y);

            if (dir == Dir.S)
            {
                p0.Y += quad.Height;
                p1.Y += quad.Height;
                p2.Y += quad.Height;
                p3.Y += quad.Height;
            }
            else if (dir == Dir.N)
            {
                p0.Y -= quad.Height;
                p1.Y -= quad.Height;
                p2.Y -= quad.Height;
                p3.Y -= quad.Height;
            }
            else if (dir == Dir.E)
            {
                p0.X += quad.Width;
                p1.X += quad.Width;
                p2.X += quad.Width;
                p3.X += quad.Width;
            }
            else if (dir == Dir.W)
            {
                p0.X -= quad.Width;
                p1.X -= quad.Width;
                p2.X -= quad.Width;
                p3.X -= quad.Width;
            }

            return new GridQuad(new PointF[] {p0, p1, p2, p3});
        }

        private enum Dir { N, E, S, W }


        private List<GridQuad> FindBoardQuads(BoardFinder boardLineFinder)
        {
            List<GridQuad> quads = new List<GridQuad>();

            for (int hi1 = 0; hi1 < boardLineFinder.HorizLines.Length - 1; hi1++)
                for (int hi2 = hi1 + 1; hi2 < boardLineFinder.HorizLines.Length; hi2++)
                {
                    var lineH1 = boardLineFinder.HorizLines[hi1];
                    var lineH2 = boardLineFinder.HorizLines[hi2];

                    for (int vi1 = 0; vi1 < boardLineFinder.VertLines.Length - 1; vi1++)
                        for (int vi2 = vi1 + 1; vi2 < boardLineFinder.VertLines.Length; vi2++)
                        {
                            var lineV1 = boardLineFinder.VertLines[vi1];
                            var lineV2 = boardLineFinder.VertLines[vi2];

                            PointF[] points = FindQuad(lineH1, lineH2, lineV1, lineV2);

                            if (points != null)
                            {
                                quads.Add(new GridQuad(points));
                            }
                        }
                }

            return quads;
        }

        private GridQuad[] FilterOutBadQuads(List<GridQuad> quads)
        {
            const float MAXAREA = 400f * 300f / 64f;
            const float MINAREA = (400f * 300f / 64f) / 5f;

            const float MAXWIDTH = 400f / 8f;
            const float MINWIDTH = (400f / 8f) / 1.7f;

            const float MAXHEIGHT = 300f / 8f;
            const float MINHEIGHT = (300f / 8f) / 3f;

            var bs1 = quads.Where(x => x.Area < MAXAREA && x.Area > MINAREA);
            var bs2 = bs1.Where(x => x.Width < MAXWIDTH && x.Width > MINWIDTH);
            var bs3 = bs2.Where(x => x.Height < MAXHEIGHT && x.Height > MINHEIGHT);
            var bs4 = bs3.Where(x => x.Height < x.Width * 1.7f && x.Width < x.Height * 3f);
            var bs5 = bs4.OrderBy(x => x.Area).ToArray();

            float minDiffToBox64Ago = 99999f;
            int indexAtMinDiff = 999;

            if(bs5.Length <= 64) throw new ApplicationException(
                string.Format("Found {0} quads, of which only {1} are plausible.", bs1.Count(), bs5.Length));

            for (int i = 64; i < bs5.Length; i++)
            {
                float diffToBox64Ago = bs5[i].Area - bs5[i - 64].Area;
                if (diffToBox64Ago < minDiffToBox64Ago)
                {
                    indexAtMinDiff = i;
                    minDiffToBox64Ago = diffToBox64Ago;
                }
            }

            float widthMaxError = 6f;
            float heightMaxError = 5f;

            medianBox = bs5[indexAtMinDiff - 32];
            var bsfinal = bs5.Where(x => x.Width > medianBox.Width - widthMaxError && x.Width < medianBox.Width + widthMaxError &&
                                         x.Height > medianBox.Height - heightMaxError && x.Height < medianBox.Height + heightMaxError)
                .OrderBy(b => b.Area)
                .ToArray();

            return bsfinal;
        }

        GridQuad medianBox;

        private void DrawRectangle(Image<Bgr, byte> image, PointF[] ps, Bgr bgr, int thickness)
        {
            image.Draw(new LineSegment2DF(ps[0], ps[1]), bgr, thickness);
            image.Draw(new LineSegment2DF(ps[1], ps[2]), bgr, thickness);
            image.Draw(new LineSegment2DF(ps[2], ps[3]), bgr, thickness);
            image.Draw(new LineSegment2DF(ps[3], ps[0]), bgr, thickness);
        }

        private PointF[] FindQuad(LineSegment2D lineH1, LineSegment2D lineH2, LineSegment2D lineV1, LineSegment2D lineV2)
        {
            PointF[] ps = new PointF[4];

            bool b0 = FindLineSegmentIntersection(lineH1.P1, lineH1.P2, lineV1.P1, lineV1.P2);
            ps[0] = new PointF(lineSegmentIntersectX, lineSegmentIntersectY);
            bool b1 = FindLineSegmentIntersection(lineH1.P1, lineH1.P2, lineV2.P1, lineV2.P2);
            ps[1] = new PointF(lineSegmentIntersectX, lineSegmentIntersectY);
            bool b2 = FindLineSegmentIntersection(lineH2.P1, lineH2.P2, lineV2.P1, lineV2.P2);
            ps[2] = new PointF(lineSegmentIntersectX, lineSegmentIntersectY);
            bool b3 = FindLineSegmentIntersection(lineH2.P1, lineH2.P2, lineV1.P1, lineV1.P2);
            ps[3] = new PointF(lineSegmentIntersectX, lineSegmentIntersectY);

            if (b0 && b1 && b2 && b3) return ps;

            return null;
        }

        public static bool DoLinesIntersect(Point lineA1, Point lineA2, Point lineB1, Point lineB2)
        {
            return CrossProduct(lineA1, lineA2, lineB1) !=
                   CrossProduct(lineA1, lineA2, lineB2) ||
                   CrossProduct(lineB1, lineB2, lineA1) !=
                   CrossProduct(lineB1, lineB2, lineA2);
        }

        public static double CrossProduct(Point p1, Point p2, Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
        }


        float lineSegmentIntersectX, lineSegmentIntersectY;

        private bool FindLineSegmentIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            return FindLineSegmentIntersection(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        private bool FindLineSegmentIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            float x12 = x1 - x2;
            float x34 = x3 - x4;
            float y12 = y1 - y2;
            float y34 = y3 - y4;

            float c = x12 * y34 - y12 * x34;

            if (Math.Abs(c) < 0.01)
            {
                // No intersection
                return false;
            }
            else
            {
                // Intersection
                float a = x1 * y2 - y1 * x2;
                float b = x3 * y4 - y3 * x4;

                lineSegmentIntersectX = (a * x34 - b * x12) / c;
                lineSegmentIntersectY = (a * y34 - b * y12) / c;

                return true;
            }
        }


        private void RemovePerspective(OLSRegression regression)
        {
            float tBit = 300;
            float B = regression.B;
            float A = regression.A;
            float x1 = A * (tBit / (B + tBit));
            float x2 = A + ((400 - A) * (B / (B + tBit)));

            PointF[] srcs = new PointF[4];
            srcs[0] = new PointF((float)x1, 0);
            srcs[1] = new PointF((float)x2, 0);
            srcs[2] = new PointF(400, tBit);
            srcs[3] = new PointF(0, tBit);

            PointF[] dsts = new PointF[4];
            dsts[0] = new PointF(0, 0);
            dsts[1] = new PointF(400, 0);
            dsts[2] = new PointF(400, 300);
            dsts[3] = new PointF(0, 300);

            HomographyMatrix warpMat = CameraCalibration.GetPerspectiveTransform(srcs, dsts);
            WarpedImage = BoardImage.WarpPerspective(warpMat, 400, 300,
                Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
                Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                GetBottomBorderColor(BoardImage));

            m_InverseWarpMatrix = CameraCalibration.GetPerspectiveTransform(dsts, srcs);
            //WarpedImage = WarpedImage.WarpPerspective(m_InverseWarpMatrix, 400, 300,
            //    Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
            //    Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
            //    GetBottomBorderColor(BoardImage));
        }

        private HomographyMatrix m_InverseWarpMatrix;

        private Bgr GetBottomBorderColor(Image<Bgr, byte> image )
        {
            int totalB = 0;
            int totalG = 0;
            int totalR = 0;
            int width = image.Width;
            int height = image.Height;

            for (int i = 0; i < image.Width; i++)
            {
                totalB += image.Data[height - 1, i, 0];
                totalG += image.Data[height - 1, i, 1];
                totalR += image.Data[height - 1, i, 2];
            }

            return new Bgr(totalB / width, totalG / width, totalR / width);
        }
    }
}
