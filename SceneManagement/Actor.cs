using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.SceneManagement
{
    public abstract class Actor : IActor
    {
        private Scene scene;
        private string name;

        public string Name { get { return name; } set { name = value; } }

        public Scene Scene { get { return scene; } internal set { scene = value; } }

        public float ParallaxLayer { get; set; }

        public virtual void Draw(float dt) { }

        public virtual void Update(float dt) { }

        public virtual void Load() { }
    }
}