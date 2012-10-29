using System;
using System.Collections.Generic;
using System.Drawing;
using Blueberry.Geometry;
using OpenTK;

namespace Blueberry
{
    /// <summary>Класс представляет узел в дереве QuadTree</summary>
    /// <typeparam name="T">Тип элементов в узле</typeparam>
    public class QuadTreeNode<T> where T : IQuadTreeItem
    {
        #region Variables

        private Rectangle area;     // прямоугольник, ограничивающий область, занимаемую узлом
        private bool partitioned;   // указывает, разбит ли узел на подузлы
        private QuadTree<T> tree;   // класс дерева
        private QuadTreeNode<T> parentNode;     // родительский узел
        private QuadTreeNode<T> leftTopNode;    // левый верхний подузел
        private QuadTreeNode<T> rightTopNode;   // правый верхний подузел
        private QuadTreeNode<T> leftBottomNode; // левый нижний подузел
        private QuadTreeNode<T> rightBottomNode;// правый нижний подузел
        private List<T> content;    // содержание узла
        private byte generation; // поколение узла

        #endregion Variables

        #region Properties

        /// <summary>Прямоугольник, ограничивающий область, занимаемую узлом</summary>
        public Rectangle Area { get { return area; } }

        /// <summary>Проверка на пустой узел</summary>
        public bool IsEmpty { get { return area.IsEmpty || !partitioned; } }

        /// <summary>Содержание конкретного узла</summary>
        public List<T> NodeContent { get { return content; } }

        /// <summary>Содержание всей ветви узлов</summary>
        public List<T> BranchContent
        {
            get
            {
                List<T> results = new List<T>();
                if (partitioned)
                {
                    results.AddRange(leftTopNode.BranchContent);
                    results.AddRange(leftBottomNode.BranchContent);
                    results.AddRange(rightTopNode.BranchContent);
                    results.AddRange(rightBottomNode.BranchContent);
                }
                results.AddRange(this.NodeContent);
                return results;
            }
        }

        /// <summary>Количество элементов в конкретном узле</summary>
        public int NodeContentCount { get { return this.NodeContent.Count; } }

        /// <summary>Количество элементов во всей ветви узлов</summary>
        public int BranchContentCount
        {
            get
            {
                int count = 0;
                if (partitioned)
                {
                    count += leftTopNode.BranchContentCount;
                    count += leftBottomNode.BranchContentCount;
                    count += rightTopNode.BranchContentCount;
                    count += rightBottomNode.BranchContentCount;
                }
                count += this.NodeContent.Count;
                return count;
            }
        }

        /// <summary>Все узлы этой ветви, включая родительский узел</summary>
        public List<QuadTreeNode<T>> Nodes
        {
            get
            {
                List<QuadTreeNode<T>> ls = new List<QuadTreeNode<T>>();
                ls.Add(this);
                if (partitioned)
                {
                    ls.AddRange(leftTopNode.Nodes);
                    ls.AddRange(leftBottomNode.Nodes);
                    ls.AddRange(rightTopNode.Nodes);
                    ls.AddRange(rightBottomNode.Nodes);
                }
                return ls;
            }
        }

        /// <summary>Количество узлов в этой ветке</summary>
        public int NodeCount
        {
            get
            {
                if (partitioned)
                    return leftTopNode.NodeCount + leftBottomNode.NodeCount + rightTopNode.NodeCount + rightBottomNode.NodeCount + 1;
                else
                    return 1;
            }
        }

        /// <summary>Поколение этого узла в дереве</summary>
        public byte Generation { get { return generation; } }

        public byte OldestGeneration
        {
            get
            {
                if (partitioned)
                {
                    byte m = Math.Max(leftTopNode.OldestGeneration, leftBottomNode.OldestGeneration);
                    m = Math.Max(m, rightTopNode.OldestGeneration);
                    m = Math.Max(m, rightBottomNode.OldestGeneration);
                    return m;
                }
                return generation;
            }
        }

