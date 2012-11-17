using OpenTK;
using OpenTK.Graphics;
using System;

namespace Blueberry
{
    public static class RandomTool
    {
        public static Random random = new Random((int)DateTime.Now.Ticks);

        public static int NextInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int NextInt(int max)
        {
            if (max >= 0)
                return random.Next(max);
            throw new ArgumentOutOfRangeException("max", "must be greater or equal than 0");
        }

        public static int NextInt()
        {
            return random.Next();
        }

        public static char NextChoice(params char[] objects)
        {
            return objects[NextInt(objects.Length)];
        }

        public static string NextChoice(params string[] objects)
        {
            return objects[NextInt(objects.Length)];
        }

        public static int NextChoice(params int[] objects)
        {
            return objects[NextInt(objects.Length)];
        }

        public static byte NextByte()
        {
            return (byte)random.Next();
        }
        public static byte NextByte(byte max)
        {
            return (byte)random.Next(max);
        }
        public static byte NextByte(byte min, byte max)
        {
            return (byte)random.Next(min, max);
        }

        public static double NextDouble()
        {
            return random.NextDouble();
        }

        public static float NextSingle()
        {
            return (float)random.NextDouble();
        }

        public static float NextSingle(float min, float max)
        {
            return (max - min) * NextSingle() + min;
        }

        public static float NextSingle(Range range)
        {
            return range.Size * NextSingle() + range.Minimum;
        }

        public static bool NextBool(float ratio)
        {
            return random.NextDouble() <= ratio;
        }

        public static bool NextBool(double ratio)
        {
            return random.NextDouble() <= ratio;
        }

        public static bool NextBool()
        {
            return random.NextDouble() <= 0.5;
        }

        public static sbyte NextSign()
        {
            return random.NextDouble() <= 0.5 ? (sbyte)1 : (sbyte)-1;
        }
        static public Color4 NextColor(ColourRange range)
        {
            return new Color4(RandomTool.NextSingle(range.Red), RandomTool.NextSingle(range.Green), RandomTool.NextSingle(range.Blue), 1f);
        }
        static public Color4 NextColor()
        {
            return new Color4(RandomTool.NextSingle(), RandomTool.NextSingle(), RandomTool.NextSingle(), 1f);
        }
        static public Vector2 NextUnitVector2()
        {
            float radians = RandomTool.NextSingle(-MathHelper.Pi, MathHelper.Pi);
            return new Vector2((float)Math.Cos(radians),(float)Math.Sin(radians));
        }
        static public Vector3 NextUnitVector3()
        {
            //Algorithm documented here http://www.cgafaq.info/wiki/Random_Points_On_Sphere
            Single radians = RandomTool.NextSingle(-MathHelper.Pi, MathHelper.Pi);

            Single z = RandomTool.NextSingle(-1f, 1f);

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