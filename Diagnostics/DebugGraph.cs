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
        float maxVal = float.MinValue;
        float minVal = float.MaxValue;
        List<float> values;
        public Rectangle rect;

        public int ValuesByX { get; set; }
        public float ApproximateGraduation { get; set; }
        public string Name {get; private set; }
        public DebugGraph(string name, Rectangle rect)
        {
        	this.Name = name;
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
           	SpriteBatch.Please.FillRectangle(r, new Color4(0, 0, 0, 0.7f));
           
           	float current = 0;
 			if(values.Count > 0)
            {
 				current = (values.Last() - minVal) * costy;
                SpriteBatch.Please.DrawLine(r.Left, r.Bottom - current, r.Right + 50, r.Bottom - current, 1f, Color4.White);
 			}
            if(values.Count > 1)
            {

	            for (int i = values.Count-1, j = 0; i >= 1; i--, j++)
	                SpriteBatch.Please.DrawLine(r.Right - (j * costx), r.Bottom - (values[i] - minVal) * costy, r.Right - (j + 1)*costx, r.Bottom - (values[i - 1] - minVal) * costy, 2, Color4.Red);
            	
            }
            SpriteBatch.Please.PrintText(DiagnosticsCenter.font, minVal.ToString(), r.X, r.Bottom, Color.LightYellow, 0, 0.7f, 0, 1f);
            //SpriteBatch.Instance.PrintText(DiagnosticsCenter.font, Name, r.X, r.Top + r.Height / 2, Color.LightGray, 0, 0.7f, 0, 1f);
            SpriteBatch.Please.PrintText(DiagnosticsCenter.font, maxVal.ToString(), r.X, r.Top, Color.LightBlue, 0, 0.7f, 0, 0f);
            if(values.Count>0)
            SpriteBatch.Please.PrintText(DiagnosticsCenter.font, values.Last().ToString()+" "+Name, r.Right + 10,r.Bottom- current-DiagnosticsCenter.font.LineSpacing,Color4.White);
        }
    }
}
