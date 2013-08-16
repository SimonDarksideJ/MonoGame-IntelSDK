using IntelPCSDK_Manager.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace IntelPCSDK_Manager.Input
{
    public class HandController : IController
    {
        const int numSmoothSamples = 5;

        private GraphicsDevice device;
        private double scale = 1;
        private PXCMGesture.GeoNode[][] nodes = new PXCMGesture.GeoNode[2][] { new PXCMGesture.GeoNode[11], new PXCMGesture.GeoNode[11] };
        private bool primaryHandDetected = false;
        private PXCMGesture.GeoNode primaryHandResting = new PXCMGesture.GeoNode();
        private PXCMGesture.GeoNode[] primaryHandCurrent = new PXCMGesture.GeoNode[11];
        private PXCMGesture.GeoNode[] primaryHandPrevious = new PXCMGesture.GeoNode[11];
        private Vector3 primaryHandWorldPositionTolerance = Vector3.Zero;
        private Vector3 primaryHandImagePositionTolerance = Vector3.Zero;

        private Vector3SmoothingSampler primaryHandWorldSmoother = new Vector3SmoothingSampler(numSmoothSamples);
        private Vector3SmoothingSampler primaryHandImageSmoother = new Vector3SmoothingSampler(numSmoothSamples);

        private bool secondaryHandDetected = false;
        private PXCMGesture.GeoNode secondaryHandResting = new PXCMGesture.GeoNode();
        private PXCMGesture.GeoNode[] secondaryHandCurrent = new PXCMGesture.GeoNode[11];
        private PXCMGesture.GeoNode[] secondaryHandPrevious = new PXCMGesture.GeoNode[11];
        private Vector3 secondaryHandWorldPositionTolerance = Vector3.Zero;
        private Vector3 secondaryHandImagePositionTolerance = Vector3.Zero;

        private Vector3SmoothingSampler secondaryHandWorldSmoother = new Vector3SmoothingSampler(numSmoothSamples);
        private Vector3SmoothingSampler secondaryHandImageSmoother = new Vector3SmoothingSampler(numSmoothSamples);

        readonly List<PrimitiveLine> debugPoints = new List<PrimitiveLine>();

        readonly TouchLocation[] touches = new TouchLocation[10];
        bool TouchReset;

        public TouchLocation[] Touches
        {
            get { return touches; }
        }

        public void EnableDebug(GraphicsDevice device)
        {
            this.device = device;
        }

        public void SetScale(double scale)
        {
            this.scale = scale;
        }

        public void Initialise(object input)
        {
            CheckInputType(input);
            primaryHandResting = nodes[0][0];
            secondaryHandResting = nodes[1][0];
        }

        private void ResetTouchCollection()
        {
            for (int i = 0; i < 10; i++)
            {
                touches[i] = new TouchLocation();
            }
        }

        public void Recognise(object input, bool debug = false)
        {
            debugPoints.Clear();
            if (input == null)
            {
                primaryHandCurrent = new PXCMGesture.GeoNode[11];
                secondaryHandCurrent = new PXCMGesture.GeoNode[11];
                if (!TouchReset)
                {
                    ResetTouchCollection();
                    TouchReset = true;
                }
                return;
            }
            TouchReset = false;
            CheckInputType(input);
            primaryHandPrevious = primaryHandCurrent;
            primaryHandCurrent = nodes[0];
            if (debug)
            {
                AddDebugDrawImages(primaryHandCurrent);
            }
            GetTouchPoints(0, nodes[0]); // get touches for primary hand

            secondaryHandPrevious = secondaryHandCurrent;
            secondaryHandCurrent = nodes[1];
            if (debug)
            {
                AddDebugDrawImages(secondaryHandCurrent);
            }
            GetTouchPoints(5, nodes[1]); // get touches for secondary hand

        }

        private void GetTouchPoints(int startTouchIndex, PXCMGesture.GeoNode[] handNodes)
        {
            int touchIndex = startTouchIndex;
            for (int i = 0; i < 11; i++)
            {
                switch (i)
                {
                    case 2: // Index finger
                    case 3: // Middle finger
                    case 4: // Ring Finger
                    case 5: // Pinky Finger
                    case 9: // Thumb / hand low?
                        var currentTouch = handNodes[i];
                        if (currentTouch.confidence > 30)
                        {
                            var touchState = touches[touchIndex].State == TouchLocationState.Released ? TouchLocationState.Pressed : TouchLocationState.Moved;
                            touches[touchIndex] = new TouchLocation(touchIndex, touchState, new Vector2(currentTouch.positionImage.x, currentTouch.positionImage.y), touchState, new Vector2(currentTouch.positionWorld.x, currentTouch.positionWorld.z));
                        }
                        else
                        {
                            touches[touchIndex] = new TouchLocation(touchIndex, TouchLocationState.Released, Vector2.Zero);
                        }
                        touchIndex++;
                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckInputType(object nodes)
        {
            if (nodes.GetType() == typeof(PXCMGesture.GeoNode[][]))
            {
                this.nodes = (PXCMGesture.GeoNode[][])nodes;
            }
            else
            {
                throw new FormatException("Data is not recognises as GeoNode data <PXCMGesture.GeoNode[][]>");
            }
            
        }

        public Vector3 PrimaryHandWorldPositionRaw()
        {
            return primaryHandCurrent[0].positionWorld.ToVector3();
        }

        public Vector3 PrimaryHandWorldPositionSmoothed()
        {
            return primaryHandWorldSmoother.GetValue(primaryHandCurrent[0].positionWorld.ToVector3());
        }

        public Vector3 PrimaryHandWorldPositionTolerance(float tolerance)
        {
            var newPos = primaryHandWorldSmoother.GetValue(primaryHandCurrent[0].positionWorld.ToVector3());
            if (IsGreaterThan(primaryHandWorldPositionTolerance, newPos,tolerance))
            {
                primaryHandWorldPositionTolerance = newPos;
            }
            return primaryHandWorldPositionTolerance;
        }

        public Vector3 PrimaryHandImagePositionRaw()
        {
            return primaryHandCurrent[0].positionImage.ToVector3();
        }

        public Vector3 PrimaryHandImagePositionSmoothed()
        {
            return primaryHandImageSmoother.GetValue(primaryHandCurrent[0].positionImage.ToVector3());
        }

        public Vector3 PrimaryHandImagePositionTolerance(float tolerance)
        {
            var newPos = primaryHandImageSmoother.GetValue(primaryHandCurrent[0].positionImage.ToVector3());
            if (IsGreaterThan(primaryHandImagePositionTolerance, newPos, tolerance))
            {
                primaryHandImagePositionTolerance = newPos;
            }
            return primaryHandImagePositionTolerance;
        }

        public Vector3 SecondaryHandWorldPositionRaw()
        {
            return secondaryHandCurrent[0].positionWorld.ToVector3();
        }

        public Vector3 SecondaryHandWorldPositionSmoothed()
        {
            return secondaryHandWorldSmoother.GetValue(secondaryHandCurrent[0].positionWorld.ToVector3());
        }

        public Vector3 SecondaryHandWorldPositionTolerance(float tolerance)
        {
            var newPos = secondaryHandWorldSmoother.GetValue(secondaryHandCurrent[0].positionWorld.ToVector3());
            if (IsGreaterThan(secondaryHandWorldPositionTolerance, newPos, tolerance))
            {
                secondaryHandWorldPositionTolerance = newPos;
            }
            return secondaryHandWorldPositionTolerance;
        }

        public Vector3 SecondaryHandImagePositionRaw()
        {
            return secondaryHandCurrent[0].positionImage.ToVector3();
        }

        public Vector3 SecondaryHandImagePositionSmoothed()
        {
            return secondaryHandImageSmoother.GetValue(secondaryHandCurrent[0].positionImage.ToVector3());
        }

        public Vector3 SecondaryHandImagePositionTolerance(float tolerance)
        {
            var newPos = secondaryHandImageSmoother.GetValue(secondaryHandCurrent[0].positionImage.ToVector3());
            if (IsGreaterThan(secondaryHandImagePositionTolerance, newPos, tolerance))
            {
                secondaryHandImagePositionTolerance = newPos;
            }
            return secondaryHandImagePositionTolerance;
        }

        public PXCMGesture.GeoNode GetBodyPart(PXCMGesture.GeoNode.Label type, PXCMGesture.GeoNode.Side side = PXCMGesture.GeoNode.Side.LABEL_LEFT)
        {
            switch ((int)type)
            {
                case 262144: //LABEL_BODY_HAND_primary / LABEL_BODY_HAND_PRIMARY
                    return primaryHandCurrent[0];
                case 524288: //LABEL_BODY_HAND_secondary / LABEL_BODY_HAND_SECONDARY
                    return secondaryHandCurrent[0];
                case 16384: // LABEL_BODY_ELBOW_primary / LABEL_BODY_ELBOW_PRIMARY
                    return primaryHandCurrent[10];
                case 32768: // LABEL_BODY_ELBOW_secondary / LABEL_BODY_ELBOW_SECONDARY
                    return secondaryHandCurrent[10];
                case 256: // PXCMGesture.GeoNode.Label.LABEL_MASK_BODY
                case 255: // PXCMGesture.GeoNode.Label.LABEL_MASK_DETAILS
                    return new PXCMGesture.GeoNode();
                default: // Fingers
                    if (side == PXCMGesture.GeoNode.Side.LABEL_LEFT)
                    {
                        return primaryHandCurrent[(int)type];
                    }
                    else
                    {
                        return secondaryHandCurrent[(int)type];
                    }
            }
        }
        
        public void DebugDraw(SpriteBatch spritebatch)
        {
            foreach (var item in debugPoints)
            {
                item.Render(spritebatch);
            }
        }

        private void AddDebugDrawImages(PXCMGesture.GeoNode[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.body <= 0)  continue;
                float sz = (i == 0) ? 10 : ((node.radiusImage > 5) ? node.radiusImage : 5);
                PrimitiveLine brush = new PrimitiveLine(device);
                brush.Colour = i > 5 ? Color.Red : Color.Green;
                brush.CreateCircle(sz, 10);
                brush.Position = new Vector2(node.positionImage.x * (float)scale, node.positionImage.y * (float)scale);
                debugPoints.Add(brush);
            }
        }

        private bool IsGreaterThan(Vector3 currPos, Vector3 newPos, float tolerance)
        {
            return currPos.X - newPos.X > tolerance ||
                    -currPos.X + newPos.X > tolerance ||
                    currPos.Y - newPos.Y > tolerance ||
                    -currPos.Y + newPos.Y > tolerance ||
                    currPos.Z - newPos.Z > tolerance ||
                    -currPos.Z + newPos.Z > tolerance;
        }
    }

    public static class GeoNodeExtensions
    {
        public static PrimitiveLine DrawGeoNode(this PXCMGesture.GeoNode input, GraphicsDevice device, int scale = 1)
        {
            PrimitiveLine brush = new PrimitiveLine(device);
            brush.Colour = input.side == PXCMGesture.GeoNode.Side.LABEL_LEFT ? Color.Red : Color.Green;
            brush.CreateCircle(10, 10);
            brush.Position = new Vector2(input.positionImage.x * scale, input.positionImage.y * scale);
            return brush;
        }

        public static PrimitiveLine DrawGeoNode(this Vector3 input, GraphicsDevice device, Color color, int scale = 1)
        {
            PrimitiveLine brush = new PrimitiveLine(device);
            brush.Colour = color;
            brush.CreateCircle(10, 10);
            brush.Position = new Vector2(input.X * scale, input.Y * scale);
            return brush;
        }
    }
}
