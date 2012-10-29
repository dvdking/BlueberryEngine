using Blueberry.Graphics;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Blueberry.Diagnostics
{
    class DebugGraph
    {
        int width = 10;
        int height;
        float rangeY;
        float maxVal = float.NegativeInfinity;
        float minVal = float.PositiveInfinity;
        List<float> values;
        public Rectangle rect;

        public int ValuesByX { get; set; }
        public float ApproximateGraduation { get; set; }

        public DebugGraph(Rectangle rect)
        {
            this.rect = rect;
            values = new List<float>();
        }
        public void AddValue(float value)
        {
            
            values.Add(value);
            if (values.Count > ValuesByX+1)
                values.RemoveAt(0);

            minVal = values.Min() - ApproximateGraduation;
            maxVal = values.Max() + ApproximateGraduation;
            
        }
        public void Update(float dt)
        {
            
        }
        public void Draw(float dt, float offsetX, float offsetY)
        {
            RectangleF r = rect;
            r.Offset(offsetX, offsetY);
            float ratio = r.Height / r.Width;
            float costy = rect.Height / (maxVal - minVal);
            float costx = r.Width / ValuesByX;
            SpriteBatch.Instance.FillRectangle(r, new Color4(0, 0, 0, 0.7f));
            //SpriteBatch.Instance.dr values.Skip(values.Count - ValuesByX);
            if(values.Count > 1)
            for (int i = values.Count-1, j = 0; i >= 1; i--, j++)
            {
                SpriteBatch.Instance.DrawLine(r.Right - (j * costx), r.Bottom - (values[i] - minVal) * costy, r.Right - (j + 1)*costx, r.Bottom - (values[i - 1] - minVal) * costy, Color4.Red, 1);
            }
            SpriteBatch.Instance.PrintText(DiagnosticsCenter.font, minVal.ToString(), r.X, r.Bottom, Color.LightYellow, 0, 0.7f, 0, 1f);
            SpriteBatch.Instance.PrintText(DiagnosticsCenter.font, maxVal.ToString(), r.X, r.Top, Color.LightBlue, 0, 0.7f, 0, 0f);
        }
    }
}
