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

        public Engine()
        {

        }

        public void Process()
        {
            // Build original image, downscaled slightly
            BoardImage = new Image<Bgr, byte>(BoardImagePath).Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);

            // Convert the image to grayscale and filter out the noise
            GrayImage = BoardImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // 
        }
    }
}
