using System;
using System.Collections.Generic;
using System.Drawing;
using Blueberry.Geometry;
using OpenTK;

namespace Blueberry
{
    public delegate void PositionChangeHandler(object sender);
    public delegate void RemoveFromSceneHandler(object sender);

    /// <summary>Класс представляет дерево квадратов для графа сцены</summary>
    /// <typeparam name="T">Тип элементов в дереве</typeparam>
    public class QuadTree<T> where T : IQuadTreeItem
    {
        #region Variables

        private QuadTreeNode<T> rootNode; // корневой узел дерева
        private byte maxGeneration = byte.MaxValue; // максимальное число поколений

        #endregion Variables

        #region Properties

        /// <summary>Прямоугольник, ограничивающий область, занимаемую деревом</summary>
        public Rectangle Area { get { return rootNode.Area; } }

        /// <summary>Количество элементов в дереве</summary>
        public int ContentCount { get { return rootNode.BranchContentCount; } }

        /// <summary>Все элементы в этом дереве</summary>
        public List<T> Content { get { return rootNode.BranchContent; } }

        /// <summary>Все узлы этого дерева</summary>
        public List<QuadTreeNode<T>> Nodes { get { return rootNode.Nodes; } }

        /// <summary>Количество узлов в дереве</summary>
        public int NodeCount { get { return rootNode.NodeCount; } }

        /// <summary>Максимальное количество поколений узлов в этом дереве</summary>
        public byte MaxGeneration { get { return maxGeneration; } set { maxGeneration = value; } }

        public byte OldestGeneration { get { return rootNode.OldestGeneration; } }

        #endregion Properties

        #region Initialize

        /// <summary>Создает новое дерево</summary>
        /// <param name="area">Область, занимаемая деревом</param>
        public QuadTree(Rectangle area)
        {
            this.rootNode = new QuadTreeNode<T>(null, this, area); // создаем корневой узел
        }

        #endregion Initialize

        #region Insertion

        /// <summary>Добавление элемента в дерево</summary>
        /// <param name="item">Элемент для добавления</param>
        public void Insert(T item)
        {
            // проверяем, не выходит ли элемент за пределы дерева
            if (!rootNode.Area.Contains(item.Bounds))
            {
                // изменяем размер дерева
                Resize(new Rectangle(
                    Math.Min(rootNode.Area.Left, item.Bounds.Left),
                    Math.Min(rootNode.Area.Top, item.Bounds.Top),
                    Math.Max(rootNode.Area.Right, item.Bounds.Right) - Math.Min(rootNode.Area.Left, item.Bounds.Left),
                    Math.Max(rootNode.Area.Bottom, item.Bounds.Bottom) - Math.Min(rootNode.Area.Top, item.Bounds.Top)));
            }
            rootNode.Insert(item); // добавляем элемент в корневой узел дерева
        }

        /// <summary>Изменяет область, занимаемую деревом</summary>
        /// <param name="newArea">Новая область</param>
        public void Resize(Rectangle newArea)
        {
            List<T> Components = rootNode.BranchContent; // извлекаем все элементы
            rootNode.Destroy(); // разрушаем корневой узел
            rootNode = null;
            rootNode = new QuadTreeNode<T>(null, this, newArea); // создаем корневой узел
            foreach (T m in Components)
            {
                rootNode.Insert(m); // добавляем элементы
            }
        }

        /// <summary>
        /// Перестравиает дерево заново
        /// </summary>
        public void Rebuild()
        {
            List<T> Components = rootNode.BranchContent; // извлекаем все элементы
            Rectangle area = Area; // после разрушения корневого узла, область станет недоступной
            rootNode.Destroy(); // разрушаем корневой узел
            rootNode = null;
            rootNode = new QuadTreeNode<T>(null, this, area); // создаем корневой узел
            foreach (T m in Components)
            {
                rootNode.Insert(m); // добавляем элементы
            }
        }

        #endregion Insertion

        #region Query

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Ссылка на точку, которую содержат элементы</param>
        /// <param name="itemsList">Возвращаемый список с элементами</param>
        public void Query(ref Vector2 queryPoint, ref List<T> itemsList)
        {
            if (itemsList != null)
            {
                rootNode.Query(ref queryPoint, itemsList);
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Ссылка на точку, которую содержат элементы</param>
        /// <param name="itemsList">Возвращаемый список с элементами</param>
        public void Query(ref Point queryPoint, List<T> itemsList)
        {
            if (itemsList != null)
            {
                rootNode.Query(ref queryPoint, itemsList);
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Ссылка на прямоугольник, в область которого должны входить элементы</param>
        /// <param name="itemsList">Возвращаемый список с элементами</param>
        public void Query(ref Rectangle queryArea, List<T> itemsList)
        {
            if (itemsList != null)
            {
                rootNode.Query(ref queryArea, itemsList);
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Ссылка на окружность, в область которой должны входить элементы</param>
        /// <param name="itemsList">Возвращаемый список с элементами</param>
        public void Query(ref Circle queryArea, List<T> itemsList)
        {
            if (itemsList != null)
            {
                rootNode.Query(ref queryArea, itemsList);
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Точка, которую содержат элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Vector2 queryPoint)
        {
            List<T> r = new List<T>();
            rootNode.Query(ref queryPoint, r);
            return r;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Точка, которую содержат элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Point queryPoint)
        {
            List<T> r = new List<T>();
            rootNode.Query(ref queryPoint, r);
            return r;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Прямоугольник, в область которого должны входить элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Rectangle queryArea)
        {
            List<T> r = new List<T>();
            rootNode.Query(ref queryArea, r);
            return r;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Окружность, в область которой должны входить элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Circle queryArea)
        {
            List<T> r = new List<T>();
            rootNode.Query(ref queryArea, r);
            return r;
        }

        #endregion Query

        #region Destroying

        /// <summary>Разрушает дерево</summary>
        public void Destroy()
        {
            rootNode.Destroy();
        }

        /// <summary>Удаляет элемент из дерева</summary>
        /// <param name="item">Элемент для удаления</param>
        public void RemoveItem(T item)
        {
            rootNode.RemoveItem(item);
        }

        #endregion Destroying
    }
}