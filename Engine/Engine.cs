using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Engine
    {
        public LombardFFT m_FFT = new LombardFFT();

        public string BoardImagePath
        {
            get;
            set;
        }

        public Image<Bgr, Byte> BoardImage
        {
            get;private set;
        }
        public Image<Gray, Byte> GrayImage
        {
            get;private set;
        }
        public Image<Gray, Byte> CannyImage
        {
            get;private set;
        }
        public LineSegment2D[] Lines
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> LinesImage
        {
            get;
            private set;
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
        public Image<Bgr, Byte> GridBoxesImage
        {
            get;
            private set;
        }


        public Engine()
        {

        }

        public void Process()
        {
            // Build original image, downscaled slightly
            BoardImage = new Image<Bgr, byte>(BoardImagePath).Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, true);

            // Convert the image to grayscale and filter out the noise
            GrayImage = BoardImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter
            CannyImage = GrayImage.Canny(170.0, 50.0);

            // Do Edge finder
            Lines = CannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                50, //threshold
                20, //min Line width
                40 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            Board boardLineFind1 = new Board();
            boardLineFind1.BuildLineSets(Lines, Math.PI / 10.0, Math.PI / 50.0);

            // Make lines image
            Image<Bgr, Byte> linesImage = BoardImage.CopyBlank();
            foreach (LineSegment2D line in Lines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in boardLineFind1.HorizLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            foreach (LineSegment2D line in boardLineFind1.VertLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            LinesImage = linesImage;

            // Remove perspective from image
            RemovePerspective(boardLineFind1.GetBoardRegression());




            // Convert the image to grayscale and filter out the noise
            WarpedGrayImage = WarpedImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter
            WarpedCannyImage = WarpedGrayImage.Canny(160.0, 120.0);

            // Do Edge finder
            WarpedLines = WarpedCannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                50, //threshold
                20, //min Line width
                40 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            Board boardLineFind2 = new Board();
            boardLineFind2.BuildLineSets(WarpedLines, Math.PI / 120.0, Math.PI / 120.0);

            // Make lines image
            Image<Bgr, Byte> warpedLinesImage = WarpedImage.CopyBlank();
            Image<Gray, Byte> warpedGridLinesImage = WarpedGrayImage.CopyBlank();

            foreach (LineSegment2D line in WarpedLines)
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in boardLineFind2.HorizLines)
            {
                warpedGridLinesImage.Draw(line, new Gray(100), 1);
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            }
            foreach (LineSegment2D line in boardLineFind2.VertLines)
            {
                warpedGridLinesImage.Draw(line, new Gray(100), 1);
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            }
            WarpedLinesImage = warpedLinesImage;

            GridBoxesImage = WarpedImage.Copy();


            // Find grid boxes
            List<GridBox> boxes = new List<GridBox>();

            for (int hi1 = 0; hi1 < boardLineFind2.HorizLines.Length - 1; hi1++)
                for (int hi2 = hi1 + 1; hi2 < boardLineFind2.HorizLines.Length; hi2++)
                {
                    var lineH1 = boardLineFind2.HorizLines[hi1];
                    var lineH2 = boardLineFind2.HorizLines[hi2];

                    for (int vi1 = 0; vi1 < boardLineFind2.VertLines.Length - 1; vi1++)
                        for (int vi2 = vi1 + 1; vi2 < boardLineFind2.VertLines.Length; vi2++)
                        {
                            var lineV1 = boardLineFind2.VertLines[vi1];
                            var lineV2 = boardLineFind2.VertLines[vi2];

                            PointF[] points = FindQuad(lineH1, lineH2, lineV1, lineV2);

                            if (points != null)
                            {
                                boxes.Add(new GridBox(points));
                                DrawRectangle(GridBoxesImage, points, new Bgr(50, 200, 100), 2);
                            }
                        }
                }
            FilterOutBadBoxes(boxes);

        }

        private void FilterOutBadBoxes(List<PointF[]> boxes)
        {
            List<PointF[]> bs1 = new List<PointF[]>();

            bs1 = boxes.Where(x => x
        }

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
            double B = regression.B;
            double A = regression.A;
            double x1 = A * (300 / (B + 300));
            double x2 = A + ((400 - A) * (B / (B + 300)));

            PointF[] srcs = new PointF[4];
            srcs[0] = new PointF((float)x1, 0);
            srcs[1] = new PointF((float)x2, 0);
            srcs[2] = new PointF(400, 300);
            srcs[3] = new PointF(0, 300);

            PointF[] dsts = new PointF[4];
            dsts[0] = new PointF(0, 0);
            dsts[1] = new PointF(400, 0);
            dsts[2] = new PointF(400, 300);
            dsts[3] = new PointF(0, 300);

            HomographyMatrix mywarpmat = CameraCalibration.GetPerspectiveTransform(srcs, dsts);
            WarpedImage = BoardImage.WarpPerspective(mywarpmat, 400, 300,
                Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
                Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                GetBottomBorderColor(BoardImage));
        }

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
