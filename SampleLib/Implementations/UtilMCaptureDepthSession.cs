/// Header
/// Util M Depth Sensor retrieval and processing
/// 
/// ** Note currently not recognising gestures



using System.Threading;

namespace SimpleLib
{
    public class UtilMCaptureDepthSession : IVideoCapture
    {
        PXCMSession session;
        UtilMCapture uc;
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

            var ColourSize = new PXCMSizeU32() { width = (uint)Width, height = (uint)Height };
            var DepthSize = new PXCMSizeU32() { width = (uint)Width, height = (uint)Height };

            session.CreateImpl<PXCMGesture>(PXCMGesture.CUID, out gesture);
            PXCMGesture.ProfileInfo pinfo;
            gesture.QueryProfile(0, out pinfo);

            uc = new UtilMCapture(session);

            uc.LocateStreams(ref pinfo.inputs);

            gesture.SetProfile(ref pinfo);

            thread = new Thread(
            new ThreadStart(StartCaptureStream));
            thread.Start();
        }

        public void StartCaptureStream()
        {
            PXCMImage[] images = new PXCMImage[PXCMCapture.VideoStream.STREAM_LIMIT];

            PXCMScheduler.SyncPoint[] sps = new PXCMScheduler.SyncPoint[2];

            capturing = true;
            while (capturing)
            {
                if (uc.ReadStreamAsync(images, out sps[0]) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    break;
                }

                gesture.ProcessImageAsync(images, out sps[1]);

                PXCMScheduler.SyncPoint.SynchronizeEx(sps);

                PXCMImage.ImageData data;
                if (images[0].AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    PXCMImage.ImageInfo imageInfo = images[0].imageInfo;
                    width = (int)imageInfo.width;
                    height = (int)imageInfo.height;
                    BufferSize = width * height * 4;
                    frameRGBA = new byte[BufferSize];
                    frameRGBA = data.ToByteArray(0, BufferSize);
                    images[0].ReleaseAccess(ref data);
                }
                Helpers.GeoNodesHelper.CaptureGeonodes(gesture);
                Helpers.GestureHelper.CaptureGestures(gesture);
            }
            PXCMScheduler.SyncPoint.Dispose(sps);
            PXCMImage.Dispose(images);
            uc.Dispose();
            gesture.Dispose();
            session.Dispose();
            Thread.Sleep(2000);
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }
    }
}
