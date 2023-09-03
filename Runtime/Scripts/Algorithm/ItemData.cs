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
        internal Vector2Int[] validPositions;
        /// <summary>
        /// サイズ
        /// </summary>
        internal int width { get; private set; }
        internal int height { get; private set; }
        /// <summary>
        /// 個数
        /// </summary>
        internal int Amount => validPositions.Length;
        
        /// <summary>
        /// スコア順にソートするので、もともとの並び順を記憶
        /// </summary>
        internal int Index { get; }

        protected Vector2Int size; 

        internal ItemData(bool[,] shape, Vector2Int size, int index)
        {
            Index = index;
            this.size = size;
            var values = CalcValidPosition(shape);
            width = values.width;
            height = values.height;
            validPositions = values.validPositions;
        }

        /// <summary>
        /// 有効なローカル座標をキャッシュ
        /// </summary>
        /// <param name="shape"></param>
        internal static (int width, int height, Vector2Int[] validPositions) CalcValidPosition(bool[,] shape)
        {
            var validPositions = new List<Vector2Int>(shape.Length);
            for (var x = 0; x < shape.GetLength(0); x++)
            {
                for (var y = 0; y < shape.GetLength(1); y++)
                {
                    if (!shape[x, y])
                        continue;

                    validPositions.Add(new Vector2Int(x, y));
                }
            }

            // 上下が空白であることを考慮する
            var minX = validPositions.Min(v => v.x);
            var minY = validPositions.Min(v => v.y);
            for (var i = 0; i < validPositions.Count; i++)
            {
                validPositions[i] = new Vector2Int(
                    validPositions[i].x - minX, 
                    validPositions[i].y - minY
                );
            }

            var width = validPositions.Max(v => v.x);
            var height = validPositions.Max(v => v.y);
            return (width, height, validPositions.ToArray());
        }
    }
}