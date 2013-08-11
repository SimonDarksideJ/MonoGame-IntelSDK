using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLib.Input
{
    public class GestureController : IController
    {
        private PXCMGesture.Gesture[] gestures;
        private PXCMGesture.Gesture leftHand;
        private PXCMGesture.Gesture rightHand;

        public void Recognise(object input, bool debug = false)
        {
            if (input == null)
            {
                return;
            }
            CheckInputType(input);
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

        public void SetScale(int scale)
        {
            throw new NotImplementedException();
        }

        public void DebugDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spritebatch)
        {
            throw new NotImplementedException();
        }
    }
}
