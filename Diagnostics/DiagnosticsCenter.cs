using Blueberry.Graphics;
using Blueberry.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Blueberry.Diagnostics
{
    public class DiagnosticsCenter : IDiagnosable
    {
        private static DiagnosticsCenter instance;
        public static DiagnosticsCenter Instance
        {
            get
            {
                if (instance == null)
                    instance = new DiagnosticsCenter();
                return instance;
            }
        }

        List<IDiagnosable> objects;
        StringBuilder buffer;

        int fps, ups;
        int fps_counter, ups_counter;
        float update_time_counter;
        float draw_time_counter;
        static internal BitmapFont font;
        RectangleF drawArea;
        DebugGraph fpsGraph;
        DebugGraph upsGraph;
        DebugGraph udtGraph;
        DebugGraph fdtGraph;
        DebugGraph memoryGraph;
        List<DebugGraph> graphs;
        long memory;
        string memStr;
        float memoryCounter = 0;
        private string[] sizes = { "B", "KB", "MB", "GB" };

        public DiagnosticsCenter()
        {
            objects = new List<IDiagnosable>();
            Add(this);
            buffer = new StringBuilder(1024);
            if(font == null)
                font = new BitmapFont(new Font("Consolas", 14));
            graphs = new List<DebugGraph>();

            fpsGraph = new DebugGraph("FPS", new Rectangle(250, 30, 200, 60)) { ValuesByX = 20, ApproximateGraduation = 1 };
            upsGraph = new DebugGraph("UPS", new Rectangle(250, 100, 200, 60)) { ValuesByX = 20, ApproximateGraduation = 1 };
            fdtGraph = new DebugGraph("fdt", new Rectangle(250, 170, 200, 60)) { ValuesByX = 120, ApproximateGraduation = 0.01f };
            udtGraph = new DebugGraph("udt", new Rectangle(250, 240, 200, 60)) { ValuesByX = 120, ApproximateGraduation = 0.01f };
            memoryGraph = new DebugGraph("mem", new Rectangle(250, 310, 200, 60)) { ValuesByX = 20, ApproximateGraduation = 1f };

            graphs.Add(fpsGraph); graphs.Add(upsGraph); graphs.Add(fdtGraph); graphs.Add(udtGraph); graphs.Add(memoryGraph);
            Init();
        }

        public void Add(IDiagnosable obj)
        {
            if (!objects.Contains(obj))
                objects.Add(obj);
        }
        public void Remove(IDiagnosable obj)
        {
            objects.Remove(obj);
        }

        private void Init()
        {
            instance = this;
            drawArea.Width = 650;
            drawArea.Height = 450;
        }
        public void UpdateBuffer()
        {
        	int lines = 0;
            buffer.Clear();
            for (int i = 0; i < objects.Count; i++)
            {
                buffer.Append("["+objects[i].DebugName+"]");
                buffer.Append(";"); lines ++;
                string temp;
                int j = 0;
                while ((temp = objects[i].DebugInfo(j++)) != ";") 
                {
                	buffer.Append("   ");
                	buffer.Append(temp);
                	buffer.Append(";");
                	lines++;
                }
            }
            drawArea.Height = Math.Max(lines*font.LineSpacing, graphs.Count*80)+20;
            if (state == PanelState.hide)
                drawArea.Y = -drawArea.Height - 10;
        }
        public void Update(float dt)
        {
            udtGraph.AddValue(dt);
            update_time_counter += dt;
            ups_counter++;
            if (update_time_counter >= 1)
            {
                update_time_counter -= 1;
                ups = ups_counter;
                ups_counter = 0;
                upsGraph.AddValue(ups);
            }
            memoryCounter += dt;
            if (memoryCounter >= 0.5f)
            {
                memoryCounter = 0;
                memory = GC.GetTotalMemory(false);

                var usedHeap = (double)memory;

                int order = 0;
                while (usedHeap >= 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    usedHeap = usedHeap / 1024;
                }
                memoryGraph.AddValue((float)usedHeap);

                memStr = String.Format("{0:0.###} {1}", usedHeap, sizes[order]);

            }

            UpdateBuffer();
            foreach (var item in graphs)
                item.Update(dt);

            if (state == PanelState.showing)
            {
                if(Math.Abs(drawArea.Y - 10) < 0.1f)
                {
                    drawArea.Y = 10;
                    state = PanelState.shown;
                }
                else
                {
                    drawArea.Y = MathUtils.Lerp(drawArea.Y, 10, dt*8);
                }
            }
            if(state == PanelState.hiding)
            {
                if (Math.Abs(drawArea.Y + drawArea.Height + 10) < 0.1f)
                {
                    drawArea.Y = -drawArea.Height - 10;
                    state = PanelState.hide;
                }
                else
                {
                    drawArea.Y = MathUtils.Lerp(drawArea.Y, -drawArea.Height - 10, dt*8);
                }
            }
            
        }

        private enum PanelState { shown, hide, showing, hiding }
        private PanelState state = PanelState.hide;
        public bool Visible { get { return state == PanelState.shown; } }
        public void Show()
        {
            if (state == PanelState.hide)
            {
                state = PanelState.showing;
            }
        }
        public void Hide()
        {
            if (state == PanelState.shown)
            {
                state = PanelState.hiding;
            }
        }

        
        public void Draw(float dt)
        {
            fdtGraph.AddValue(dt);
            draw_time_counter += dt;
            fps_counter++;
            if (draw_time_counter >= 1f)
            {
                draw_time_counter -= 1f;
                fps = fps_counter;
                fps_counter = 0;
                fpsGraph.AddValue(fps);
            }
            SpriteBatch.Instance.FrameCheckPoint();
            if (state == PanelState.hide) return;

            SpriteBatch.Instance.Begin();
            SpriteBatch.Instance.FillRectangle(drawArea, new OpenTK.Graphics.Color4(0, 0, 0, 0.7f));

            int start = 0, line = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == ';')
                {
                    SpriteBatch.Instance.PrintText(font, buffer.ToString(start, i - start), drawArea.Left + 10, drawArea.Top + 10 + line * font.LineSpacing, Color.White, 0, 1f);
                    line++;
                    start = ++i;
                }
            }
            foreach (var item in graphs)
                item.Draw(dt, drawArea.X, drawArea.Y);
            SpriteBatch.Instance.End();
            
        }

        public string DebugInfo(int i)
        {
			switch (i) {
				case 0: return "Mem: " + memStr;
				default: return ";";
			}
        }

        public string DebugName
        {
            get { return "General"; }
        }
    }
}
