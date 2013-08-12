using System;
using System.Threading;

namespace SimpleLib
{
    class UtilMPipelineSession : IVideoCapture
    {
        CaptureType captureType;

        PXCMSession session;
        UtilMPipeline pp;

        PXCMGesture gesture;
        PXCMGesture.GeoNode[][] nodes;
        PXCMGesture.Gesture[] gestures;

        #region Interface properties
        protected int width = 320;
        protected int height = 240;
        protected int DEVICE_ID = 0;
        protected int BufferSize;
        Thread thread;
        private bool capturing;
        protected byte[] depthFrame;
        protected byte[] colourFrame;

        public byte[] DepthFrame
        {
            get { return depthFrame; }
        }

        public byte[] ColourFrame
        {
            get { return colourFrame; }
        }

        public bool Capturing
        {
            get { return capturing; }
        }

        public ThreadState State
        {
            get { return thread.ThreadState; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public PXCMGesture.GeoNode[][] Nodes
        {
            get { return nodes; }
        }

        public PXCMGesture.Gesture[] Gestures
        {
            get { return gestures; }
        }
        #endregion

        public void Initialise(CaptureType captureType = CaptureType.IMAGE_TYPE_DEPTH)
        {
            if (captureType == CaptureType.BOTH)
            {
                throw new NotSupportedException("This capture type only supports depth or colour and not both");
            }
            this.captureType = captureType;

            var sts = PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = 640, height = 480 };
            var DepthSize = new PXCMSizeU32() { width = 320, height = 240 };

            pp = new UtilMPipeline();
            pp.EnableGesture();
            //pp.EnableVoiceRecognition();
            if (captureType == CaptureType.IMAGE_TYPE_DEPTH) // select the stream
            {
                pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH); 
            }
            else if (captureType == CaptureType.IMAGE_TYPE_COLOUR)
            {
                pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32);
            }


            sts = session.CreateImpl<PXCMGesture>(PXCMGesture.CUID, out gesture);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                session.Dispose();
                return;
            }

            if (pp.Init())
            {
                thread = new Thread(new ThreadStart(StartCaptureStream));
                thread.Start();
            }

        }

        public void StartCaptureStream()
        {
            PXCMImage depthImage = null;
            PXCMImage colourImage = null;

            capturing = true;
            while (capturing)
            {
                if (!pp.AcquireFrame(true)) break;
                else
                {
                    gesture = pp.QueryGesture();
                    if (captureType == CaptureType.IMAGE_TYPE_DEPTH)
                    {
                        depthImage = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH); // retrieve the sample

                        depthFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(depthImage, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
                    }
                    else if (captureType == CaptureType.IMAGE_TYPE_COLOUR)
                    {
                        colourImage = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_COLOR); // retrieve the sample

                        colourFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(colourImage, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
                    }

                    nodes = Helpers.GeoNodesHelper.CaptureGeonodes(gesture);
                    gestures = Helpers.GestureHelper.CaptureGestures(gesture);

                    pp.ReleaseFrame(); // go fetching the next sample
                }
            }
            if (depthImage != null) depthImage.Dispose();
            if (colourImage != null) colourImage.Dispose();
            pp.Dispose();
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }


    }
}
