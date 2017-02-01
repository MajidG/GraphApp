using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace GraphApp
{
    internal enum Mode
    {
        Move,
        Add,
        Remove,
        Link,
        Unlink
    }

    public partial class Form1 : Form
    {
        private Mode mode = Mode.Move;
        private List<ToolStripButton> buttons = null;

        public Form1()
        {
            InitializeComponent();
            buttons = new List<ToolStripButton>(){toolStripButton1,toolStripButton2,
                toolStripButton4,toolStripButton5,toolStripButton6};
        }

        private List<Color> colors = new List<Color>() { Color.Red, Color.Green, Color.Violet, Color.Orange, Color.Blue,
        Color.Yellow, Color.Indigo};

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            foreach (var btn in buttons)
            {
                btn.Checked = false;
            }
            (sender as ToolStripButton).Checked = true;
            int val = int.Parse((sender as ToolStripButton).Tag.ToString());
            mode = (Mode)val;
        }

        private Sommet sel1 = null;

        private void graphCanvas1_MouseClick(object sender, MouseEventArgs e)
        {
            switch (mode)
            {
                case Mode.Move:
                    break;

                case Mode.Add:
                    Sommet sm = new Sommet("X" + (graphCanvas1.graph.Count + 1), Color.White, e.Location);
                    graphCanvas1.graph.Add(sm);
                    break;

                case Mode.Remove:
                    Sommet clicked = graphCanvas1.GetAt(e.Location);
                    if (clicked != null)
                    {
                        while (clicked.succ.Count > 0)
                            clicked.RemoveArc(clicked.succ[0]);
                        while (clicked.pred.Count > 0)
                            clicked.RemoveArc(clicked.pred[0]);
                        graphCanvas1.graph.Remove(clicked);
                    }
                    break;

                case Mode.Link:
                    clicked = graphCanvas1.GetAt(e.Location);
                    if (sel1 == null)
                        sel1 = clicked;
                    else
                    {
                        sel1.AddArc(clicked);
                        sel1 = null;
                    }
                    break;

                case Mode.Unlink:
                    clicked = graphCanvas1.GetAt(e.Location);
                    if (sel1 == null)
                        sel1 = clicked;
                    else
                    {
                        sel1.RemoveArc(clicked);
                        sel1 = null;
                    }
                    break;

                default:
                    break;
            }
            graphCanvas1.Invalidate();
        }

        private Point oldpos;

        private void graphCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && smove != null)
            {
                Point offset = new Point(e.X - oldpos.X, e.Y - oldpos.Y);
                smove.Translate(offset);
                oldpos = e.Location;
                graphCanvas1.Invalidate();
            }
        }

        private Sommet smove = null;
        private bool mouseDown = false;

        private void graphCanvas1_MouseDown(object sender, MouseEventArgs e)
        {
            smove = graphCanvas1.GetAt(e.Location);
            mouseDown = true;
            oldpos = e.Location;
        }

        private void graphCanvas1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            smove = null;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlWriterSettings st = new XmlWriterSettings();
                st.Indent = true;
                XmlWriter xr = XmlWriter.Create(saveFileDialog1.FileName, st);
                xr.WriteStartDocument();
                xr.WriteStartElement("graph");

                xr.WriteStartElement("sommets");
                foreach (var sm in graphCanvas1.graph)
                {
                    xr.WriteStartElement("sommet");
                    xr.WriteStartAttribute("X");
                    xr.WriteValue(sm.pos.X);
                    xr.WriteStartAttribute("Y");
                    xr.WriteValue(sm.pos.Y);
                    xr.WriteStartAttribute("text");
                    xr.WriteValue(sm.text);
                    xr.WriteEndElement();
                }
                xr.WriteEndElement();

                xr.WriteStartElement("arcs");
                foreach (var sm in graphCanvas1.graph)
                {
                    foreach (var other in sm.succ)
                    {
                        xr.WriteStartElement("arc");
                        xr.WriteStartAttribute("s1");
                        xr.WriteValue(sm.text);
                        xr.WriteStartAttribute("s2");
                        xr.WriteValue(other.text);
                        xr.WriteEndElement();
                    }
                }
                xr.WriteEndElement();

                xr.WriteEndElement();
                xr.WriteEndDocument();
                xr.Dispose();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var graph = graphCanvas1.graph;
                treeView1.Nodes.Clear();
                graphCanvas1.Clear();

                var xdoc = XDocument.Load(openFileDialog1.FileName);
                var xgraph = xdoc.Element("graph");
                foreach (var smt in xgraph.Element("sommets").Elements())
                {
                    Sommet sommet = new Sommet(smt.Attribute("text").Value, Color.White,
                            new Point(int.Parse(smt.Attribute("X").Value),
                            int.Parse(smt.Attribute("Y").Value)));
                    graph.Add(sommet);
                }
                foreach (var arc in xgraph.Element("arcs").Elements())
                {
                    string s1 = arc.Attribute("s1").Value;
                    string s2 = arc.Attribute("s2").Value;
                    Sommet sm1 = graph.Find(x => x.text == s1);
                    Sommet sm2 = graph.Find(x => x.text == s2);
                    sm1.AddArc(sm2);
                }
                graphCanvas1.Invalidate();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            var nodes = graphCanvas1.graph;
            var tmp = new List<Sommet>(nodes.ToArray());
            while (tmp.Count > 0)
            {
                Sommet sm1 = tmp[0];
                var snp = sm1.GetPredeccessors();
                var sns = sm1.GetSuccessors();
                var comp = snp.Intersect(sns);
                treeView1.Nodes.Add("comp " + (treeView1.Nodes.Count + 1)).Tag = comp.ToList();
                tmp.RemoveAll(smt => comp.Contains(smt));
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var comp = treeView1.SelectedNode.Tag as List<Sommet>;
            foreach (var smt in graphCanvas1.graph)
            {
                smt.color = Color.White;
            }
            foreach (var smt in comp)
            {
                smt.color = Color.Green;
            }
            graphCanvas1.Invalidate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            graphCanvas1.Clear();
        }
    }
}