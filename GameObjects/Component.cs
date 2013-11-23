using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlueberryEngine.GameObjects.Messages;

namespace BlueberryEngine.GameObjects
{
    public abstract class Component
    {
        public abstract int ComponentType { get; }

		public GameObject Owner { get; internal set; }

        public Component()
        {
        }


        public abstract void ProccesMessage(IMessage message);

        public virtual bool GetDependicies()
        {
            return true;
        }
    }
}
