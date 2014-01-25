﻿using System;
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


        public void Process()
        {
            PieceImage = new Image<Bgr, byte>(PieceImagePath).Resize(200, 400, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, true);

            // Convert the image to grayscale and filter out the noise
            GrayImage = PieceImage.Convert<Gray, Byte>().PyrDown().PyrUp();

            // Do canny filter
            CannyImage = GrayImage.Canny(170.0, 50.0);
        }

    }
}
