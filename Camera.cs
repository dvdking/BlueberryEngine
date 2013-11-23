using System;
using System.Drawing;
using Blueberry.Geometry;
using OpenTK;

namespace Blueberry
{
    /// <summary>Используется для перемещения по 2D миру</summary>
    public class Camera
    {
        private struct Rumble
        {
            public bool enable;
            public float time;
            public float pushInterval;
            public float amount;
            public float step;
            public float pushTime;
        }
        #region Variables

        private int scrWidth;
        private int scrHeight;
        private float _scale;
        private float _nextScale;
        private float _minScale;
        private float _maxScale;
        private float _rotation;
        private float _nextRotation;
        private Vector2 _position;
        private Rectangle _bounds;
        private bool _smooth;
        private Vector2 _nextPosition;
        private float _moveSpeed;

        private Rumble _positionRumble;
        private Rumble _rotationRumble;
        private sbyte _lastRotationRumbleSign = 1;

        private Rumble _scaleRumble;
        private sbyte _lastScaleRumbleSign  = 1;
		
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

        public float Scaling { get { return _scale; } set { _scale = MathUtils.Clamp(value, _minScale, _maxScale); _nextScale = MathUtils.Clamp(value, _minScale, _maxScale); } }
		
        public float MinScale
        {
        	get { return _minScale; }
        	set 
        	{
        		if(_space != Rectangle.Empty)
        		{
        			float minScale = Math.Max((float)scrWidth / (float)_space.Width, (float)scrHeight / (float)_space.Height);
        			if(value > _minScale)
        				_minScale = value;
        			else
        				_minScale = Math.Max(minScale, value);
        		}
        		else
        			_minScale = Math.Max(0, value);
        	} 
        }
        public float MaxScale
        {
        	get { return _maxScale; }
        	set
        	{
        		_maxScale = Math.Max(_minScale, value);
        	}
        }
        
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
            set { _space = value; MinScale = _minScale; MaxScale = _maxScale; }
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
            this._origin = new Vector2(.5f, .5f);
            this._pixelPerfect = false;
            this._minScale = 0;
            this._maxScale = float.PositiveInfinity;
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
            	//_scale = MathUtils.Clamp(_minScale, _maxScale);
                _nextScale = MathUtils.Clamp(_nextScale, _minScale, _maxScale);
                
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
            	/*
            	float minScale = Math.Max((float)scrWidth / (float)_space.Width, (float)scrHeight / (float)_space.Height);
            	if(_scale <= minScale)
            	{
            		_scale = minScale;
            		Console.WriteLine("{0}", _nextScale);
            		_nextScale = minScale;
            		Console.WriteLine("{0}", _nextScale);
            	}
            	*/
            	_position.X = MathUtils.Clamp(_position.X, _space.X + (scrWidth/2) / _scale, _space.Right - (scrWidth/2) / _scale);
            	_position.Y = MathUtils.Clamp(_position.Y, _space.Y + (scrHeight/2) / _scale, _space.Bottom - (scrHeight/2) / _scale);
            }
            UpdateRumble(elapsed);

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

        private void UpdateRumble(float dt)
        {
            if (_positionRumble.enable)
            {
                if (_positionRumble.time >= 0)
                {
                    _positionRumble.time -= dt;
                    _positionRumble.pushTime += dt;
                    if (_positionRumble.pushTime >= _positionRumble.pushInterval)
                    {
                        Vector2 direction = RandomTool.NextUnitVector2();
                        
                        _positionRumble.amount += _positionRumble.step;
                        direction *= (_positionRumble.amount);
                        
                        _position += direction;
                        
                        _positionRumble.pushTime -= _positionRumble.pushInterval;
                    }
                }
                else
                    _positionRumble.enable = false;
            }
            if (_rotationRumble.enable)
            {
                if (_rotationRumble.time >= 0)
                {
                    _rotationRumble.time -= dt;
                    _rotationRumble.pushTime += dt;
                    if (_rotationRumble.pushTime >= _rotationRumble.pushInterval)
                    {

                        _rotationRumble.amount += _rotationRumble.step;
                        
                        _rotation += _lastRotationRumbleSign * RandomTool.NextSingle(0, _rotationRumble.amount);  

                        _lastRotationRumbleSign = (sbyte)-_lastRotationRumbleSign;

                        _rotationRumble.pushTime -= _rotationRumble.pushInterval;
                    }
                }
                else
                    _rotationRumble.enable = false;
            }
            if (_scaleRumble.enable)
            {
                if (_scaleRumble.time >= 0)
                {
                    _scaleRumble.time -= dt;
                    _scaleRumble.pushTime += dt;
                    if (_scaleRumble.pushTime >= _scaleRumble.pushInterval)
                    {
                        
                        _scaleRumble.amount += _scaleRumble.step;
                        
                        _scale += _lastScaleRumbleSign * RandomTool.NextSingle(0, _scaleRumble.amount);  
                        _lastScaleRumbleSign = (sbyte)-_lastScaleRumbleSign;

                        _scaleRumble.pushTime -= _scaleRumble.pushInterval;
                    }
                }
                else
                    _scaleRumble.enable = false;
            }
        }

        public void RumblePosition(float seconds, float amount, int pushesCount)
        {
            RumblePosition(seconds, amount, amount, pushesCount);
        }

        public void RumblePosition(float seconds, float startAmount, float endAmount, int pushesCount)
        {
            _positionRumble.enable = true;
            _positionRumble.time = seconds;
            _positionRumble.amount = startAmount;
            _positionRumble.pushInterval = seconds / pushesCount;
            _positionRumble.step = (endAmount - startAmount) / pushesCount;
        }

        public void RumbleRotation(float seconds, float amount, int pushesCount)
        {
            RumbleRotation(seconds, amount, amount, pushesCount);
        }
        
        public void RumbleRotation(float seconds, float startAmount, float endAmount, int pushesCount)
        {
            _rotationRumble.enable = true;
            _rotationRumble.time = seconds;
            _rotationRumble.amount = startAmount;
            _rotationRumble.pushInterval = seconds / pushesCount;
            _rotationRumble.step = (endAmount - startAmount) / pushesCount;
        }
        public void RumbleScale(float seconds, float amount, int pushesCount)
        {
            RumbleScale(seconds, amount, amount, pushesCount);
        }
        
        public void RumbleScale(float seconds, float startAmount, float endAmount, int pushesCount)
        {
            _scaleRumble.enable = true;
            _scaleRumble.time = seconds;
            _scaleRumble.amount = startAmount;
            _scaleRumble.pushInterval = seconds / pushesCount;
            _scaleRumble.step = (endAmount - startAmount) / pushesCount;
        }

        #endregion Rumble
    }
}