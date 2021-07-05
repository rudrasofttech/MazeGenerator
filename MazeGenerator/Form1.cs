﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeGenerator
{
    public partial class Form1 : Form
    {
        private int Xmin, Ymin, CellWid, CellHgt;

        private void CreateButton_Click(object sender, EventArgs e)
        {
            // Figure out the drawing geometry.
            int wid = int.Parse(WidthTextBox.Text);
            int hgt = int.Parse(HeightTextBox.Text);

            CellWid = picMaze.ClientSize.Width / (wid + 2);
            CellHgt = picMaze.ClientSize.Height / (hgt + 2);
            if (CellWid > CellHgt) CellWid = CellHgt;
            else CellHgt = CellWid;
            Xmin = (picMaze.ClientSize.Width - wid * CellWid) / 2;
            Ymin = (picMaze.ClientSize.Height - hgt * CellHgt) / 2;

            // Build the maze nodes.
            MazeNode[,] nodes = MakeNodes(wid, hgt);

            // Build the spanning tree.
            FindSpanningTree(nodes[0, 0]);

            // Display the maze.
            DisplayMaze(nodes);

            PrintDocument printDocument1 = new PrintDocument();
            printDocument1.OriginAtMargins = true;
            printDocument1.DocumentName = "Maze";
            printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            printDialog1.Document = printDocument1;
            

            if (printDialog1.ShowDialog() == DialogResult.OK)
                printDocument1.Print();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(picMaze.Image, 0, 0);
        }

        // Make the network of MazeNodes.
        private MazeNode[,] MakeNodes(int wid, int hgt)
        {
            // Make the nodes.
            MazeNode[,] nodes = new MazeNode[hgt, wid];
            for (int r = 0; r < hgt; r++)
            {
                int y = Ymin + CellHgt * r;
                for (int c = 0; c < wid; c++)
                {
                    int x = Xmin + CellWid * c;
                    nodes[r, c] = new MazeNode(
                        x, y, CellWid, CellHgt);
                }
            }

            // Initialize the nodes' neighbors.
            for (int r = 0; r < hgt; r++)
            {
                for (int c = 0; c < wid; c++)
                {
                    if (r > 0)
                        nodes[r, c].Neighbors[MazeNode.North] = nodes[r - 1, c];
                    if (r < hgt - 1)
                        nodes[r, c].Neighbors[MazeNode.South] = nodes[r + 1, c];
                    if (c > 0)
                        nodes[r, c].Neighbors[MazeNode.West] = nodes[r, c - 1];
                    if (c < wid - 1)
                        nodes[r, c].Neighbors[MazeNode.East] = nodes[r, c + 1];
                }
            }

            // Return the nodes.
            return nodes;
        }

        // Build a spanning tree with the indicated root node.
        private void FindSpanningTree(MazeNode root)
        {
            Random rand = new Random();

            // Set the root node's predecessor so we know it's in the tree.
            root.Predecessor = root;

            // Make a list of candidate links.
            List<MazeLink> links = new List<MazeLink>();

            // Add the root's links to the links list.
            foreach (MazeNode neighbor in root.Neighbors)
            {
                if (neighbor != null)
                    links.Add(new MazeLink(root, neighbor));
            }

            // Add the other nodes to the tree.
            while (links.Count > 0)
            {
                // Pick a random link.
                int link_num = rand.Next(0, links.Count);
                MazeLink link = links[link_num];
                links.RemoveAt(link_num);

                // Add this link to the tree.
                MazeNode to_node = link.ToNode;
                link.ToNode.Predecessor = link.FromNode;

                // Remove any links from the list that point
                // to nodes that are already in the tree.
                // (That will be the newly added node.)
                for (int i = links.Count - 1; i >= 0; i--)
                {
                    if (links[i].ToNode.Predecessor != null)
                        links.RemoveAt(i);
                }

                // Add to_node's links to the links list.
                foreach (MazeNode neighbor in to_node.Neighbors)
                {
                    if ((neighbor != null) && (neighbor.Predecessor == null))
                        links.Add(new MazeLink(to_node, neighbor));
                }
            }
        }

        // Display the maze in the picMaze PictureBox.
        private void DisplayMaze(MazeNode[,] nodes)
        {
            int hgt = nodes.GetUpperBound(0) + 1;
            int wid = nodes.GetUpperBound(1) + 1;
            Bitmap bm = new Bitmap(
                picMaze.ClientSize.Width,
                picMaze.ClientSize.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                for (int r = 0; r < hgt; r++)
                {
                    for (int c = 0; c < wid; c++)
                    {
                        //nodes[r, c].DrawCenter(gr, Brushes.Red);
                        nodes[r, c].DrawWalls(gr, Pens.Black);
                        //nodes[r, c].DrawNeighborLinks(gr, Pens.Black);
                        //nodes[r, c].DrawBoundingBox(gr, Pens.Blue);
                        //nodes[r, c].DrawPredecessorLink(gr, Pens.LightGray);
                    }
                }
            }

            picMaze.Image = bm;
        }

        public Form1()
        {
            InitializeComponent();
        }

    }
}
