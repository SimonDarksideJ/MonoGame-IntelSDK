using System.Threading;

namespace SimpleLib
{
    public class UtilMCaptureSession : IVideoCapture
    {
        PXCMSession session;
        UtilMCapture uc;

        protected int width = 320;
        protected int height = 240;
        protected int DEVICE_ID = 0;
        protected int BufferSize;
        Thread thread;
        private bool capturing;
        protected byte[] frameRGBA;

        public byte[] FrameBGRA
        {
            get { return frameRGBA.ConvertBetweenBGRAandRGBA(width,height); }
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
            PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = 320, height = 240 };
            var DepthSize = new PXCMSizeU32() { width = 320, height = 240 };

            PXCMCapture.VideoStream.DataDesc.StreamDesc colourStrm = new PXCMCapture.VideoStream.DataDesc.StreamDesc()
            {
                format = PXCMImage.ColorFormat.COLOR_FORMAT_RGB32,
                sizeMin = DepthSize,
                sizeMax = DepthSize
            };

            PXCMCapture.VideoStream.DataDesc req = new PXCMCapture.VideoStream.DataDesc();

            req.streams[0] = colourStrm;

            uc = new UtilMCapture(session);

            uc.LocateStreams(ref req);

            thread = new Thread(
            new ThreadStart(StartCaptureStream));
            thread.Start();
        }

        public void StartCaptureStream()
        {
            PXCMScheduler.SyncPoint sp = null;
            PXCMImage image = null;
            capturing = true;
            while (capturing)
            {
                if (uc.QueryVideoStream(0).ReadStreamAsync(out image, out sp) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    break;
                }
                sp.Synchronize();

                PXCMImage.ImageData data;
                if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    PXCMImage.ImageInfo imageInfo = image.imageInfo;
                    width = (int)imageInfo.width;
                    height = (int)imageInfo.height;
                    BufferSize = width * height * 4;
                    frameRGBA = new byte[BufferSize];
                    frameRGBA = data.ToByteArray(0, BufferSize);
                    image.ReleaseAccess(ref data);
                }
            }
            sp.Dispose();
            image.Dispose();
            uc.Dispose();
            session.Dispose();
            Thread.Sleep(2000);
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }
    }
}
