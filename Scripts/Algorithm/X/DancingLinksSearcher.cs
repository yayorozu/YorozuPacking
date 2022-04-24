using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Yorozu
{
    internal class DancingLinksSearcher : SearcherAbstract
    {
        internal DancingLinksSearcher(WaitPackingSearch owner) : base(owner)
        {
        }
        
        protected override bool ValidAllIndexSearch => false;

        protected override AlgorithmNode GetNode(int startIndex)
        {
            return new DancingLinksAlgorithm(_owner);
        }
    }

    internal class DancingLinksAlgorithm : AlgorithmNode
    {
        private CancellationToken _token;
        private List<Node> _nodes;
        private HeadNode _head;
        private Node[] _solutions;
        private int _length;
        private int _shapesCount;

        internal DancingLinksAlgorithm(WaitPackingSearch owner) : base(owner, 0)
        {
            _length = owner.size.x * owner.size.y;
            _shapesCount = owner.shapes.Count();
            CreateNodes(owner);
            _solutions = new Node[owner.shapes.Count()];
        }

        /// <summary>
        /// 列のノードを作成
        /// </summary>
        private void CreateNodes(WaitPackingSearch owner)
        {
            var size = owner.size;
            
            var invalidIndexHash = new HashSet<int>(owner.invalidPositions.Count);
            foreach (var p in owner.invalidPositions) 
                invalidIndexHash.Add(p.x + p.y * owner.size.x);

            _head = new HeadNode();
            var columnNodes = new Dictionary<int, ColumnNode>(size.x * size.y);
            for (var i = 0; i < size.x * size.y; i++)
            {
                if (invalidIndexHash.Contains(i))
                    continue;
                
                var columnNode = new ColumnNode(i);
                _head.AppendRight(columnNode);
                columnNodes.Add(i, columnNode);
            }
            // GroupNodeを追加
            for (var i = 0; i < owner.shapes.Count(); i++)
            {
                var columnNode = new ColumnNode(_length + i);
                _head.AppendRight(columnNode);
                columnNodes.Add(_length + i, columnNode);
            }
        
            var columnCount = 0;
            // 各ノードを生成
            for (var i = 0; i < owner.shapes.Count(); i++)
            {
                var shape = owner.shapes.ElementAt(i);
                var values = ItemData.CalcValidPosition(shape);
                var nodes = new List<Node>();
                
                // 入りうる領域を追加
                for (var y = 0; y < size.y - values.height; y++)
                {
                    for (var x = 0; x < size.x - values.width; x++)
                    {
                        var isSkip = false;
                        foreach (var p in values.validPositions)
                        {
                            var index = x + p.x + (y + p.y) * size.x;
                            if (!columnNodes.ContainsKey(index))
                            {
                                isSkip = true;
                                break;
                            }
                        }
                        // 無効座標が含まれていた場合は無視
                        if (isSkip)
                            continue;

                        // 左側だけ確保
                        var leftCell = CreateCell(values.validPositions[0]);
                        nodes.Add(leftCell);
                        for (var j = 1; j < values.validPositions.Length; j++)
                        {
                            var cell = CreateCell(values.validPositions[j]);
                            leftCell.AppendRight(cell);
                        }
                        
                        // 同じグループ識別用のノードを追加
                        var group = new CellNode(-1, -1, _length + i, columnCount, i, columnNodes[_length + i]);
                        leftCell.AppendRight(group);

                        CellNode CreateCell(Vector2Int p)
                        {
                            var rowIndex = x + p.x + (y + p.y) * size.x;
                            var cellNode = new CellNode(x + p.x, y + p.y, rowIndex, columnCount, i, columnNodes[rowIndex]);
                            return cellNode;
                        }

                        columnCount++;
                    }
                }
            }
        }
        
        internal override void Process(CancellationToken token)
        {
            _token = token;
            if (validLog)
                Print();
            ProcessRecursive(0);
        }

        private bool ProcessRecursive(int depth)
        {
            if (_token.IsCancellationRequested)
                return false;
            
            if (_head.horizontalCount <= allowCount)
            {
                ApplySuccessMap();
                if (validLog)
                    Logger("Find");
                return true;
            }

            var columnNodes = FindMinRows();
            if (!columnNodes.Any())
            {
                // 埋まったけど、特定のやつを使わなかった場合
                var sum = 0;
                for (var n = _head.right; n != _head; n = n.right)
                {
                    if (n.row < _length)
                        return false;
                    
                    sum += (n as ColumnNode).size;
                }

                if (sum <= allowCount)
                {
                    ApplySuccessMap();
                    if (validLog)
                        Logger("Find");
                    return true;
                }
                
                return false;
            }
            
            foreach (var columnNode in columnNodes)
            {
                for (var n = columnNode.down; n != columnNode; n = n.down)
                {
                    _solutions[depth] = n;
                    ControlNode(n, true);

                    if (ProcessRecursive(depth + 1))
                        return true;

                    ControlNode(n, false);
                }
            }

            return false;
        }

        /// <summary>
        /// 成功データを作成
        /// </summary>
        private void ApplySuccessMap()
        {
            _box.Clear();
            
            for (var i = 0; i < _solutions.Length; i++)
            {
                if (_solutions[i] == null)
                {
                    continue;
                }

                var node = _solutions[i];
                Put(node as CellNode);
                for (var n = node.right; n != node; n = n.right)
                {
                    Put(n as CellNode);
                }

                void Put(CellNode node)
                {
                    // 種類判定用は無視
                    if (node.x < 0 || node.y < 0) 
                        return;
                    
                    _box.Put(node.x, node.y, node.index);
                }
            }

            AddSuccessLog();
        }
        
        /// <summary>
        /// 最小の列を探す
        /// </summary>
        private IEnumerable<ColumnNode> FindMinRows()
        {
            var minSize = int.MaxValue;
            for (var n = _head.right; n != _head; n = n.right)
            {
                var columnNode = n as ColumnNode;
                if (columnNode.size == 0)
                    continue;
                
                if (columnNode.size < minSize)
                    minSize = columnNode.size;
            }

            for (var n = _head.right; n != _head; n = n.right)
            {
                var columnNode = n as ColumnNode;
                if (columnNode.size == minSize)
                    yield return columnNode;
            }
        }

        /// <summary>
        /// ノードを削除する
        /// </summary>
        private void ControlNode(Node controlNode, bool delete)
        {
            var node = controlNode;
            do
            {
                // ヘッダーを削除
                var header = (node as CellNode).columnNode;
                header.ControlRow(delete);
                _head.UpdateHorizontalCount(delete);

                // 同じ列をたどる
                for (var c = header.down; c != header; c = c.down)
                {
                    // ヘッダーはスキップ
                    if (c is ColumnNode)
                        continue;
                        
                    // 行をたどる
                    for (var r = c.right; r != c; r = r.right)
                    {
                        r.ControlColumn(delete);
                    }
                }
                
                node = node.right;
            } while (node != controlNode);
            
            if (validLog)
                Print($"{(delete ? "Delete" : "Undo  ")} Row: {controlNode.row} Column: {controlNode.column}");
        }

        private void Print(string additional = "")
        {
            var builder = new StringBuilder();
            
            builder.AppendLine(additional);

            var validRows = new HashSet<int>();
            var validColumns = new HashSet<int>();
            builder.Append("\t\t");
            var maxColumnIndex = 0;
            //Header
            for (var column = _head.right; column != _head; column = column.right)
            {
                builder.Append($"{column.row}|{(column as ColumnNode).size}\t");
                if (maxColumnIndex < column.up.column)
                    maxColumnIndex = column.up.column;
                validRows.Add(column.row);
            }
            builder.AppendLine("");
            
            var indexes = new int[maxColumnIndex + 1];
            var table = new string[_length + _shapesCount, maxColumnIndex + 1];
            for (var x = 0; x < table.GetLength(0); x++)
                for (var y = 0; y < table.GetLength(1); y++)
                    table[x, y] = "_";
            
            for (var column = _head.right; column != _head; column = column.right)
            {
                for (var cell = column.down; cell != column; cell = cell.down)
                {
                    if (!validColumns.Contains(cell.column))
                        validColumns.Add(cell.column);
                    
                    
                    indexes[cell.column] = cell.index;
                    table[cell.row, cell.column] = cell.row >= _length ? "▲" : "●";
                }
            }

            for (var y = 0; y < table.GetLength(1); y++)
            {
                if (!validColumns.Contains(y))
                    continue;
                
                builder.Append($"{y:D5}:\t{indexes[y]}:\t");
                for (var x = 0; x < table.GetLength(0); x++)
                {
                    if (!validRows.Contains(x)) 
                        continue;
                    
                    builder.Append($"{table[x, y]}\t");
                }
                builder.AppendLine("");
            }

            Logger(builder.ToString());
        }

        internal class HeadNode : Node
        {
            internal HeadNode() : base(-1, -1)
            {
            }

            /// <summary>
            /// 列の数
            /// </summary>
            internal int horizontalCount { get; private set; }

            internal override void AppendRight(Node node)
            {
                horizontalCount++;
                base.AppendRight(node);
            }

            internal void UpdateHorizontalCount(bool delete)
            {
                horizontalCount += delete ? -1 : 1;
            }
        }

        /// <summary>
        /// 列の上のノード
        /// </summary>
        internal class ColumnNode : Node
        {
            internal int size
            {
                get
                {
                    var size = 0;
                    for (var node = down; node != this; node = node.down)
                    {
                        size++;
                    }

                    return size;
                }
            }
            
            internal ColumnNode(int x) : base(x, -1)
            {
                row = x;
            }
            
            /// <summary>
            /// 下にノードを追加
            /// </summary>
            internal void AppendDown(Node node)
            {
                up.down = node;
                node.down = this;
                node.up = up;
                up = node;
            }
        }

        internal class CellNode : Node
        {
            internal ColumnNode columnNode;
            
            internal int x;
            internal int y;
            
            internal CellNode(int x, int y, int row, int column, int index, ColumnNode columnNode) : base(x, y)
            {
                this.x = x;
                this.y = y;
                this.index = index;
                this.column = column;
                this.row = row;
                this.columnNode = columnNode;
                columnNode.AppendDown(this);
            }
        }
        
        internal class Node
        {
            /// <summary>
            /// 何行目か
            /// </summary>
            internal int column = -1;
            internal int row = -1;
            internal int index = -1;
            
            internal Node left;
            internal Node right;
            internal Node up;
            internal Node down;

            internal Node(int row, int column)
            {
                left = right = up = down = this;
                this.row = row;
                this.column = column;
            }

            /// <summary>
            /// 右側にノードを追加
            /// </summary>
            internal virtual void AppendRight(Node node)
            {
                // 左端を自分に紐付ける
                left.right = node;
                node.right = this;
                node.left = left;
                left = node;
            }

            /// <summary>
            /// 列から削除 or 戻す
            /// </summary>
            internal void ControlRow(bool delete)
            {
                left.right = delete ? right : this;
                right.left = delete ? left : this;
            }
            
            /// <summary>
            /// 行から削除 or 戻す
            /// </summary>
            internal void ControlColumn(bool delete)
            {
                up.down = delete ? down : this;
                down.up = delete ? up : this;
            }
        }
    }
}