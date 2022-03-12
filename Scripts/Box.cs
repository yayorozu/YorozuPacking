using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 敷き詰める瓶
    /// </summary>
    internal class Box : IPrint
    {
        private int[,] _map;
        private int _empty;

        internal int[,] Map => _map;
        int[,] IPrint.Map => _map;
        
        internal Box(Vector2Int size)
        {
            _map = new int[size.x, size.y];
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    _map[x, y] = WaitPackingSearch.EMPTY;
                }
            }

            _empty = size.x * size.y;
        }

        internal bool Valid(Vector2Int point)
        {
            return _map[point.x, point.y] == WaitPackingSearch.EMPTY;
        }

        /// <summary>
        /// 置き状態に
        /// </summary>
        internal void Put(Vector2Int[] points, int index)
        {
            foreach (var point in points)
            {
                _map[point.x, point.y] = index;
            }
            _empty -= points.Length;
        }

        /// <summary>
        /// もとに戻す
        /// </summary>
        internal void Reset(Vector2Int[] points)
        {
            foreach (var point in points)
            {
                _map[point.x, point.y] = WaitPackingSearch.EMPTY;
            }

            _empty += points.Length;
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