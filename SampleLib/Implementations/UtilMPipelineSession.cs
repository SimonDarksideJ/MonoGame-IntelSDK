using System.Threading;

namespace SimpleLib
{
    class UtilMPipelineSession : IVideoCapture
    {
        PXCMSession session;
        UtilMPipeline pp;
        PXCMGesture gesture;

        protected int width = 320;
        protected int height = 240;
        protected int DEVICE_ID = 0;
        protected int BufferSize;
        Thread thread;
        private bool capturing;
        protected byte[] frameRGBA;

        public byte[] FrameBGRA
        {
            get { return frameRGBA.ConvertBetweenBGRAandRGBA(width, height); }
        }

        public byte[] FrameRGBA
        {
            get { return frameRGBA; }
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

        public void Initialise()
        {
            var sts = PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = 320, height = 240 };
            var DepthSize = new PXCMSizeU32() { width = 320, height = 240 };

            pp = new UtilMPipeline();
            pp.EnableGesture();
            pp.EnableVoiceRecognition();
            pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, (uint)Width, (uint)Height); // select the stream

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
            PXCMImage image = null;
            capturing = true;
            while (capturing)
            {
                if (!pp.AcquireFrame(true)) break;
                else
                {
                    gesture = pp.QueryGesture();
                    image = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH); // retrieve the sample

                    PXCMImage.ImageData data;
                    if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        PXCMImage.ImageInfo imageInfo = image.imageInfo;
                        width = (int)imageInfo.width;
                        height = (int)imageInfo.height;
                        BufferSize = width * height * 4;
                        frameRGBA = new byte[BufferSize];
                        frameRGBA = data.ToByteArray(0, BufferSize);
                        image.ReleaseAccess(ref data);
                    }
                    Helpers.GeoNodesHelper.CaptureGeonodes(gesture);
                    Helpers.GestureHelper.CaptureGestures(gesture);

                    pp.ReleaseFrame(); // go fetching the next sample
                }
            }
            image.Dispose();
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }


    }
}
