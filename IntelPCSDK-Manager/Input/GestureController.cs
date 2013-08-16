using System;

namespace IntelPCSDK_Manager.Input
{
    public class GestureController : IController
    {
        private PXCMGesture.Gesture[] gestures;
        public PXCMGesture.Gesture PrimaryGesture
        {
            get { return gestures != null ? gestures[0] : new PXCMGesture.Gesture(); }
        }
        public PXCMGesture.Gesture SecondaryGesture
        {
            get { return gestures != null ? gestures[1] : new PXCMGesture.Gesture(); }
        }
        public PXCMGesture.Gesture.Label[] DetectedGestures = new PXCMGesture.Gesture.Label[2];

        public void Recognise(object input, bool debug = false)
        {
            if (input == null)
            {
                return;
            }
            CheckInputType(input);
            DetectedGestures[0] = PrimaryGesture.label;
            DetectedGestures[1] = SecondaryGesture.label;
        }

        private void CheckInputType(object input)
        {
            if (input.GetType() == typeof(PXCMGesture.Gesture[]))
            {
                this.gestures = (PXCMGesture.Gesture[])input;
            }
            else
            {
                throw new FormatException("Data is not recognises as Gesture data <PXCMGesture.Gesture[]>");
            }
        }

        public void Initialise(object input)
        {
            throw new NotImplementedException();
        }

        public void EnableDebug(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void SetScale(double scale)
        {
            throw new NotImplementedException();
        }

        public void DebugDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spritebatch)
        {
            throw new NotImplementedException();
        }
    }
}
