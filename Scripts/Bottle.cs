using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 敷き詰める瓶
    /// </summary>
    internal class Bottle
    {
        private int[,] _map;
        
        internal Bottle(Vector2Int size)
        {
            _map = new int[size.x, size.y];
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    _map[x, y] = Packing.EMPTY;
                }
            }
        }

        internal bool Valid(Vector2Int point)
        {
            return _map[point.x, point.y] == Packing.EMPTY;
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
        }

        /// <summary>
        /// もとに戻す
        /// </summary>
        internal void Reset(Vector2Int[] points)
        {
            foreach (var point in points)
            {
                _map[point.x, point.y] = Packing.EMPTY;
            }
        }
        
        /// <summary>
        /// 空の数
        /// </summary>
        internal int EmptyCount()
        {
            var count = 0;
            for (var y = 0; y < _map.GetLength(1); y++)
            {
                for (var x = 0; x < _map.GetLength(0); x++)
                {
                    if (_map[x, y] == Packing.EMPTY)
                        count++;
                }
            }
            return count;
        }


        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            for (var y = 0; y < _map.GetLength(1); y++)
            {
                for (var x = 0; x < _map.GetLength(0); x++)
                {
                    builder.Append($"\t{_map[x, y]}, ");
                }

                builder.AppendLine("");
            }

            return builder.ToString();
        }
    }

}