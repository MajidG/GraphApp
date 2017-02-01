using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GraphApp
{
    public class Sommet
    {
        public Point pos;
        public string text;
        public Color color;
        public List<Sommet> succ = new List<Sommet>();
        public List<Sommet> pred = new List<Sommet>();
        private int size = 40;
        private static Pen pen = new Pen(Color.Gray, 2);
        private static Font font = new Font("Arial", 16);

        public Sommet(string text, Color color, Point pos)
        {
            this.text = text;
            this.color = color;
            this.pos = pos;
        }

        public void Paint(Graphics g)
        {
            Point dpos = new Point(pos.X - size / 2, pos.Y - size / 2);
            g.FillEllipse(new SolidBrush(color), new Rectangle(dpos, new Size(size, size)));
            g.DrawEllipse(pen, new Rectangle(dpos, new Size(size, size)));
            g.DrawString(text, font, Brushes.Black, pos.X - size / 4 - 5, pos.Y - size / 4);
        }

        public void AddArc(Sommet other)
        {
            if (this == other || succ.Contains(other) || other.pred.Contains(this))
                return;
            succ.Add(other);
            other.pred.Add(this);
        }

        public void RemoveArc(Sommet other)
        {
            if (this == other)
                return;
            if (succ.Contains(other))
            {
                succ.Remove(other);
                other.pred.Remove(this);
                return;
            }
            if (pred.Contains(other))
            {
                pred.Remove(other);
                other.succ.Remove(this);
                return;
            }
        }

        private void AddAllSucc(List<Sommet> list)
        {
            List<Sommet> nov = succ.Where(smt => !list.Contains(smt)).ToList();
            list.AddRange(nov);
            foreach (var smt in nov)
            {
                smt.AddAllSucc(list);
            }
        }

        private void AddAllPred(List<Sommet> list)
        {
            List<Sommet> nov = pred.Where(smt => !list.Contains(smt)).ToList();
            list.AddRange(nov);
            foreach (var smt in nov)
            {
                smt.AddAllPred(list);
            }
        }

        public List<Sommet> GetSuccessors()
        {
            List<Sommet> tmp = new List<Sommet>();
            tmp.Add(this);
            tmp.AddRange(succ);
            foreach (var smt in succ)
            {
                smt.AddAllSucc(tmp);
            }
            return tmp;
        }

        public List<Sommet> GetPredeccessors()
        {
            List<Sommet> tmp = new List<Sommet>();
            tmp.Add(this);
            tmp.AddRange(pred);
            foreach (var smt in pred)
            {
                smt.AddAllPred(tmp);
            }
            return tmp;
        }

        public void Translate(Point offset)
        {
            pos.Offset(offset);
        }

        public override string ToString()
        {
            return text + ":" + succ.Count + "," + pred.Count;
        }
    }
}