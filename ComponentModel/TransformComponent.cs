using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class TransformComponent : Component
    {
        public float X;
        public float Y;
        public float ScaleX;
        public float ScaleY;
        public float RotationZ;

        public TransformComponent()
        {
            X = Y = 0.0f;
            ScaleX = ScaleY = 1.0f;
            RotationZ = 0.0f;
        }

        public override void ReceiveMessage(IMessage message)
        {
        }
    }
}
