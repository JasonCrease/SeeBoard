using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

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

        public LineSegment2D[] Lines
        {
            get;
            private set;
        }

        public Image<Bgr, Byte> LinesImage
        {
            get
            {
                Image<Bgr, Byte> lineImage = BoardImage.CopyBlank();
                foreach (LineSegment2D line in Lines)
                    lineImage.Draw(line, new Bgr(System.Drawing.Color.Black), 1);
                foreach (LineSegment2D line in m_Board.HorizLines)
                    lineImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);
                foreach (LineSegment2D line in m_Board.VertLines)
                    lineImage.Draw(line, new Bgr(System.Drawing.Color.Green), 1);
                return lineImage;
            }
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
                20, //threshold
                10, //min Line width
                10 //gap between lines
                )[0]; //Get the lines from the first channel

            // Find board
            m_Board = new Board();
            m_Board.FindCorners(Lines);

        }
    }
}
