using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.SceneManagement
{
    public interface IActor
    {
        Scene Scene { get; }

        string Name { get; }

        float ParallaxLayer { get; set; }

        void Draw(float dt);

        void Update(float dt);

        void Load();
    }
}