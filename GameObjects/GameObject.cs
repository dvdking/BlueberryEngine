using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Blueberry.GameObjects.Messages;
using OpenTK;

namespace Blueberry.GameObjects
{
	public class GameObject
    {
		public GameObjectsManager GameObjectManager{ get; internal set;}

		public string Name;

		public Transform Transform;

		public bool Active = true;
		public bool AutoChangeActivity;
		public int ObjectGroup;
        
        public event EventHandler OnRemove;

		private List<Component> _components;
		private List<IUpdatable> _updatableComponents;
		private List<IDrawable> _drawableComponents;

		public GameObject(int objectGroup = 0)
        {
            _components = new List<Component>();
            _updatableComponents = new List<IUpdatable>();
            _drawableComponents = new List<IDrawable>();

            ObjectGroup = objectGroup;
			AutoChangeActivity = false;

			Transform = new Transform ();
			_components.Add(Transform);
        }


		public Component GetComponent(string name)
		{
			for (int i = 0; i < _components.Count; i++)
			{
				Component p = _components[i];
				if (p.Name == name)
					return p;
			}
			return null;
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
				if (component is IQuadTreeItem)
				{
					GameObjectManager.QuadTree.Insert(component as IQuadTreeItem);
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

		public void SendMessage(string Name, params object[] values)
		{
			foreach (var item in _components)
			{
				var method = item.MessagesMethods.Find(p => p.Name == Name);
				if (method != null)
				{
					method.Invoke(item, values);
				}
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
				if (component is IDisposable)
				{
					(component as IDisposable).Dispose();
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
			{
				OnRemove(this, EventArgs.Empty);
			}
            OnRemove = null;
        }

		public void ClearComponents()
		{
			_components.Clear();
			_updatableComponents.Clear();
			_drawableComponents.Clear();

			Transform = null;
		}

		public GameObject Clone()
		{
			var clone = new GameObject();
			clone.ClearComponents();

			var components = new Component[_components.Count];

			clone.Transform = (Transform)Transform.Clone();
			components[0] = clone.Transform;
			for (int i = 1; i < components.Length; i++)
				components[i] = (Component)_components[i].Clone();

			clone.AddComponents(components);
			return clone;
		}
    }
}
