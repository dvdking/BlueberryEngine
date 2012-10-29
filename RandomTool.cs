using OpenTK;
using OpenTK.Graphics;
using System;

namespace Blueberry
{
    public static class RandomTool
    {
        public static Random random = new Random((int)DateTime.Now.Ticks);

        public static int RandInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int RandInt(int max)
        {
            if (max >= 0)
                return random.Next(max);
            throw new ArgumentOutOfRangeException("max", "must be greater or equal than 0");
        }

        public static int RandInt()
        {
            return random.Next();
        }

        public static char ChooseRandom(params char[] objects)
        {
            return objects[RandInt(objects.Length)];
        }

        public static string ChooseRandom(params string[] objects)
        {
            return objects[RandInt(objects.Length)];
        }

        public static int ChooseRandom(params int[] objects)
        {
            return objects[RandInt(objects.Length)];
        }

        public static byte RandByte()
        {
            return (byte)random.Next();
        }
        public static byte RandByte(byte max)
        {
            return (byte)random.Next(max);
        }
        public static byte RandByte(byte min, byte max)
        {
            return (byte)random.Next(min, max);
        }

        public static double RandDouble()
        {
            return random.NextDouble();
        }

        public static float RandFloat()
        {
            return (float)random.NextDouble();
        }

        public static float RandFloat(float min, float max)
        {
            return (max - min) * RandFloat() + min;
        }

        public static float RandFloat(Range range)
        {
            return range.Size * RandFloat() + range.Minimum;
        }

        public static bool RandBool(float ratio)
        {
            return random.NextDouble() <= ratio;
        }

        public static bool RandBool(double ratio)
        {
            return random.NextDouble() <= ratio;
        }

        public static bool RandBool()
        {
            return random.NextDouble() <= 0.5;
        }

        public static sbyte RandSign()
        {
            return random.NextDouble() <= 0.5 ? (sbyte)1 : (sbyte)-1;
        }
        static public Color4 RandColor(ColourRange range)
        {
            return new Color4(RandomTool.RandFloat(range.Red), RandomTool.RandFloat(range.Green), RandomTool.RandFloat(range.Blue), 1f);
        }
        static public Color4 RandColor()
        {
            return new Color4(RandomTool.RandFloat(), RandomTool.RandFloat(), RandomTool.RandFloat(), 1f);
        }
        static public Vector2 NextUnitVector2()
        {
            float radians = RandomTool.RandFloat(-MathHelper.Pi, MathHelper.Pi);
            return new Vector2((float)Math.Cos(radians),(float)Math.Sin(radians));
        }
        static public Vector3 NextUnitVector3()
        {
            //Algorithm documented here http://www.cgafaq.info/wiki/Random_Points_On_Sphere
            Single radians = RandomTool.RandFloat(-MathHelper.Pi, MathHelper.Pi);

            Single z = RandomTool.RandFloat(-1f, 1f);

            Single t = (float)Math.Sqrt(1f - (z * z));

            Vector2 planar = new Vector2
            {
                X = (float)Math.Cos(radians) * t,
                Y = (float)Math.Sin(radians) * t
            };

            return new Vector3
            {
                X = planar.X,
                Y = planar.Y,
                Z = z
            };
        }
    }
}