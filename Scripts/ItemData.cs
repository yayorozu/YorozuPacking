using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Yorozu
{
    internal class ItemData
    {
        /// <summary>
        /// 有効な座標
        /// </summary>
        private List<Vector2Int> _validPosition;
        /// <summary>
        /// サイズ
        /// </summary>
        private int width => _validPosition.Max(v => v.x);
        private int height => _validPosition.Max(v => v.y);

        /// <summary>
        /// 図形の回転許可されるか
        /// </summary>
        private bool _validChange;

        /// <summary>
        /// 配置した場合の全パターン
        /// </summary>
        private List<Vector2Int[]> _maps;
        
        internal IEnumerable<Vector2Int[]> Maps => _maps;
        /// <summary>
        /// 個数
        /// </summary>
        internal int Amount => _validPosition.Count;

        internal ItemData(bool[,] shape, Vector2Int size)
        {
            CacheValidPosition(shape);
            SetMap(size);
        }

        /// <summary>
        /// 有効なローカル座標をキャッシュ
        /// </summary>
        /// <param name="shape"></param>
        private void CacheValidPosition(bool[,] shape)
        {
            _validPosition = new List<Vector2Int>(shape.Length);
            for (var x = 0; x < shape.GetLength(0); x++)
            {
                for (var y = 0; y < shape.GetLength(1); y++)
                {
                    if (!shape[x, y])
                        continue;

                    _validPosition.Add(new Vector2Int(x, y));
                }
            }

            // 上下が空白であることを考慮する
            var minX = _validPosition.Min(v => v.x);
            var minY = _validPosition.Min(v => v.y);
            for (var i = 0; i < _validPosition.Count; i++)
            {
                _validPosition[i] = new Vector2Int(
                    _validPosition[i].x - minX, 
                    _validPosition[i].y - minY
                );
            }
        }

        /// <summary>
        /// 配置の全パターンをキャッシュ
        /// TODO 回転を考慮していない
        /// </summary>
        private void SetMap(Vector2Int size)
        {
            _maps = new List<Vector2Int[]>();
            for (var x = 0; x < size.x - width; x++)
            {
                for (var y = 0; y < size.y - height; y++)
                {
                    var map = new Vector2Int[_validPosition.Count];
                    foreach (var item in _validPosition.Select((position, index) => new {position, index}))
                    {
                        var position = new Vector2Int(x + item.position.x, y + item.position.y);
                        map[item.index] = position;
                    }
                    _maps.Add(map);
                }
            }
        }
    }
}