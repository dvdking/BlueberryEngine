using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blueberry.Geometry;
using Blueberry.GameObjects.Messages;
using Blueberry;
using System.Drawing;

namespace Blueberry.GameObjects
{
    public class GameObjectsManager
    {
		public readonly QuadTree<IQuadTreeItem> QuadTree;

        private List<GameObject> _gameObjects;
        private Queue<GameObject> _addNewObjectsQueue;
        private Queue<GameObject> _deleteObjectsQueue;

        private Camera _camera;
        private Rectangle _cameraRect;


        public GameObjectsManager(Camera camera)
        {
            _camera = camera;
            _gameObjects = new List<GameObject>(150);
            _addNewObjectsQueue = new Queue<GameObject>(150);
            _deleteObjectsQueue = new Queue<GameObject>(150);

			QuadTree = new QuadTree<IQuadTreeItem>(new Rectangle(0, 0, 1024, 1024));
		}

		public GameObject GetByName(string name)
		{
			return _gameObjects.Find(p => p.Name == name);
		}

        public void AddObject(GameObject gameObject)
        {
            if (gameObject == null) throw new ArgumentNullException("gameObject");
            _addNewObjectsQueue.Enqueue(gameObject);
			gameObject.GameObjectManager = this;

        }

        public void RemoveObject(GameObject gameObject)
        {
            if (gameObject == null) throw new ArgumentNullException("gameObject");
            _deleteObjectsQueue.Enqueue(gameObject);
        }

        public void Clear(params GameObject[] ignoreList)
        {
            UpdateObjectsEnqueues();
            foreach (var gameObject in _gameObjects.Where(gameObject => ignoreList.All(p => p != gameObject)))
            {
                RemoveObject(gameObject);
            }
            UpdateObjectsEnqueues();
        }


		public void SendGlobalMessage(string message, params object[] values)
        {
            foreach (var gameObject in _gameObjects)
            {
				gameObject.SendMessage(message, values);
            }
        }

        public void Update(float dt)
        {
            _cameraRect = _camera.BoundingRectangle;
            _cameraRect.Inflate(_cameraRect.Width, _cameraRect.Height);

            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var gameObject = _gameObjects[i];
                if (gameObject.AutoChangeActivity)
                    gameObject.Active = IsActive(gameObject);
                if (gameObject.Active)
                    gameObject.Update(dt);
            }

            UpdateObjectsEnqueues();
        }

        private bool IsActive(GameObject gameObject)
        {
			var bc = gameObject.Transform;
            return bc == null || _cameraRect.Contains(bc.Position);
        }

        public void Draw(float dt)
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var gameObject = _gameObjects[i];
                if (gameObject.Active)
                    gameObject.Draw(dt);
            }
        }


		public void UpdateObjectsEnqueues()
        {
            while (_addNewObjectsQueue.Count > 0)
            {
                GameObject gameObject = _addNewObjectsQueue.Dequeue();
                _gameObjects.Add(gameObject);
            }
            while (_deleteObjectsQueue.Count > 0)
            {
                GameObject obj = _deleteObjectsQueue.Dequeue();
                obj.Dispose();
                _gameObjects.Remove(obj);
            }
        }
    }
}
