/// Header
/// Util M Depth Sensor retrieval and processing
/// 
/// ** Note currently not recognising gestures



using System;
using System.Threading;

namespace SimpleLib
{
    public class UtilMCaptureDepthSession : IVideoCapture
    {
        CaptureType captureType;

        PXCMSession session;
        UtilMCapture uc;

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
            if (captureType == CaptureType.BOTH || captureType == CaptureType.IMAGE_TYPE_COLOUR)
            {
                throw new NotSupportedException("Only depth sensor available when gestures are being detected");
            }
            this.captureType = captureType;

            var sts = PXCMSession.CreateInstance(out session);

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
            PXCMImage depthImage = null;
            PXCMImage colourImage = null;

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
                if (captureType == CaptureType.IMAGE_TYPE_DEPTH || captureType == CaptureType.BOTH)
                {
                    depthImage = uc.QueryImage(images, PXCMImage.ImageType.IMAGE_TYPE_DEPTH);

                    depthFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(depthImage, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
                }
                if (captureType == CaptureType.IMAGE_TYPE_COLOUR || captureType == CaptureType.BOTH)
                {
                    colourImage = uc.QueryImage(images, PXCMImage.ImageType.IMAGE_TYPE_COLOR);

                    colourFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(colourImage, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
                }

                nodes = Helpers.GeoNodesHelper.CaptureGeonodes(gesture);
                gestures = Helpers.GestureHelper.CaptureGestures(gesture);
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
