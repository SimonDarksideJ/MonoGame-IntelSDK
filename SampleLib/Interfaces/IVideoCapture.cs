using System.Threading;

namespace SimpleLib
{
    interface IVideoCapture
    {
        //Raw depth byte streams from camera - RGBA format
        byte[] DepthFrame { get; }

        //Raw depth byte streams from camera - RGBA format (Note XNA requires BGRA so convert it using extension)
        byte[] ColourFrame { get; }

        //Node and Gesture capture points
        PXCMGesture.GeoNode[][] Nodes { get; }
        PXCMGesture.Gesture[] Gestures { get; }

        //State to indicate camera is streaming
        bool Capturing { get; }

        //Current dimentions of the stream image
        int Width { get; }
        int Height { get; }

        //Test for the running of the internal thread - needed to check when closing to avoid overflow errors
        ThreadState State { get; }

        //initialise the camera and start streaming
        void Initialise(CaptureType captureType = CaptureType.IMAGE_TYPE_DEPTH);

        //internal use only - direct access to stream gathering function
        void StartCaptureStream();

        //Clean up and close down current stream - Note, thread has to finish before closing to avoid memory overflow from SDK
        void Dispose(bool disposing);

        
    }
}
