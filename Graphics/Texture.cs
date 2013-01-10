using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Blueberry.Graphics
{
    public class Texture : IDisposable
    {
        #region Properties

        public bool IsDisposed { get; protected set; }

        public bool IsLocked { get; protected set; }
        
        protected ImageLockMode LockMode;

        public int ID { get; private set; }

        protected PixelInternalFormat PixelInternalFormat;

        public OpenTK.Graphics.OpenGL.PixelFormat PixelFormat { get; protected set; }

        public Color BorderColor
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                int[] color = new int[4];
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureBorderColor, color);

                return Color.FromArgb(color[0], color[1], color[2], color[3]);
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);

                int[] color = new int[4];
                color[0] = value.A;
                color[1] = value.R;
                color[2] = value.G;
                color[3] = value.B;

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, color);
            }
        }

        public TextureMinFilter MinFilter
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);

                int value;
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMinFilter, out value);

                return (TextureMinFilter)value;
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)value);
            }
        }

        public TextureMagFilter MagFilter
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                int value;
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMagFilter, out value);

                return (TextureMagFilter)value;
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)value);
            }
        }

        public float AnisotropicFilter
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                float value;
                GL.GetTexParameter(TextureTarget.Texture2D, (GetTextureParameter)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, out value);
                return value;
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, value);
            }
        }

        public Size Size { get; protected set; }

        public byte[] Data { get; set; }

        public Rectangle Bounds { get { return new Rectangle(Point.Empty, Size); } }

        public TextureWrapMode HorizontalWrap
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                int value;
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureWrapS, out value);

                return (TextureWrapMode)value;
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)value);
            }
        }

        public TextureWrapMode VerticalWrap
        {
            get
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                int value;
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureWrapT, out value);

                return (TextureWrapMode)value;
            }
            set
            {
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)value);
            }
        }

        #endregion Properties

        #region Constructors

        public Texture()
        {
            int id;
            // Create handle
            GL.GenTextures(1, out id);
            ID = id;
            GL.BindTexture(TextureTarget.Texture2D, ID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)DefaultHorizontalWrapFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)DefaultVerticalWrapFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);

            PixelInternalFormat = PixelInternalFormat.Rgba;
            this.PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

            BorderColor = DefaultBorderColor;
        }

        public Texture(Size size)
            : this()
        {
            SetSize(size);
        }

        public Texture(string filename)
            : this()
        {
            LoadImage(filename);
        }

        public Texture(Stream stream)
            : this()
        {
            if (stream == null)
                return;

            FromStream(stream);

            stream.Close();
        }

        public Texture(Size size, OpenTK.Graphics.OpenGL.PixelFormat format)
            : this()
        {
            PixelFormat = format;
            SetSize(size);
        }

        public Texture(OpenTK.Graphics.OpenGL.PixelFormat format)
            : this()
        {
            PixelFormat = format;
        }

        #endregion Constructors

        public void Dispose()
        {
            GL.DeleteTexture(ID);
            ID = -1;

            IsDisposed = true;
            //GDevice.Textures.Remove(this);
        }

        public void GenerateMipmap()
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public override string ToString()
        {
            return String.Format("(id {0}) ({1}x{2})", ID, Size.Width, Size.Height);
        }

        public void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }

        public Vector2 GetTextureCoordinate(Vector2 point)
        {
            return new Vector2(point.X / (float)Size.Width, point.Y / (float)Size.Height);
        }

        public void MakePixelPerfect()
        {
            MagFilter = TextureMagFilter.Nearest;
            MinFilter = TextureMinFilter.Nearest;
            this.VerticalWrap = TextureWrapMode.ClampToEdge;
            this.HorizontalWrap = TextureWrapMode.ClampToEdge;
        }

        #region Image IO

        public bool LoadImage(string filename)
        {
            using (Stream stream = File.OpenRead(filename))
                return FromStream(stream);
        }

        public bool FromStream(Stream stream)
        {
            if (stream == null)
                return false;

            Bitmap bm = new Bitmap(stream);
            bool ret = FromBitmap(bm);

            if (bm != null)
                bm.Dispose();

            return ret;
        }

        public bool FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return false;

            GL.BindTexture(TextureTarget.Texture2D, ID);

            SetSize(bitmap.Size);

            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, this.PixelInternalFormat, data.Width, data.Height, 0,
                this.PixelFormat, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            return true;
        }

        public bool LoadImage(byte[] data)
        {
            if (data == null)
                return false;

            MemoryStream stream = new MemoryStream(data);
            bool ret = FromStream(stream);
            stream.Dispose();

            return ret;
        }

        public bool SaveToDisk(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            Bitmap bm = ToBitmap(new Rectangle(Point.Empty, Size));
            if (bm == null)
                return false;

            bm.Save(name, System.Drawing.Imaging.ImageFormat.Png);
            bm.Dispose();

            return true;
        }

        public Bitmap ToBitmap(Rectangle rectangle)
        {
            if (!Lock(ImageLockMode.ReadOnly, rectangle))
                return null;

            Bitmap bm = new Bitmap(rectangle.Width, rectangle.Height);

            System.Drawing.Imaging.BitmapData bmd = bm.LockBits(rectangle,
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Runtime.InteropServices.Marshal.Copy(Data, 0, bmd.Scan0, Data.Length);
            bm.UnlockBits(bmd);

            Unlock();

            return bm;
        }

        #endregion Image IO

        #region Blitting

        public Color[,] GetColors(Rectangle rectangle)
        {
            return DataToColor(rectangle);
        }

        public Color[,] GetColors()
        {
            return DataToColor(Bounds);
        }

        #endregion Blitting

        #region Locking / unlocking

        protected bool Lock(ImageLockMode mode)
        {
            return Lock(mode, new Rectangle(Point.Empty, Size));
        }

        protected bool Lock(ImageLockMode mode, Rectangle rectangle)
        {
            // No texture bounds
            if (ID == -1 || IsLocked)
                return false;

            Data = new byte[rectangle.Width * rectangle.Height * 4];

            LockMode = mode;
            IsLocked = true;

            if (mode == ImageLockMode.WriteOnly)
                return true;

            GL.BindTexture(TextureTarget.Texture2D, ID);

            // Get the whole texture
            if (rectangle == new Rectangle(Point.Empty, Size))
            {
                GL.GetTexImage<byte>(TextureTarget.Texture2D, 0, (OpenTK.Graphics.OpenGL.PixelFormat)PixelFormat, PixelType.UnsignedByte, Data);
            }
            else
            {
                GL.ReadPixels(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, (OpenTK.Graphics.OpenGL.PixelFormat)PixelFormat, PixelType.UnsignedByte, Data);
            }
            return true;
        }

        protected void Unlock()
        {
            if (!IsLocked)
            {
                IsLocked = false;
                return;
            }

            IsLocked = false;
            if (LockMode == ImageLockMode.ReadOnly)
                return;

            GL.BindTexture(TextureTarget.Texture2D, ID);

            // The below is almost OK. The problem is the GL_RGBA. On certain platforms, the GPU prefers that red and blue be swapped (GL_BGRA).
            // If you supply GL_RGBA, then the driver will do the swapping for you which is slow.
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat,
                    Size.Width, Size.Height,
                    0,
                    (OpenTK.Graphics.OpenGL.PixelFormat)PixelFormat,
                    PixelType.UnsignedByte,
                    Data);

            Data = null;
        }

        protected Color[,] DataToColor(Rectangle rectangle)
        {
            Lock(ImageLockMode.ReadOnly, rectangle);

            Color[,] colors = new Color[rectangle.Width, rectangle.Height];

            for (int y = rectangle.Top; y < rectangle.Height; y++)
            {
                for (int x = rectangle.Left; x < rectangle.Width; x++)
                {
                    int offset = y * rectangle.Width * 4 + x * 4;
                    colors[x, y] = Color.FromArgb(
                            Data[offset + 3],
                            Data[offset + 2],
                            Data[offset + 1],
                            Data[offset + 0]);
                }
            }
            Unlock();

            return colors;
        }

        protected void SetSize(Size size)
        {
            Size = size;
            Lock(ImageLockMode.WriteOnly, new Rectangle(Point.Empty, size));
            Data = null;
            Unlock();
        }

        #endregion Locking / unlocking

        #region Statics
        
        static public TextureMagFilter DefaultMagFilter = TextureMagFilter.Linear;
        static public TextureMinFilter DefaultMinFilter = TextureMinFilter.Linear;
        /*
        static public TextureMagFilter DefaultMagFilter = TextureMagFilter.Nearest;
        static public TextureMinFilter DefaultMinFilter = TextureMinFilter.Nearest;
        */
        static public Color DefaultBorderColor = Color.Black;
        static public TextureWrapMode DefaultHorizontalWrapFilter = TextureWrapMode.Repeat;//.ClampToEdge;
        static public TextureWrapMode DefaultVerticalWrapFilter = TextureWrapMode.Repeat;//ClampToEdge;

        public static bool CheckTextureSize(Size size)
        {
            string extensions = GL.GetString(StringName.Extensions);

            if (extensions.Contains("ARB_texture_non_power_of_two"))
                return true;

            if (size.Width != NextPowerOfTwo(size.Width) || size.Height != NextPowerOfTwo(size.Height))
                return false;

            return true;
        }

        protected static bool IsPowerOfTwo(int value)
        {
            return (value & (value - 1)) == 0;
        }

        protected static int NextPowerOfTwo(int input)
        {
            int value = 1;

            while (value < input)
                value <<= 1;

            return value;
        }

        public static Size GetNextPOT(Size size)
        {
            return new Size(NextPowerOfTwo(size.Width), NextPowerOfTwo(size.Height));
        }

        #endregion Statics
    }
}