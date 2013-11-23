using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using BlueberryEngine.GameObjects.Messages;
using OpenTK;

namespace BlueberryEngine.GameObjects
{
	public class GameObject
    {
		public Transform Transform{ get; set;}

        private List<Component> _components;
        private List<IUpdatable> _updatableComponents;
        private List<IDrawable> _drawableComponents;

        public bool Active { get; set; }
        public bool AutoChangeActivity { get; set; }
		public int ObjectGroup { get;private set; }
        
        public event EventHandler OnRemove;

		public GameObject(int objectGroup = 0)
        {
            _components = new List<Component>();
            _updatableComponents = new List<IUpdatable>();
            _drawableComponents = new List<IDrawable>();

            ObjectGroup = objectGroup;
            AutoChangeActivity = true;

			Transform = new Transform ();
        }


        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Component p = _components[i];
                if (p is T)
                    return (T) p;
            }
            return default(T);
        }

        public List<T> GetComponents<T>() where T : Component
        {
            List<T> list = new List<T>();
            for (int i = 0; i < _components.Count; i++)
            {
                Component p = _components[i];
                if (p is T) list.Add((T) p);
            }
            return list;
        }

        public void AddComponents(params Component[] components)
        {
            foreach (var component in components)
            {
				component.Owner = this;
                _components.Add(component);
                if (component is IUpdatable)
                {
                    _updatableComponents.Add((IUpdatable)component);
                }
                if (component is IDrawable)
                {
                    _drawableComponents.Add((IDrawable)component);
                }
            }
            foreach (Component component in components)
            {
                if (!component.GetDependicies()) throw new Exception("Depedicies were not found " + component);
            }
        }

        public void Update(float dt)
        {
            for (int i = 0; i < _updatableComponents.Count; i++)
            {
                var component = _updatableComponents[i];
                component.Update(dt);
            }
        }

        public void Draw(float dt)
        {
            for (int i = 0; i < _drawableComponents.Count; i++)
            {
                var component = _drawableComponents[i];
                component.Draw(dt);
            }
        }

        public void SendMessage(IMessage message)
        {
            for (int index = 0; index < _components.Count; index++)
            {
                var component = _components[index];
                component.ProccesMessage(message);
            }
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component component = GetComponent<T>();
            if (component != null)
            {
                _components.Remove(component);
                if(component is IUpdatable)
                {
                    _updatableComponents.Remove((IUpdatable)component);
                }
                if(component is IDrawable)
                {
                    _drawableComponents.Remove((IDrawable)component);
                }
            }
        }

        public void Dispose()
        {
            foreach (var component in _components)
            {
                if(component is IDisposable)
                    (component as IDisposable).Dispose();
            }
            if (OnRemove != null)
                OnRemove(this, EventArgs.Empty);
            OnRemove = null;
        }
    }
}
