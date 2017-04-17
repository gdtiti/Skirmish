﻿using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Collections
{
    using Engine.Common;

    /// <summary>
    /// Quadtree node
    /// </summary>
    public class QuadTreeNode<T> where T : IVertexList
    {
        /// <summary>
        /// Static node count
        /// </summary>
        private static int NodeCount = 0;

        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode<T> CreatePartitions(
            QuadTree<T> quadTree, QuadTreeNode<T> parent,
            BoundingBox bbox, T[] items,
            int maxDepth,
            int treeDepth)
        {
            if (parent == null) NodeCount = 0;

            if (treeDepth <= maxDepth)
            {
                var nodeItems = Array.FindAll(items, i =>
                {
                    var tbox = BoundingBox.FromPoints(i.GetVertices());

                    return Intersection.BoxContainsBox(ref bbox, ref tbox) != ContainmentType.Disjoint;
                });

                if (nodeItems.Length > 0)
                {
                    var node = new QuadTreeNode<T>(quadTree, parent)
                    {
                        Id = -1,
                        Level = treeDepth,
                        BoundingBox = bbox,
                    };

                    bool haltByDepth = treeDepth == maxDepth;
                    if (haltByDepth)
                    {
                        node.Id = NodeCount++;
                        node.Items = nodeItems;
                    }
                    else
                    {
                        Vector3 M = bbox.Maximum;
                        Vector3 c = (bbox.Maximum + bbox.Minimum) * 0.5f;
                        Vector3 m = bbox.Minimum;

                        //-1-1-1   +0+1+0   -->   mmm    cMc
                        BoundingBox topLeftBox = new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
                        //-1-1+0   +0+1+1   -->   mmc    cMM
                        BoundingBox topRightBox = new BoundingBox(new Vector3(m.X, m.Y, c.Z), new Vector3(c.X, M.Y, M.Z));
                        //+0-1-1   +1+1+0   -->   cmm    MMc
                        BoundingBox bottomLeftBox = new BoundingBox(new Vector3(c.X, m.Y, m.Z), new Vector3(M.X, M.Y, c.Z));
                        //+0-1+0   +1+1+1   -->   cmc    MMM
                        BoundingBox bottomRightBox = new BoundingBox(new Vector3(c.X, m.Y, c.Z), new Vector3(M.X, M.Y, M.Z));

                        var topLeftChild = CreatePartitions(quadTree, node, topLeftBox, nodeItems, maxDepth, treeDepth + 1);
                        var topRightChild = CreatePartitions(quadTree, node, topRightBox, nodeItems, maxDepth, treeDepth + 1);
                        var bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, nodeItems, maxDepth, treeDepth + 1);
                        var bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, nodeItems, maxDepth, treeDepth + 1);

                        List<QuadTreeNode<T>> childList = new List<QuadTreeNode<T>>();

                        if (topLeftChild != null) childList.Add(topLeftChild);
                        if (topRightChild != null) childList.Add(topRightChild);
                        if (bottomLeftChild != null) childList.Add(bottomLeftChild);
                        if (bottomRightChild != null) childList.Add(bottomRightChild);

                        if (childList.Count > 0)
                        {
                            node.Children = childList.ToArray();
                            node.TopLeftChild = topLeftChild;
                            node.TopRightChild = topRightChild;
                            node.BottomLeftChild = bottomLeftChild;
                            node.BottomRightChild = bottomRightChild;
                        }
                    }

                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Parent
        /// </summary>
        public QuadTree<T> QuadTree { get; private set; }
        /// <summary>
        /// Parent node
        /// </summary>
        public QuadTreeNode<T> Parent { get; private set; }
        /// <summary>
        /// Gets the child node al top lef position (from above)
        /// </summary>
        public QuadTreeNode<T> TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al top right position (from above)
        /// </summary>
        public QuadTreeNode<T> TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom lef position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom right position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomRightChild { get; private set; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        public QuadTreeNode<T> TopNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        public QuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        public QuadTreeNode<T> RightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        public QuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        public QuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomRightNeighbor { get; private set; }

        /// <summary>
        /// Node Id
        /// </summary>
        public int Id;
        /// <summary>
        /// Depth level
        /// </summary>
        public int Level;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// Gets the node center position
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return (this.BoundingBox.Maximum + this.BoundingBox.Minimum) * 0.5f;
            }
        }
        /// <summary>
        /// Children list
        /// </summary>
        public QuadTreeNode<T>[] Children;
        /// <summary>
        /// Node triangles
        /// </summary>
        internal T[] Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public QuadTreeNode(QuadTree<T> quadTree, QuadTreeNode<T> parent)
        {
            this.QuadTree = quadTree;
            this.Parent = parent;
        }
        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        public void ConnectNodes()
        {
            this.TopNeighbor = this.FindNeighborNodeAtTop();
            this.BottomNeighbor = this.FindNeighborNodeAtBottom();
            this.LeftNeighbor = this.FindNeighborNodeAtLeft();
            this.RightNeighbor = this.FindNeighborNodeAtRight();

            this.TopLeftNeighbor = this.TopNeighbor != null ? this.TopNeighbor.FindNeighborNodeAtLeft() : null;
            this.TopRightNeighbor = this.TopNeighbor != null ? this.TopNeighbor.FindNeighborNodeAtRight() : null;
            this.BottomLeftNeighbor = this.BottomNeighbor != null ? this.BottomNeighbor.FindNeighborNodeAtLeft() : null;
            this.BottomRightNeighbor = this.BottomNeighbor != null ? this.BottomNeighbor.FindNeighborNodeAtRight() : null;

            if (this.Children != null && this.Children.Length > 0)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].ConnectNodes();
                }
            }
        }
        /// <summary>
        /// Searchs for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtTop()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomRightChild;
                    }
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    return this.Parent.TopLeftChild;
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    return this.Parent.TopRightChild;
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtBottom()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.BottomLeftChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    return this.Parent.BottomRightChild;
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopRightChild;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at right position(from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtRight()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.TopRightChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtRight();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    return this.Parent.BottomRightChild;
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtRight();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at left position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at left position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtLeft()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtLeft();
                    if (node != null)
                    {
                        return node.TopRightChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    return this.Parent.TopLeftChild;
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtLeft();
                    if (node != null)
                    {
                        return node.BottomRightChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    return this.Parent.BottomLeftChild;
                }
            }

            return null;
        }

        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public BoundingBox[] GetBoundingBoxes(int maxDepth = 0)
        {
            List<BoundingBox> bboxes = new List<BoundingBox>();

            if (this.Children != null)
            {
                bool haltByDepth = maxDepth > 0 ? this.Level == maxDepth : false;
                if (haltByDepth)
                {
                    Array.ForEach(this.Children, (c) =>
                    {
                        bboxes.Add(c.BoundingBox);
                    });
                }
                else
                {
                    Array.ForEach(this.Children, (c) =>
                    {
                        bboxes.AddRange(c.GetBoundingBoxes(maxDepth));
                    });
                }
            }
            else
            {
                bboxes.Add(this.BoundingBox);
            }

            return bboxes.ToArray();
        }
        /// <summary>
        /// Gets maximum level value
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevel()
        {
            int level = 0;

            if (this.Children != null)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    int cLevel = this.Children[i].GetMaxLevel();

                    if (cLevel > level) level = cLevel;
                }
            }
            else
            {
                level = this.Level;
            }

            return level;
        }

        /// <summary>
        /// Gets the tail nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the tail nodes contained into the frustum</returns>
        public QuadTreeNode<T>[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

            if (this.Children == null)
            {
                if (frustum.Contains(this.BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetNodesInVolume(ref frustum);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodes</returns>
        public QuadTreeNode<T>[] GetLeafNodes()
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

            if (this.Children == null)
            {
                nodes.Add(this);
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetLeafNodes();
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets node at position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the tail node wich contains the specified position</returns>
        public QuadTreeNode<T> GetNode(Vector3 position)
        {
            if (this.Children == null)
            {
                if (this.BoundingBox.Contains(position) != ContainmentType.Disjoint)
                {
                    return this;
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNode = this.Children[i].GetNode(position);
                    if (childNode != null)
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Children == null)
            {
                //Tail node
                return string.Format("QuadTreeNode {0}; Depth {1}; Items {2}", this.Id, this.Level, this.Items.Length);
            }
            else
            {
                //Node
                return string.Format("QuadTreeNode {0}; Depth {1}; Childs {2}", this.Id, this.Level, this.Children.Length);
            }
        }
    }
}
