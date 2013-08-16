/// Header
/// PXC M Sensor retrieval and processing
/// 
/// ** Note WIP - only enumerates video devices at the moment
/// Needs updating to allow audio and other streams

using System.Collections.Generic;
using System.Threading;

namespace IntelPCSDK_Manager
{
    public class PXCMCaptureSessionMultiple : IVideoCapture
    {
        CaptureType captureType;

        PXCMSession session;
        List< PXCMCapture.Device> cameras = new List<PXCMCapture.Device>();
        List<PXCMCapture.VideoStream> depthStreams = new List<PXCMCapture.VideoStream>();

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
            this.captureType = captureType;

            var sts = PXCMSession.CreateInstance(out session);

            PXCMCapture capture;

            this.session.CreateImpl(PXCMCapture.CUID, out capture);

            for (int d = 0; ; ++d)
            {

                PXCMCapture.DeviceInfo devInfo;

                PXCMCapture.Device.StreamInfo streamInfo;

                PXCMCapture.Device camera;

                PXCMCapture.VideoStream depthStream;

                if (capture.QueryDevice((uint)d, out devInfo) ==

                    pxcmStatus.PXCM_STATUS_ITEM_UNAVAILABLE) break;

                if (!devInfo.name.get().Contains("325V2")) continue;

                capture.CreateDevice((uint)d, out camera);

                for (int s = 0; ; ++s)
                {

                    if (camera.QueryStream((uint)s, out streamInfo) !=

                        pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    if (streamInfo.cuid != PXCMCapture.VideoStream.CUID) continue;

                    if (streamInfo.imageType != PXCMImage.ImageType.IMAGE_TYPE_DEPTH) continue;

                    if (camera.CreateStream(

                        streamInfo.sidx, streamInfo.cuid, out depthStream) !=

                        pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    this.depthStreams.Add(depthStream);

                    this.cameras.Add(camera);

                    break;

                }

                if (this.cameras.Count > 1) break;

            }

            capture.Dispose();




            if (cameras.Count > 0 & depthStreams.Count > 0)
            {
                thread = new Thread(new ThreadStart(StartCaptureStream));
                thread.Start();
            }
        }

        public void StartCaptureStream()
        {
            //Work with multiple cameras and depth streams

        }

        public void Dispose(bool disposing)
        {
            capturing = false;
            foreach (var stream in this.depthStreams) stream.Dispose();

            foreach (var camera in this.cameras) camera.Dispose();

            if (this.session != null) session.Dispose();
        }


    }

}
