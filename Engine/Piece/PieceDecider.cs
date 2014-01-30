using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine.Piece
{
    public class PieceDecider
    {
        public string PieceImagePath { get; set; }

        public Image<Bgr, Byte> PieceImage
        {
            get;
            set;
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
        public Image<Gray, Byte> HueImage
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> MaskedImage
        {
            get;
            private set;
        }


        public void Process()
        {
            PieceImage = new Image<Bgr, byte>(PieceImagePath).Resize(200, 400, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, false);

            // Convert the image to grayscale and filter out the noise
            GrayImage = PieceImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            HueImage = GrayImage.CopyBlank();

            // Do hsv histogram
            Image<Hsv, Byte> hsvImg = PieceImage.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] channels = hsvImg.Split();
            channels[0] = channels[0].InRange(new Gray(10), new Gray(18));
            channels[1] = channels[1].InRange(new Gray(100), new Gray(255));
            channels[2] = channels[2].InRange(new Gray(20), new Gray(255));

            HueImage = channels[1].And(channels[0]);

            for (int i = 0; i < 4; i++)
            {
               HueImage = HueImage.Erode(1);
               HueImage = HueImage.Dilate(1);
            }

            //HueImage = HueImage.SmoothBlur(5, 5);


            // Masked image
            MaskedImage = PieceImage.Xor(new Bgr(0, 0, 0), HueImage);

            // Do canny filter
            CannyImage = MaskedImage.Canny(170.0, 50.0);
        }

    }
}