        #endregion Properties

        #region Initialize

        /// <summary>Создает новый экземпляр узла</summary>
        /// <param name="parentNode">Родительский узел</param>
        /// <param name="tree">Дерево, которому принадлежит узел</param>
        /// <param name="area">Область, занимаемая узлом</param>
        public QuadTreeNode(QuadTreeNode<T> parentNode, QuadTree<T> tree, Rectangle area)
        {
            if (parentNode != null)
            {
                this.parentNode = parentNode;
                this.generation = (byte)(parentNode.Generation + 1);
            }
            else
            {
                this.parentNode = null;
                this.generation = 0;
            }
            this.content = new List<T>();
            this.partitioned = false;
            this.area = area;
            this.tree = tree;
        }

        #endregion Initialize

        #region Query

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Прямоугольник, в область которого должны входить элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Rectangle queryArea)
        {
            List<T> results = new List<T>();// Создаем список для найденых элементов
            // если запрашиваемая область полностью содержит этот узел,
            // то возвращаем содержимое всей ветви
            if (queryArea.Contains(this.Area))
            {
                results.AddRange(BranchContent);
                return results;
            }
            // если запрашиваемая область пересекает область этого узла,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (this.Area.IntersectsWith(queryArea))
            {
                foreach (T item in this.NodeContent)
                {
                    if (queryArea.IntersectsWith(item.Bounds))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryArea, results);
                    leftBottomNode.Query(ref queryArea, results);
                    rightBottomNode.Query(ref queryArea, results);
                    rightTopNode.Query(ref queryArea, results);
                }
            }
            return results;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Окружность, в область которой должны входить элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Circle queryArea)
        {
            List<T> results = new List<T>();// Создаем список для найденых элементов
            // если запрашиваемая область полностью содержит этот узел,
            // то возвращаем содержимое всей ветви
            if (queryArea.Contains(this.Area))
            {
                results.AddRange(BranchContent);
                return results;
            }
            // если запрашиваемая область пересекает область этого узла,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (this.Area.IntersectsWith(queryArea))
            {
                foreach (T item in this.NodeContent)
                {
                    if (queryArea.IntersectsWith(item.Bounds))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryArea, results);
                    leftBottomNode.Query(ref queryArea, results);
                    rightBottomNode.Query(ref queryArea, results);
                    rightTopNode.Query(ref queryArea, results);
                }
            }
            return results;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Точка, которую содержат элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Point queryPoint)
        {
            List<T> results = new List<T>(); // Создаем список для найденых элементов
            // если этот узел содержит точку,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (Area.Contains(queryPoint))
            {
                foreach (T item in NodeContent)
                {
                    if (item.Bounds.Contains(queryPoint))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryPoint, results);
                    leftBottomNode.Query(ref queryPoint, results);
                    rightBottomNode.Query(ref queryPoint, results);
                    rightTopNode.Query(ref queryPoint, results);
                }
            }
            return results;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Точка, которую содержат элементы</param>
        /// <returns>Список элементов</returns>
        public List<T> Query(Vector2 queryPoint)
        {
            List<T> results = new List<T>(); // Создаем список для найденых элементов
            // если этот узел содержит точку,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (Area.Contains(queryPoint))
            {
                foreach (T item in NodeContent)
                {
                    if (item.Bounds.Contains(queryPoint))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryPoint, results);
                    leftBottomNode.Query(ref queryPoint, results);
                    rightBottomNode.Query(ref queryPoint, results);
                    rightTopNode.Query(ref queryPoint, results);
                }
            }
            return results;
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Ссылка на прямоугольник, в область которого должны входить элементы</param>
        /// <param name="results">Возвращаемый список с элементами</param>
        public void Query(ref Rectangle queryArea, List<T> results)
        {
            // если запрашиваемая область полностью содержит этот узел,
            // то возвращаем содержимое всей ветви
            if (queryArea.Contains(this.Area))
            {
                results.AddRange(BranchContent);
                return;
            }
            // если запрашиваемая область пересекает область этого узла,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (this.Area.IntersectsWith(queryArea))
            {
                foreach (T item in this.NodeContent)
                {
                    if (queryArea.IntersectsWith(item.Bounds))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryArea, results);
                    leftBottomNode.Query(ref queryArea, results);
                    rightBottomNode.Query(ref queryArea, results);
                    rightTopNode.Query(ref queryArea, results);
                }
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryArea">Ссылка на окружность, в область которой должны входить элементы</param>
        /// <param name="results">Возвращаемый список с элементами</param>
        public void Query(ref Circle queryArea, List<T> results)
        {
            // если запрашиваемая область полностью содержит этот узел,
            // то возвращаем содержимое всей ветви
            if (queryArea.Contains(this.Area))
            {
                results.AddRange(BranchContent);
                return;
            }
            // если запрашиваемая область пересекает область этого узла,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (this.Area.IntersectsWith(queryArea))
            {
                foreach (T item in this.NodeContent)
                {
                    if (queryArea.IntersectsWith(item.Bounds))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryArea, results);
                    leftBottomNode.Query(ref queryArea, results);
                    rightBottomNode.Query(ref queryArea, results);
                    rightTopNode.Query(ref queryArea, results);
                }
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Ссылка на точку, которую содержат элементы</param>
        /// <param name="results">Возвращаемый список с элементами</param>
        public void Query(ref Point queryPoint, List<T> results)
        {
            // если этот узел содержит точку,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (Area.Contains(queryPoint))
            {
                foreach (T item in NodeContent)
                {
                    if (item.Bounds.Contains(queryPoint))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryPoint, results);
                    leftBottomNode.Query(ref queryPoint, results);
                    rightBottomNode.Query(ref queryPoint, results);
                    rightTopNode.Query(ref queryPoint, results);
                }
            }
        }

        /// <summary>Метод делает запрос на элементы</summary>
        /// <param name="queryPoint">Ссылка на точку, которую содержат элементы</param>
        /// <param name="results">Возвращаемый список с элементами</param>
        public void Query(ref Vector2 queryPoint, List<T> results)
        {
            // если этот узел содержит точку,
            // то проверяем содержимое этого узла, и опрашиваем подузлы
            if (Area.Contains(queryPoint))
            {
                foreach (T item in NodeContent)
                {
                    if (item.Bounds.Contains(queryPoint))
                        results.Add(item);
                }
                if (partitioned)
                {
                    leftTopNode.Query(ref queryPoint, results);
                    leftBottomNode.Query(ref queryPoint, results);
                    rightBottomNode.Query(ref queryPoint, results);
                    rightTopNode.Query(ref queryPoint, results);
                }
            }
        }

        /// <summary>Метод для нахождения узла, в котором находится известный элемент, в этой ветви дерева</summary>
        /// <param name="item">Элемент, узел которого требуется найти</param>
        /// <returns>Узел, в котором содержится элемент или null, если такого нет</returns>
        public QuadTreeNode<T> FindItemNode(T item)
        {
            // если элемент содержится в этом узле, то возвращаем ссылку на себя
            if (NodeContent.Contains(item))
                return this;
            else if (partitioned) // иначе проверяем подузлы
            {
                QuadTreeNode<T> n = null;
                // Проверяем только те узлы, которые могут содержать область элемента
                if (leftTopNode.Area.Contains(item.Bounds))
                {
                    n = leftTopNode.FindItemNode(item);
                }
                if (n == null && leftBottomNode.Area.Contains(item.Bounds))
                {
                    n = leftBottomNode.FindItemNode(item);
                }
                if (n == null && rightBottomNode.Area.Contains(item.Bounds))
                {
                    n = rightBottomNode.FindItemNode(item);
                }
                if (n == null && rightTopNode.Area.Contains(item.Bounds))
                {
                    n = rightTopNode.FindItemNode(item);
                }
                return n;
            }
            else return null;
        }

        #endregion Query

        #region Insertion

        /// <summary>Добавление элемента в ветку</summary>
        /// <param name="item">Элемент для добавления</param>
        public void Insert(T item)
        {
            // Если узел разбит на подузлы, попробовать поместить элемент в подузел
            if (!TryInsertInChild(item))
            {
                // подписываем узел на события элемента
                item.OnPositionChange += new PositionChangeHandler(ItemMove);
                item.OnRemoveFromScene += new RemoveFromSceneHandler(ItemRemove);
                NodeContent.Add(item);
            }
        }

        /// <summary>Метод пробует добавить элемент в один из подузлов этого узла</summary>
        /// <param name="item">Элемент для добавления в подузел</param>
        /// <returns>Результат операции</returns>
        protected bool TryInsertInChild(T item)
        {
            if (!partitioned && tree.MaxGeneration > this.Generation) // если узел не разбит на подузлы, и можно разбивать, то делаем разбиение
                Partition();
            if (partitioned)
            {
                if (leftTopNode.Area.Contains(item.Bounds))
                    leftTopNode.Insert(item);
                else if (rightTopNode.Area.Contains(item.Bounds))
                    rightTopNode.Insert(item);
                else if (leftBottomNode.Area.Contains(item.Bounds))
                    leftBottomNode.Insert(item);
                else if (rightBottomNode.Area.Contains(item.Bounds))
                    rightBottomNode.Insert(item);
                else return false; // вставка провалилась
                return true;
            }
            return false;
        }

        /// <summary>Метод пробует продвинуть элемент вниз по ветке дерева</summary>
        /// <param name="i">Индекс элемента в этом узле</param>
        /// <returns>Результат операции</returns>
        protected bool TryPushItemDown(int i)
        {
            if (TryInsertInChild(NodeContent[i])) // если вставка в подузел прошла успешно,
            {
                RemoveItem(i); // удаляем элемент из этого узла
                return true;
            }
            else return false;
        }

        /// <summary>Метод пробует продвинуть элемент вверз по ветке дерева</summary>
        /// <param name="i">Индекс элемента в этом узле</param>
        /// <returns>Результат операции</returns>
        protected bool TryPushItemUp(int i)
        {
            QuadTreeNode<T> node = parentNode;
            while (node != null) // пока выше есть узлы, пытаемся вставить элемент
            {
                if (node.Area.Contains(NodeContent[i].Bounds))
                {
                    node.Insert(NodeContent[i]);
                    RemoveItem(i);
                    return true;
                }
                node = node.parentNode; // не получилось, переходим выше по ветке
            }
            return false;
        }

        /// <summary>Разбиение узла на подузлы</summary>
        protected void Partition()
        {
            // защита от разбиения микроскопических узлов
            if ((Area.Height * Area.Width) <= 10)
                return;
            int halfWidth = (int)(Area.Width / 2);
            int halfHeight = (int)(Area.Height / 2);
            // создание четырех подузлов
            leftTopNode = new QuadTreeNode<T>(this, tree, new Rectangle(Area.Left, Area.Top, halfWidth, halfHeight));
            leftBottomNode = new QuadTreeNode<T>(this, tree, new Rectangle(Area.Left, Area.Top + halfHeight, halfWidth, halfHeight));
            rightTopNode = new QuadTreeNode<T>(this, tree, new Rectangle(Area.Left + halfWidth, Area.Top, halfWidth, halfHeight));
            rightBottomNode = new QuadTreeNode<T>(this, tree, new Rectangle(Area.Left + halfWidth, Area.Top + halfHeight, halfWidth, halfHeight));
            partitioned = true; // помечаем узел разбитым
        }

        #endregion Insertion

        #region Destroying

        /// <summary>Разрушает узел</summary>
        public void Destroy()
        {
            // Разрушаем все подузлы
            if (partitioned)
            {
                leftTopNode.Destroy();
                leftBottomNode.Destroy();
                rightTopNode.Destroy();
                rightBottomNode.Destroy();
                leftTopNode = null;
                leftBottomNode = null;
                rightTopNode = null;
                rightBottomNode = null;
            }
            // Удаляем все элементы
            while (NodeContent.Count > 0)
            {
                RemoveItem(0);
            }
        }

        /// <summary>Удаляет элемент из узла</summary>
        /// <param name="item">Элемент для удаления</param>
        public bool RemoveItem(T item)
        {
            item.OnPositionChange -= new PositionChangeHandler(ItemMove);
            item.OnRemoveFromScene -= new RemoveFromSceneHandler(ItemRemove);
            // Находим и удаляем элемент
            if (NodeContent.Contains(item)){

                NodeContent.Remove(item);
                return true;
            }
            else if (Area.Contains(item.Bounds))
            {
                if (leftBottomNode.RemoveItem(item)) return true;
                if ( rightBottomNode.RemoveItem(item)) return true;
                if ( leftTopNode.RemoveItem(item)) return true;
                if (rightTopNode.RemoveItem(item)) return true;
            }
            return false;
        }

        /// <summary>Удаляет элемент из узла по его индексу</summary>
        /// <param name="i">Индекс элемента для удаления</param>
        protected void RemoveItem(int i)
        {
            if (i < NodeContent.Count)
            {
                NodeContent[i].OnPositionChange -= new PositionChangeHandler(ItemMove);
                NodeContent[i].OnRemoveFromScene -= new RemoveFromSceneHandler(ItemRemove);
                NodeContent.RemoveAt(i);
            }
        }

        #endregion Destroying

        #region Events

        /// <summary>Событие при перемещении элемента по сцене</summary>
        /// <param name="sender">Объект отправитель</param>
        private void ItemRemove(object sender)
        {
            if (sender is T) // если отправитель является элементом дерева,
                RemoveItem((T)sender); // то удаляем его из узла
        }

        /// <summary>Событие при удалении элемента из сцены</summary>
        /// <param name="sender">Объект отправитель</param>
        private void ItemMove(object sender)
        {
            if (sender is T) // если отправитель является элементом дерева
            {
                T i = (T)sender;
                if (NodeContent.Contains(i))
                {
                    int index = NodeContent.IndexOf(i); // Находим элемент
                    // Пробуем продвинуть элемент вниз по дереву
                    if (!TryPushItemDown(index))
                    {
                        // если узел содержит элемент, то не надо ничего делать
                        if (!Area.Contains(i.Bounds))
                        {
                            // иначе продвигаем элемент наверх
                            if (!TryPushItemUp(index))
                            {
                                // при неудаче, расширяем область дерева
                                tree.Resize(new Rectangle(
                                    Math.Min(tree.Area.Left, i.Bounds.Left),
                                    Math.Min(tree.Area.Top, i.Bounds.Top),
                                    Math.Max(tree.Area.Right, i.Bounds.Right) - Math.Min(tree.Area.Left, i.Bounds.Left),
                                    Math.Max(tree.Area.Bottom, i.Bounds.Bottom) - Math.Min(tree.Area.Top, i.Bounds.Top)));
                            }
                        }
                    }
                }
                else
                {
                    // этот элемент не принадлежит узлу, удаляем подписки на события
                    i.OnPositionChange -= new PositionChangeHandler(ItemMove);
                    i.OnRemoveFromScene -= new RemoveFromSceneHandler(ItemRemove);
                }
            }
        }

        #endregion Events
    }

    public interface IQuadTreeItem
    {
        Rectangle Bounds { get; }

        event PositionChangeHandler OnPositionChange; // событие должно срабатывать при перемещении объекта по сцене
        event RemoveFromSceneHandler OnRemoveFromScene; // событие должно срабатывать при удалении объекта со сцены
    }
}