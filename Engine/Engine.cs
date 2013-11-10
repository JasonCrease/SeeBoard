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
        Board m_Board;

        public string BoardImagePath
        {
            get;
            set;
        }

        public Image<Bgr, Byte> BoardImage
        {
            get;
            private set;
        }

        public Image<Gray, Byte> GrayImage
        {
            get;
            private set;
        }

        public Image<Gray, Byte> CannyImage
        {
            get;
            private set;
        }

        public Image<Bgr, byte> WarpedImage
        {
            get;
            private set;
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
                40, //threshold
                40, //min Line width
                20 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            m_Board = new Board();
            m_Board.FindCorners(Lines);

            // Make lines image

            Image<Bgr, Byte> linesImage = BoardImage.CopyBlank();
            foreach (LineSegment2D line in Lines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Gray), 1);
            foreach (LineSegment2D line in m_Board.HorizLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
            foreach (LineSegment2D line in m_Board.VertLines)
                linesImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
            LinesImage = linesImage;


            // Warp image

            double x1 = m_Board.OlsRegression.A * 300 / m_Board.OlsRegression.B;
            double x2 = m_Board.OlsRegression.A + ((400 - m_Board.OlsRegression.A) * 300 / m_Board.OlsRegression.B);
            double x3 = m_Board.OlsRegression.A - ()

            PointF[] srcs = new PointF[4];
            srcs[0] = new PointF((float)x1, 0);
            srcs[1] = new PointF((float)x2, 0);
            srcs[2] = new PointF(400, 300);
            srcs[3] = new PointF(0, 300);

            PointF[] dsts = new PointF[4];
            dsts[0] = new PointF(0, 0);
            dsts[1] = new PointF(600, 0);
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
