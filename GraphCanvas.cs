using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GraphApp
{
    public partial class GraphCanvas : UserControl
    {
        public GraphCanvas()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            arrow.AddPolygon(new Point[] { new Point(0, 15), new Point(7, 0), new Point(-7, 0) });
        }

        public List<Sommet> graph = new List<Sommet>();
        private Pen pen = new Pen(Color.Black, 2);
        private GraphicsPath arrow = new GraphicsPath();

        public void Clear()
        {
            graph.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Point[] pnts = new Point[3];
            base.OnPaint(pe);
            Graphics gh = pe.Graphics;
            gh.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gh.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gh.Clear(Color.White);
            foreach (var sm in graph)
                foreach (var other in sm.succ)
                {
                    pnts[0] = sm.pos;
                    pnts[2] = other.pos;
                    int dx = other.pos.X - sm.pos.X;
                    int dy = other.pos.Y - sm.pos.Y;
                    int dist = (int)Math.Sqrt(dx * dx + dy * dy);
                    double teta = Math.Atan2(dy, dx);
                    pnts[1].X = (sm.pos.X + other.pos.X) / 2 + (int)(Math.Cos(teta + Math.PI / 2) * dist / 10);
                    pnts[1].Y = (sm.pos.Y + other.pos.Y) / 2 + (int)(Math.Sin(teta + Math.PI / 2) * dist / 10);
                    gh.DrawCurve(pen, pnts);
                    gh.TranslateTransform(pnts[1].X, pnts[1].Y);
                    gh.RotateTransform((float)(teta * 180 / Math.PI - 90));
                    gh.FillPath(Brushes.Black, arrow);
                    gh.ResetTransform();
                }
            foreach (var sm in graph)
                sm.Paint(gh);
        }

        public Sommet GetAt(Point pos)
        {
            for (int i = graph.Count - 1; i >= 0; i--)
            {
                Sommet sm = graph[i];
                double dist = Math.Sqrt(Math.Pow(pos.X - sm.pos.X, 2) + Math.Pow(pos.Y - sm.pos.Y, 2));
                if (dist < 40)
                    return sm;
            }
            return null;
        }
    }
}