using System.Collections.Generic;
using UnityEngine;

namespace Yorozu
{
    internal class BottomLeftItemData : ItemData
    {
        /// <summary>
        /// 配置した場合の全パターン
        /// </summary>
        private List<Vector2Int[]> _maps;
        
        internal IEnumerable<Vector2Int[]> Maps => _maps;
        
        public BottomLeftItemData(bool[,] shape, Vector2Int size, int index, Algorithm algorithm) : base(shape, size, index)
        {
            _maps = new List<Vector2Int[]>();
            // 配置の全パターンをキャッシュ
            // TODO 回転は考慮していない
            if (algorithm == Algorithm.BottomLeft)
            {
                for (var x = 0; x < size.x - width; x++)
                    for (var y = 0; y < size.y - height; y++)
                        AddMap(x, y);
            }
            // LeftBottom
            else
            {
                for (var y = 0; y < size.y - height; y++)
                    for (var x = 0; x < size.x - width; x++)
                        AddMap(x, y);
            }

            void AddMap(int x, int y)
            {
                var map = new Vector2Int[validPositions.Length];
                for (var i = 0; i < validPositions.Length; i++)
                {
                    map[i] = new Vector2Int(x + validPositions[i].x, y + validPositions[i].y);
                }
                _maps.Add(map);
            }
        }
    }
}