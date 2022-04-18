using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 敷き詰める瓶
    /// </summary>
    internal class Box : IPrint
    {
        int[,] IPrint.Map => _map;
        
        private int[,] _map;
        private int _empty;

        internal int[,] Map => _map;
        internal Vector2Int Size => new Vector2Int(_map.GetLength(0), _map.GetLength(1));
        private IList<Vector2Int> _invalids;
        
        internal Box(WaitPackingSearch owner)
        {
            var size = owner.size;
            _invalids = owner.invalidPositions;
            _map = new int[size.x, size.y];
            Clear();
            _empty = size.x * size.y;
        }

        internal bool Valid(Vector2Int point)
        {
            return _map[point.x, point.y] == PackingUtility.EMPTY;
        }

        /// <summary>
        /// 置き状態に
        /// </summary>
        internal void Put(IList<Vector2Int> points, int index)
        {
            foreach (var point in points)
            {
                _map[point.x, point.y] = index;
            }
            _empty -= points.Count;
        }

        /// <summary>
        /// もとに戻す
        /// </summary>
        internal void Reset(Vector2Int[] points)
        {
            foreach (var point in points)
            {
                _map[point.x, point.y] = PackingUtility.EMPTY;
            }

            _empty += points.Length;
        }

        /// <summary>
        /// 全データ初期化
        /// </summary>
        internal void Clear()
        {
            for (var x = 0; x < _map.GetLength(0); x++)
            {
                for (var y = 0; y < _map.GetLength(1); y++)
                {
                    _map[x, y] = _invalids != null && _invalids.Contains(new Vector2Int(x, y))
                        ? PackingUtility.INVALID
                        : PackingUtility.EMPTY;
                }
            }
        }

        /// <summary>
        /// 空の数
        /// </summary>
        internal int EmptyCount() => _empty;

        public override string ToString()
        {
            return this.Print();
        }

        /// <summary>
        /// 新しいインスタンスのデータを取得
        /// </summary>
        internal int[,] Copy()
        {
            var copy = new int[_map.GetLength(0), _map.GetLength(1)]; 
            for (var y = 0; y < _map.GetLength(1); y++)
            {
                for (var x = 0; x < _map.GetLength(0); x++)
                {
                    copy[x, y] = _map[x, y];
                }
            }

            return copy;
        }
    }

}