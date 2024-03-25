using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoShop
{
    internal class OctTreeFilter : IFilter
    {
        public int[,]? Kernel { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public int colors { get; set; }

        public OctTreeFilter(int colors)
        {
            this.colors = colors;
        }

        public WriteableBitmap Apply(WriteableBitmap In)
        {
            int pxWidth = (int)In.Width;
            int pxHeight = (int)In.Height;

            int bytesPerPixel = (In.Format.BitsPerPixel + 7) / 8;
            int stride = (int)(pxWidth * bytesPerPixel);

            Byte[] pixels = new Byte[stride * pxHeight];
            In.CopyPixels(pixels, stride, 0);

            OctTree octTree = new OctTree(null, 0, colors);


            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    //creating and adding color to octTree
                    Color color = new Color(pixels[index], pixels[index + 1], pixels[index + 2]);
                    Add(octTree, color);

                    //checking if maxLeafs passed
                    while (octTree.leafCount > octTree.maxLeaves)
                        Reduce(octTree);
                }
            }

            

            for (int y = 0; y < pxHeight; y++)
            {
                for (int x = 0; x < pxWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    //finding collors using the tree 
                    Color color = new Color(pixels[index], pixels[index + 1], pixels[index + 2]);
                    (pixels[index], pixels[index + 1], pixels[index + 2]) = Find(octTree, color);

                   
                }
            }

            In.WritePixels(new Int32Rect(0, 0, pxWidth, pxHeight), pixels, stride, 0);
            return In;
        }

        private (byte R, byte G, byte B) Find(OctTree octTree, Color color)
        {
            var node = octTree.root;
            int depth = 0;
            while (!node.isLeaf)
            {
                int i = ChildIndex(color, depth);
                node = node.nodes[i];
                depth++;
            }
            byte R = (byte)(node.sumR / node.count);
            byte G = (byte)(node.sumG / node.count);
            byte B = (byte)(node.sumB / node.count);


            return (R,G,B);
        } 

        private void Add(OctTree octTree, Color color)
        {
            if (octTree.root == null)
                octTree.root = CreateNode(octTree, 0);
            AddRecursive(octTree, octTree.root, color, 0);
        }

        private void AddRecursive(OctTree octTree, Node parent, Color color, int depth)
        {
            if(parent.isLeaf)
            {
                parent.sumR += color.R;
                parent.sumG += color.G;
                parent.sumB += color.B;
                parent.count++;
            }
            else
            {
                int i = ChildIndex(color, depth);
                if (parent.nodes[i] == null)
                {
                    parent.nodes[i] = CreateNode(octTree, depth + 1);
                }
                AddRecursive(octTree, parent.nodes[i], color, depth + 1);
            }
        }

        private void Reduce(OctTree octTree)
        {
            for (int i = 7; i >= 0; i--)
            {
                if (octTree.innerNodes[i].Count == 0)
                {
                    continue;
                }
                Node node = SelectAndRemoveNode(octTree.innerNodes[i]);

                int removed = 0;
                for (int k = 0; k < 8; k++)
                {
                    if (node.nodes[k] != null)
                    {
                        node.sumR += node.nodes[k].sumR;
                        node.sumG += node.nodes[k].sumG;
                        node.sumB += node.nodes[k].sumB;
                        node.count += node.nodes[k].count;
                        node.nodes[k] = null;
                        removed++;
                    }
                }
                node.isLeaf = true;
                octTree.leafCount += 1 - removed;
                return; // Exit the method after processing the first non-empty list
            }


        }

        private int ChildIndex(Color color, int depth)
        {
            int bitR = (color.R >> (7 - depth)) & 0x1;
            int bitG = (color.G >> (7 - depth)) & 0x1;
            int bitB = (color.B >> (7 - depth)) & 0x1;
            return (bitR << 2) | (bitG << 1) | bitB;
        }

        private Node CreateNode(OctTree octTree, int depth)
        {
            Node newNode = new Node();
            newNode.isLeaf = (depth == 8);
            if (!newNode.isLeaf) //enter if no leaf
                octTree.innerNodes[depth].Add(newNode);
            else
                octTree.leafCount++;
            return newNode;
        }

        private Node SelectAndRemoveNode(List<Node> nodeList)
        {
            Node node = nodeList[0];
            nodeList.RemoveAt(0);
            return node;
        }



        struct Color
        {
            public byte R;
            public byte G;
            public byte B;

            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

    }

    public class Node
    {
        public bool isLeaf;
        public int sumR;
        public int sumG;
        public int sumB;
        public int count;
        public Node[] nodes;

        public Node()
        {
            this.nodes = new Node[8];
        }

        public Node(bool isLeaf, int sumR, int sumG, int sumB, int count)
        {
            this.isLeaf = isLeaf;
            this.sumR = sumR;
            this.sumG = sumG;
            this.sumB = sumB;
            this.count = count;
            this.nodes = new Node[8];
        }
    }

    public class OctTree
    {
        public Node? root;
        public int leafCount;
        public int maxLeaves;
        public List<Node>[] innerNodes;

        public OctTree(Node? root, int leafCount, int maxLeaves)
        {
            this.root = root;
            this.leafCount = leafCount;
            this.maxLeaves = maxLeaves;
            innerNodes = new List<Node>[8];
            for (int i = 0; i < innerNodes.Length; i++)
            {
                innerNodes[i] = new List<Node>();
            }
        }
    }
}
