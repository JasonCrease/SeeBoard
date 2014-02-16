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
            channels[2] = channels[2].InRange(new Gray(0), new Gray(200));

            //CvInvoke.cvReshape(hsvImg, output,);

            //CvInvoke.cvKMeans2()


            // Do k-means clustering

            Matrix<float> samples = new Matrix<float>(hsvImg.Rows * hsvImg.Cols, 1, 3);
            Matrix<int> finalClusters = new Matrix<int>(hsvImg.Rows * hsvImg.Cols, 1);

            for (int y = 0; y < hsvImg.Rows; y++)
            {
                for (int x = 0; x < hsvImg.Cols; x++)
                {
                    samples.Data[y + x * hsvImg.Rows, 0] = (float)hsvImg[y, x].Hue;
                    samples.Data[y + x * hsvImg.Rows, 1] = (float)hsvImg[y, x].Satuation;
                    samples.Data[y + x * hsvImg.Rows, 2] = (float)hsvImg[y, x].Value;
                }
            }

            MCvTermCriteria term = new MCvTermCriteria(100, 0.5);
            term.type = Emgu.CV.CvEnum.TERMCRIT.CV_TERMCRIT_ITER | Emgu.CV.CvEnum.TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = 3;
            int attempts = 4;
            Matrix<Single> centers = new Matrix<Single>(clusterCount, hsvImg.Rows * hsvImg.Cols);
            CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, Emgu.CV.CvEnum.KMeansInitType.PPCenters, IntPtr.Zero, IntPtr.Zero);

            MaskedImage = new Image<Bgr, byte>(hsvImg.Size);
            Bgr[] clusterColors = new Bgr[4];
            clusterColors[0] = new Bgr(100, 0, 200);
            clusterColors[1] = new Bgr(200, 0, 200);
            clusterColors[2] = new Bgr(100, 200, 0);
            clusterColors[3] = new Bgr(200, 50, 50);

            for (int y = 0; y < hsvImg.Rows; y++)
            {
                for (int x = 0; x < hsvImg.Cols; x++)
                {
                    //if (finalClusters[y + x * hsvImg.Rows, 0] == 0)
                    //{
                        PointF p = new PointF(x, y);
                        Bgr c = clusterColors[finalClusters[y + x * hsvImg.Rows, 0]];
                        //MaskedImage.Data[y, x, 1] = 200; // 
                        MaskedImage.Draw(new CircleF(p, 0.4f), c, 1);
                    //}
                }
            }


            HueImage = channels[1].And(channels[0]).And(channels[2]);

            for (int i = 0; i < 0; i++)
            {
                HueImage = HueImage.Erode(1);
                HueImage = HueImage.Dilate(1);
            }

            //HueImage = HueImage.SmoothBlur(5, 5);


            // Masked image
            //MaskedImage = PieceImage.Xor(new Bgr(0, 0, 0), HueImage);

            //MaskedImage = new Image<Bgr, byte>(new Image<Gray, byte>[] { channels[0], channels[1], channels[2] });

            // Do canny filter
            CannyImage = MaskedImage.Canny(170.0, 50.0);
        }

    }
}
