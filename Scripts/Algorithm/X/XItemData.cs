using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Yorozu
{
    internal class XItemData : ItemData
    {
        /// <summary>
        /// 削除した絡むのIndex
        /// </summary>
        private HashSet<int> _deleteColumns = new HashSet<int>();

        /// <summary>
        /// 仮削除
        /// </summary>
        private Stack<HashSet<int>> _stackDeleteColumns = new Stack<HashSet<int>>();
        
        /// <summary>
        /// 全パターンの存在するIndexを確保している
        /// </summary>
        private List<HashSet<int>> _columns;

        /// <summary>
        /// 正解カラムのIndex
        /// </summary>
        private int? _validColumnIndex = null;

        /// <summary>
        /// このデータ内で確定が出てか
        /// </summary>
        internal bool IsFix => _validColumnIndex.HasValue;

        private int _length;
        /// <summary>
        /// 何種類のパターンがあるか
        /// </summary>
        internal int PatternCount => _columns.Count;
        
        /// <summary>
        /// 指定した列の合計値
        /// </summary>
        internal int this[int r]
        {
            get
            {
                var sum = 0;
                for (var i = 0; i < _columns.Count; i++)
                {
                    if (_deleteColumns.Contains(i))
                        continue;
                    
                    if (_columns[i].Contains(r))
                        sum++;
                }

                return sum;
            }
        }
        
        internal XItemData(bool[,] shape, Vector2Int size, int index, HashSet<int> invalidPositions) : base(shape, size, index)
        {
            _columns = new List<HashSet<int>>();
            _length = size.x * size.y;
            
            // 入りうる領域を追加
            for (var y = 0; y < size.y - height; y++)
            {
                for (var x = 0; x < size.x - width; x++)
                {
                    var column = new HashSet<int>(size.x * size.y);

                    // 有効座標で埋める
                    foreach (var p in validPositions)
                    {
                        var columnIndex = x + p.x + (y + p.y) * size.x;
                        column.Add(columnIndex);
                    }
                    
                    // 無効座標が含まれていた場合は無視
                    if (invalidPositions.Count > 0 && column.Any(invalidPositions.Contains))
                        continue;
                    
                    _columns.Add(column);
                }
            }
        }
        
        /// <summary>
        /// 対象列を含む行の列一覧
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(int column, HashSet<int> hash)> HasRows(int r)
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                if (_deleteColumns.Contains(i))
                    continue;
                
                if (!_columns[i].Contains(r))
                    continue;
                
                yield return (i, _columns[i]);
            }
        }

        internal Vector2Int[] FixPositions()
        {
            if (!IsFix)
                return Array.Empty<Vector2Int>();

            var column = _columns[_validColumnIndex.Value];
            var ret = new Vector2Int[column.Count];
            var index = 0;
            foreach (var rowIndex in column)
            {
                var x = rowIndex % size.x;
                var y = Mathf.FloorToInt(rowIndex / (float)size.y);
                ret[index++] = new Vector2Int(x, y);
            }

            return ret;
        }

        /// <summary>
        /// 確定フラグを更新
        /// </summary>
        internal void UpdateSelectColum(int? index)
        {
            _validColumnIndex = index;
        }

        /// <summary>
        /// 対象列を持ってる行に削除フラグを建てる
        /// この際に削除したやつを記録しておく
        /// </summary>
        internal void DeleteColumns(HashSet<int> deleteRows)
        {
            var hash = new HashSet<int>();
            for (var i = 0; i < _columns.Count; i++)
            {
                if (_deleteColumns.Contains(i))
                    continue;

                foreach (var row in deleteRows)
                {
                    if (!_columns[i].Contains(row))
                        continue;

                    _deleteColumns.Add(i);
                    hash.Add(i);
                    break;
                }
            }
            _stackDeleteColumns.Push(hash);
        }

        /// <summary>
        /// 削除していたやつを戻す
        /// </summary>
        internal void UndoDeleteColumns()
        {
            if (_stackDeleteColumns.Count <= 0)
                throw new Exception("invalid Stack");

            var undoHash = _stackDeleteColumns.Pop();
            foreach (var column in undoHash)
            {
                _deleteColumns.Remove(column);
            }
        }

        /// <summary>
        /// 現在の未確定行の中身を返す
        /// </summary>
        internal IEnumerable<string> PrintValidColumns(HashSet<int> deleteRows, int startIndex)
        {
            var builder = new StringBuilder(_length);
            for (var i = 0; i < _columns.Count; i++)
            {
                if (_deleteColumns.Contains(i))
                    continue;

                builder.Clear();
                builder.Append($"{startIndex + i:D5}: {Index:D2}:  ");
                for (var row = 0; row < _length; row++)
                {
                    if (deleteRows.Contains(row))
                        continue;

                    builder.Append(_columns[i].Contains(row) ? "●" : "○");
                    builder.Append("   ");
                }

                yield return builder.ToString();
            }
        }
    }
}