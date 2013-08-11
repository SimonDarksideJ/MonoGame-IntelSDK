using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLib.Input
{
    public class InputHandler
    {
        private GestureController gestures = new GestureController();
        private HandController hands = new HandController();

        public InputHandler() { }

        public GestureController Gestures
        {
            get { return gestures; }
            set { gestures = value; }
        }

        public HandController Hands
        {
            get { return hands; }
            set { hands = value; }
        }
        
    }
}
