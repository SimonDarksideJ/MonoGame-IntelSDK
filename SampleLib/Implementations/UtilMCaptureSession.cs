using System;
using System.Threading;

namespace SimpleLib
{
    public class UtilMCaptureSession : IVideoCapture
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
            if (captureType == CaptureType.BOTH)
            {
                throw new NotSupportedException("This capture type only supports depth or colour and not both");
            }
            this.captureType = captureType;

            PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = (uint)Width, height = (uint)Height };
            var DepthSize = new PXCMSizeU32() { width = (uint)Width, height = (uint)Height };

            PXCMCapture.VideoStream.DataDesc.StreamDesc strm;
            if (captureType == CaptureType.IMAGE_TYPE_DEPTH)
            {
                strm = new PXCMCapture.VideoStream.DataDesc.StreamDesc()
                {
                    format = PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH,
                    sizeMin = DepthSize,
                    sizeMax = DepthSize
                };
            }
            else
            {
                strm = new PXCMCapture.VideoStream.DataDesc.StreamDesc()
                {
                    format = PXCMImage.ColorFormat.COLOR_FORMAT_RGB32,
                    sizeMin = ColourSize,
                    sizeMax = ColourSize
                };
            }


            PXCMCapture.VideoStream.DataDesc req = new PXCMCapture.VideoStream.DataDesc();

            req.streams[0] = strm;

            uc = new UtilMCapture(session);

            uc.LocateStreams(ref req);

            thread = new Thread(
            new ThreadStart(StartCaptureStream));
            thread.Start();
        }

        public void StartCaptureStream()
        {
            PXCMImage image = null;

            PXCMScheduler.SyncPoint sp = null;
            
            capturing = true;
            while (capturing)
            {
                if (uc.QueryVideoStream(0).ReadStreamAsync(out image, out sp) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    break;
                }
                sp.Synchronize();
                if (captureType == CaptureType.IMAGE_TYPE_DEPTH)
                {
                    depthFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(image, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
                }
                else
                {
                    colourFrame = Helpers.PCXMImageHelper.PXCMImageToByteArray(image, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out width, out height);
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
