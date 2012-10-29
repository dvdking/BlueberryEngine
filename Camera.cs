using System;
using System.Drawing;
using Blueberry.Geometry;
using OpenTK;

namespace Blueberry
{
    /// <summary>Используется для перемещения по 2D миру</summary>
    public class Camera
    {
        #region Variables

        private int scrWidth;
        private int scrHeight;
        private float _scale;
        private float _nextScale;
        private float _rotation;
        private float _nextRotation;
        private Vector2 _position;
        private Rectangle _bounds;
        private bool _smooth;
        private Vector2 _nextPosition;
        private float _moveSpeed;
        private bool _rumble;
        private double _rumbleTime;
        private float _rumbleAmount;
        private float _rumbleAmountStep;
        private int _pushesCount;
        private double _onePushTime;
        private double _temp;
        private Random rnd;
        private Rectangle _space;
        private Vector2 _origin;
        private bool _pixelPerfect;

        #endregion Variables

        #region Properties

        public bool PixelPerfect { get { return _pixelPerfect; } set { _pixelPerfect = value; } }

        public Vector2 Origin { get { return _origin; } set { _origin = value; } }

        /// <summary>Позиция относительно центра</summary>
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; _nextPosition = value; }
        }

        public float X
        {
            get { return _position.X; }
            set { _position.X = value; _nextPosition.X = value; }
        }

        public float Y
        {
            get { return _position.Y; }
            set { _position.Y = value; _nextPosition.Y = value; }
        }

        public Vector2 Top { get { return Vector2.Transform(-Vector2.UnitY, Quaternion.FromAxisAngle(Vector3.UnitZ, _rotation)); } }

        public float Scaling { get { return _scale; } set { _scale = Math.Max(value, 0); _nextScale = Math.Max(value, 0); } }

        /// <summary>Угол поворота воокруг Z</summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; _nextRotation = value; }
        }

        /// <summary>Дозволеное пространство движения</summary>
        public Rectangle AllowedSpace
        {
            get { return _space; }
            set { _space = value; }
        }

        /// <summary>Получает ограничивающую окружность камеры</summary>
        public Circle BoundingCircle
        {
            get { return new Circle((int)_position.X, (int)_position.Y, (int)Math.Round(Math.Sqrt(scrWidth / 2 * scrWidth / 2 + scrHeight / 2 * scrHeight / 2)) / _scale); }
        }

        /// <summary>Получает ограничивающий прямоугольник камеры</summary>
        public Rectangle BoundingRectangle
        {
            get { return _bounds; }
        }

        /// <summary>Определяет, будет ли перемещение камеры сглаженное</summary>
        public bool MoveSmooth
        {
            get { return _smooth; }
            set { _smooth = value; }
        }

        /// <summary>Получает или устанавливает скорость перемещения камеры</summary>
        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>Создает новую камеру</summary>
        /// <param name="screenWidth">Ширина экрана</param>
        /// <param name="screenHeight">Высота экрана</param>
        /// <param name="x">X координата позиции камеры</param>
        /// <param name="y">Y координата позиции камеры</param>
        /// <param name="moveSmooth">Определяет, будут ли движения камеры сглаженными</param>
        public Camera(int screenWidth, int screenHeight, float x, float y, bool moveSmooth)
        {
            this.scrWidth = screenWidth;
            this.scrHeight = screenHeight;
            this.rnd = new Random();
            this._moveSpeed = 1.5f;
            this._position.X = x;
            this._position.Y = y;
            this._nextPosition.X = x;
            this._nextPosition.Y = y;
            this._scale = 1.0f;
            this._nextScale = 1.0f;
            this._rotation = 0.0f;
            this._smooth = moveSmooth;
            this._space = Rectangle.Empty;
            this._origin = new Vector2(.5f);
            this._pixelPerfect = false;
        }

        /// <summary>Создает новую камеру</summary>
        /// <param name="screenWidth">Ширина экрана</param>
        /// <param name="screenHeight">Высота экрана</param>
        /// <param name="position">Позиция камеры</param>
        /// <param name="moveSmooth">Определяет, будут ли движения камеры сглаженными</param>
        public Camera(int screenWidth, int screenHeight, Vector2 position, bool moveSmooth)
            : this(screenWidth, screenHeight, position.X, position.Y, moveSmooth)
        {
        }

        /// <summary>Создает новую камеру</summary>
        /// <param name="screenWidth">Ширина экрана</param>
        /// <param name="screenHeight">Высота экрана</param>
        /// <param name="position">Позиция камеры</param>
        /// <param name="moveSmooth">Определяет, будут ли движения камеры сглаженными</param>
        public Camera(int screenWidth, int screenHeight, Point position, bool moveSmooth)
            : this(screenWidth, screenHeight, position.X, position.Y, moveSmooth)
        {
        }

        /// <summary>Создает новую камеру</summary>
        /// <param name="screen">Размер экрана</param>
        /// <param name="position">Позиция камеры</param>
        /// <param name="moveSmooth">Определяет, будут ли движения камеры сглаженными</param>
        public Camera(Size screen, Point position, bool moveSmooth)
            : this(screen.Width, screen.Height, position.X, position.Y, moveSmooth)
        {
        }

        /// <summary>Создает новую камеру</summary>
        /// <param name="screen">Размер экрана</param>
        /// <param name="position">Позиция камеры</param>
        /// <param name="moveSmooth">Определяет, будут ли движения камеры сглаженными</param>
        public Camera(Size screen, Vector2 position, bool moveSmooth)
            : this(screen.Width, screen.Height, position.X, position.Y, moveSmooth)
        {
        }

        #endregion Constructors

        #region Transforms

        public PointF ToWorld(PointF screenPoint, float parallax = 1)
        {
            Vector2 v = ToWorld(new Vector2(screenPoint.X, screenPoint.Y), parallax);
            return new Point((int)v.X, (int)v.Y);
        }

        public Vector2 ToWorld(float screenX, float screenY, float parallax = 1)
        {
            return ToWorld(new Vector2(screenX, screenY), parallax);
        }

        public Vector2 ToWorld(Vector2 screenPoint, float parallax = 1)
        {
            /*
            Vector2 shift = new Vector2(_origin.X * scrWidth, _origin.Y * scrHeight);

            screenPoint.X -= shift.X;
            screenPoint.Y -= shift.Y;

            screenPoint = Vector2.Transform(screenPoint, Quaternion.FromAxisAngle(Vector3.UnitZ, -_rotation));//Vector3.Transform(new Vector3(screenPoint), Matrix4.CreateRotationZ(-_rotation)).Xy;
            screenPoint /= _scale;

            //screenPoint.X += shift.X;
            //screenPoint.Y += shift.Y;

            screenPoint.X += _position.X * parallax;
            screenPoint.Y += _position.Y * parallax;
             */
            return screenPoint.Transform(Matrix4.Invert(GetViewMatrix(parallax)));
            //return screenPoint;
        }

        public Vector2 ToScreen(Vector2 worldPoint, float parallax = 1)
        {
            /*
            Vector2 shift = new Vector2(_origin.X * scrWidth, _origin.Y * scrHeight);

            worldPoint.X -= _position.X * parallax;
            worldPoint.Y -= _position.Y * parallax;
            //worldPoint.X -= shift.X;
            //worldPoint.Y -= shift.Y;

            worldPoint = Vector2.Transform(worldPoint, Quaternion.FromAxisAngle(Vector3.UnitZ, -_rotation));
            worldPoint *= _scale;

            worldPoint.X += shift.X;
            worldPoint.Y += shift.Y;
            */
            return worldPoint.Transform(GetViewMatrix(parallax));
            //return worldPoint;
        }

        public Point ToScreen(Point worldPoint, float parallax = 1)
        {
            Vector2 v = ToScreen(new Vector2(worldPoint.X, worldPoint.Y), parallax);
            return new Point((int)v.X, (int)v.Y);
        }

        public Vector2 ToScreen(float worldX, float worldY, float parallax = 1)
        {
            return ToScreen(new Vector2(worldX, worldY), parallax);
        }

        public Vector2 ParallaxTransform(Vector2 point, float sourceParallax, float dectParallax)
        {
            Vector2 screen = point -= _position * sourceParallax;
            Vector2 res = point += _position * dectParallax;
            return res;
        }

        #endregion Transforms

        public void Update(float elapsed)
        {
            if (_smooth)
            {
                _position = Vector2.Lerp(_position, _nextPosition, (float)(_moveSpeed * elapsed));
                this._scale = MathUtils.Lerp(_scale, _nextScale, (float)(_moveSpeed * elapsed));
                this._rotation = MathUtils.Lerp(_rotation, _nextRotation, (float)(_moveSpeed * elapsed));
            }
            else
            {
                this._position.X = _nextPosition.X;
                this._position.Y = _nextPosition.Y;
                this._scale = _nextScale;
                this._rotation = _nextRotation;
            }
            if (_space != Rectangle.Empty)
            {
                _position.X = Math.Min(Math.Max(_space.X + scrWidth / 2, _position.X), _space.Right - scrWidth / 2);
                _position.Y = Math.Min(Math.Max(_space.Y + scrHeight / 2, _position.Y), _space.Bottom - scrHeight / 2);
            }
            if (_rumble)
            {
                if (_rumbleTime >= 0)
                {
                    _rumbleTime -= elapsed;
                    Push(elapsed);
                }
                else
                    _rumble = false;
            }
            // установка начального прямоугольника согласно position
            _bounds.X = (int)((Position.X - scrWidth / 2));
            _bounds.Y = (int)((Position.Y - scrHeight / 2));
            _bounds.Width = (int)(scrWidth);
            _bounds.Height = (int)(scrHeight);
            // масштабирование согласно scale
            _bounds.X += (int)((_bounds.Width - (_bounds.Width / _scale)) / 2);
            _bounds.Y += (int)((_bounds.Height - (_bounds.Height / _scale)) / 2);
            _bounds.Width = (int)(_bounds.Width / _scale);
            _bounds.Height = (int)(_bounds.Height / _scale);
            // поворот согласно rotate
            if (_rotation % MathHelper.TwoPi != 0)
            {
                Vector2 lt = new Vector2(-_bounds.Width / 2, -_bounds.Height / 2);
                Vector2 rt = new Vector2(_bounds.Width / 2, -_bounds.Height / 2);
                Vector2 rb = new Vector2(_bounds.Width / 2, _bounds.Height / 2);
                Vector2 lb = new Vector2(-_bounds.Width / 2, _bounds.Height / 2);
                Quaternion rot = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), _rotation);

                Vector2.Transform(ref lt, ref rot, out lt);
                Vector2.Transform(ref rt, ref rot, out rt);
                Vector2.Transform(ref rb, ref rot, out rb);
                Vector2.Transform(ref lb, ref rot, out lb);
                lt.X += _bounds.X + _bounds.Width / 2;
                lt.Y += _bounds.Y + _bounds.Height / 2;
                rt.X += _bounds.X + _bounds.Width / 2;
                rt.Y += _bounds.Y + _bounds.Height / 2;
                rb.X += _bounds.X + _bounds.Width / 2;
                rb.Y += _bounds.Y + _bounds.Height / 2;
                lb.X += _bounds.X + _bounds.Width / 2;
                lb.Y += _bounds.Y + _bounds.Height / 2;
                _bounds.X = (int)Math.Round(Math.Min(Math.Min(lt.X, rt.X), Math.Min(rb.X, lb.X)));
                _bounds.Y = (int)Math.Round(Math.Min(Math.Min(lt.Y, rt.Y), Math.Min(rb.Y, lb.Y)));
                _bounds.Width = (int)Math.Round(Math.Max(Math.Max(lt.X, rt.X), Math.Max(rb.X, lb.X))) - _bounds.X;
                _bounds.Height = (int)Math.Round(Math.Max(Math.Max(lt.Y, rt.Y), Math.Max(rb.Y, lb.Y))) - _bounds.Y;
            }
        }

        public Matrix4 GetViewMatrix(float parallax = 1)
        {
            Vector2 pos = _position;
            Vector2 shift = new Vector2(_origin.X * scrWidth, _origin.Y * scrHeight);
            if (_pixelPerfect)
            {
                pos.X = (int)Math.Round(pos.X);
                pos.Y = (int)Math.Round(pos.Y);
                shift.X = (int)Math.Round(shift.X);
                shift.Y = (int)Math.Round(shift.Y);
            }
            return Matrix4.Identity *
                Matrix4.CreateTranslation(new Vector3(-pos.X * parallax, -pos.Y * parallax, 0)) *
                //Matrix4.CreateTranslation(new Vector3(-shift.X, -shift.Y, 0)) *
            Matrix4.CreateRotationZ(_rotation) *
            Matrix4.Scale(_scale) *
            Matrix4.CreateTranslation(new Vector3(shift.X, shift.Y, 0));
        }

        #region Translations

        public void MoveTo(Vector2 position)
        {
            this._nextPosition = position;
        }

        public void MoveTo(Point position)
        {
            this._nextPosition.X = position.X;
            this._nextPosition.Y = position.Y;
        }

        public void MoveTo(float x, float y)
        {
            this._nextPosition.X = x;
            this._nextPosition.Y = y;
        }

        public void Move(float stepX, float stepY)
        {
            _nextPosition.X += stepX;
            _nextPosition.Y += stepY;
        }

        public void Move(Vector2 step)
        {
            _nextPosition.X += step.X;
            _nextPosition.Y += step.Y;
        }

        public void Move(Point step)
        {
            _nextPosition.X += step.X;
            _nextPosition.Y += step.Y;
        }

        public void ScaleTo(float scale)
        {
            _nextScale = Math.Max(scale, 0);
        }

        public void ScaleOn(float amount)
        {
            _nextScale = Math.Max(_nextScale + amount, 0);
        }

        public void RotateTo(float angle)
        {
            _nextRotation = angle;
        }

        public void RotateOn(float amount)
        {
            _nextRotation += amount;
        }

        #endregion Translations

        #region Rumble

        private void Push(double dt)
        {
            _temp += dt;
            if (_temp >= _onePushTime)
            {
                short diapazon;
                if (rnd.Next(0, 2) == 1)
                    diapazon = -1;
                else
                    diapazon = 1;

                Vector2 direction = new Vector2((float)rnd.NextDouble() * diapazon, (float)rnd.NextDouble() * diapazon);
                direction.Normalize();

                _rumbleAmount += _rumbleAmountStep;
                direction *= (_rumbleAmount);

                _position += direction;

                _temp -= _onePushTime;
            }
        }

        public void Rumble(double seconds, float amount, int pushesCount)
        {
            _rumble = true;
            _rumbleTime = seconds;
            _rumbleAmount = amount;
            this._pushesCount = pushesCount;
            this._rumbleAmountStep = 0;
        }

        public void Rumble(double seconds, float startAmount, float endAmount, int pushesCount)
        {
            _rumble = true;
            _rumbleTime = seconds;
            _rumbleAmount = startAmount;
            this._pushesCount = pushesCount;
            this._onePushTime = seconds / pushesCount;
            this._rumbleAmountStep = (endAmount - startAmount) / pushesCount;
        }

        #endregion Rumble
    }
}