using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace Blueberry.SceneManagement
{
    public interface IBoundedActor : IActor
    {
        RectangleF Bounds { get; }

        Vector2 Position { get; set; }
    }
}