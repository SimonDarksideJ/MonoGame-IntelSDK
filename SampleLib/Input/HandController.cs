using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLib.Input
{
    public class HandController : IController
    {
        private GraphicsDevice device;
        private int scale = 1;
        private PXCMGesture.GeoNode[][] nodes = new PXCMGesture.GeoNode[2][] { new PXCMGesture.GeoNode[11], new PXCMGesture.GeoNode[11] };
        private bool leftHandDetected = false;
        private PXCMGesture.GeoNode leftHandResting = new PXCMGesture.GeoNode();
        private PXCMGesture.GeoNode[] leftHandCurrent = new PXCMGesture.GeoNode[11];
        private PXCMGesture.GeoNode[] leftHandPrevious = new PXCMGesture.GeoNode[11];
        private bool rightHandDetected = false;
        private PXCMGesture.GeoNode rightHandResting = new PXCMGesture.GeoNode();
        private PXCMGesture.GeoNode[] rightHandCurrent = new PXCMGesture.GeoNode[11];
        private PXCMGesture.GeoNode[] rightHandPrevious = new PXCMGesture.GeoNode[11];

        List<PrimitiveLine> debugPoints = new List<PrimitiveLine>();
        
        public void EnableDebug(GraphicsDevice device)
        {
            this.device = device;
        }

        public void SetScale(int scale)
        {
            this.scale = scale;
        }

        public void Initialise(object input)
        {
            CheckInputType(input);
            leftHandResting = nodes[0][0];
            rightHandResting = nodes[1][0];
        }

        public void Recognise(object input, bool debug = false)
        {
            debugPoints.Clear();
            if (input == null)
            {
                return;
            }

            CheckInputType(input);
            leftHandPrevious = leftHandCurrent;
            leftHandCurrent = nodes[0];
            if (debug)
            {
                AddDebugDrawImages(leftHandCurrent);
            }
            rightHandPrevious = rightHandCurrent;
            rightHandCurrent = nodes[1];
            if (debug)
            {
                AddDebugDrawImages(rightHandCurrent);
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

        public Vector3 LeftHandWorldPosition()
        {
            return new Vector3(leftHandCurrent[0].positionWorld.x, leftHandCurrent[0].positionWorld.z, leftHandCurrent[0].positionWorld.y);
        }

        public Vector3 LeftHandCapturePosition()
        {
            return new Vector3(leftHandCurrent[0].positionImage.x, leftHandCurrent[0].positionImage.y, leftHandCurrent[0].positionImage.z);
        }

        public Vector3 RightHandWorldPosition()
        {
            return new Vector3(rightHandCurrent[0].positionWorld.x, rightHandCurrent[0].positionWorld.z, rightHandCurrent[0].positionWorld.y);
        }

        public Vector3 RightHandCapturePosition()
        {
            return new Vector3(rightHandCurrent[0].positionImage.x, rightHandCurrent[0].positionImage.y, rightHandCurrent[0].positionImage.z);
        }

        public PXCMGesture.GeoNode GetBodyPart(PXCMGesture.GeoNode.Label type, PXCMGesture.GeoNode.Side side = PXCMGesture.GeoNode.Side.LABEL_LEFT)
        {
            switch ((int)type)
            {
                case 262144: //LABEL_BODY_HAND_LEFT / LABEL_BODY_HAND_PRIMARY
                    return leftHandCurrent[0];
                case 524288: //LABEL_BODY_HAND_RIGHT / LABEL_BODY_HAND_SECONDARY
                    return rightHandCurrent[0];
                case 16384: // LABEL_BODY_ELBOW_LEFT / LABEL_BODY_ELBOW_PRIMARY
                    return leftHandCurrent[10];
                case 32768: // LABEL_BODY_ELBOW_RIGHT / LABEL_BODY_ELBOW_SECONDARY
                    return rightHandCurrent[10];
                case 256: // PXCMGesture.GeoNode.Label.LABEL_MASK_BODY
                case 255: // PXCMGesture.GeoNode.Label.LABEL_MASK_DETAILS
                    return new PXCMGesture.GeoNode();
                default: // Fingers
                    if (side == PXCMGesture.GeoNode.Side.LABEL_LEFT)
                    {
                        return leftHandCurrent[(int)type];
                    }
                    else
                    {
                        return rightHandCurrent[(int)type];
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
                brush.Position = new Vector2(node.positionImage.x * scale, node.positionImage.y * scale);
                debugPoints.Add(brush);
            }
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
    }
}
