using System;
using Blueberry.GameObjects;
using OpenTK;
using System.Diagnostics;
using System.Collections.Generic;

namespace Blueberry
{
	public abstract class ColliderComponent:Component, IQuadTreeItem,  IUpdatable, IDisposable
    {
		public Vector2 Offset;
		
		protected IQuadTreeCollider QuadTreeCollider;
		private Vector2 _oldPosition;

		bool _hasCollisionListeners = false;

		public ColliderComponent()
		{
		}

		public override void Init()
		{
			foreach (var comp in Owner.GetComponents())
			{
				foreach (var item in comp.MessagesMethods)
				{
					if (item.Name == "OnTriggerEnter")
					{
						_hasCollisionListeners = true;
						break;
					}
				}
			}

		}

		protected abstract void UpdateCollider();

		private void UpdateCilliderData()
		{
			if (_oldPosition != Owner.Transform.Position)
			{
				UpdateCollider();
				_oldPosition = Owner.Transform.Position;
				if (OnPositionChange != null)
				{
					OnPositionChange(this);
				}
				else
				{
					Debug.WriteLine("No event");
				}
			}
		}

		#region IUpdatable implementation
		public void Update(float dt)
		{
			UpdateCilliderData();
			if (_hasCollisionListeners)
			{
				var collisions = new List<ColliderComponent>();
				Owner.GameObjectManager.QuadTree.Query(QuadTreeCollider, ref collisions);
				if (collisions.Count != 0)
				{
					foreach (var item in collisions)
					{
						Owner.SendMessage("OnTriggerEnter", item.Owner);
					}
				}
			}
		}
		#endregion

		#region IQuadTreeItem implementation
		public event PositionChangeHandler OnPositionChange;
		public event RemoveFromSceneHandler OnRemoveFromScene;
		public IQuadTreeCollider Collider
		{
			get
			{
				//UpdateCilliderData();// in case something changed position during collision checking
				return QuadTreeCollider;
			}
		}
		#endregion

		#region IDisposable implementation
		public void Dispose()
		{
			if (OnRemoveFromScene != null)
			{
				OnRemoveFromScene(this);
			}
		}
		#endregion
    }
}

