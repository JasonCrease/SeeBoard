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

        public Engine()
        {

        }

        public void Process()
        {
            // Build original image, downscaled slightly
            BoardImage = new Image<Bgr, byte>(BoardImagePath).Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);

            // Convert the image to grayscale and filter out the noise
            GrayImage = BoardImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter
            CannyImage = GrayImage.Canny(120.0, 80.0);

            // Do Edge finder
            Lines = CannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                80, //threshold
                20, //min Line width
                20 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            Board boardLineFind1 = new Board();
            boardLineFind1.BuildLineSets(Lines, Math.PI / 30.0, Math.PI / 10.0);

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
            WarpedCannyImage = GrayImage.Canny(120.0, 80.0);

            // Do Edge finder
            WarpedLines = WarpedCannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                20, //threshold
                20, //min Line width
                20 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            Board boardLineFind2 = new Board();
            boardLineFind2.BuildLineSets(Lines, Math.PI / 360.0, Math.PI / 360.0);

            // Make lines image

            Image<Bgr, Byte> warpedLinesImage = BoardImage.CopyBlank();
            foreach (LineSegment2D line in Lines)
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in m_Board.HorizLines)
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            foreach (LineSegment2D line in m_Board.VertLines)
                warpedLinesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            WarpedLinesImage = warpedLinesImage;
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
            WarpedImage = BoardImage.WarpPerspective(mywarpmat,
                Emgu.CV.CvEnum.INTER.CV_INTER_NN,
                Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                new Bgr(60, 40, 20));
        }
    }
}
