using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// Exact Cover Problem
    /// 瓶詰め詰めアルゴリズムを利用して瓶詰めを行う
    /// Bottom-Left Algorithm
    /// </summary>
    public class Cover
    {
        public const int EMPTY = -1;

        private ItemData[] _data;
        private Vector2Int _size;

        public Cover(int width, int height, IEnumerable<int[,]> shapes)
        {
            SetData(width, height, shapes.Select(Convert));
            bool[,] Convert(int[,] shape)
            {
                var boolShape = new bool[shape.GetLength(0), shape.GetLength(1)];
                for (var x = 0; x < shape.GetLength(0); x++)
                {
                    for (var y = 0; y < shape.GetLength(1); y++)
                    {
                        boolShape[x, y] = shape[x, y] > 0;
                    }
                }

                return boolShape;
            }
        }

        public Cover(int width, int height, IEnumerable<bool[,]> shapes)
        {
            SetData(width, height, shapes);
        }

        private void SetData(int width, int height, IEnumerable<bool[,]> shapes)
        {
            var count = shapes.Count();
            
            _data = new ItemData[count];
            _size = new Vector2Int(width, height);
            for (var i = 0; i < count; i++)
            {
                _data[i] = new ItemData(shapes.ElementAt(i), _size);
            }   
        }
        
        /// <summary>
        /// 探索開始
        /// </summary>
        public CoverResult Evaluate()
        {
            var search = new Searcher(_size, _data);
            var success = search.Process();
            return new CoverResult(success, success ? search.SuccessMap : null);
        }
        
        /// <summary>
        /// 探索開始
        /// 見つからなかった場合は指定した値以下の空き結果を記録して返す
        /// </summary>
        /// <param name="logScore"></param>
        /// <returns></returns>
        public CoverResult Evaluate(int logScore)
        {
            var search = new Searcher(_size, _data, logScore);
            var success = search.Process();
            
            return new CoverResult(success, success ? search.SuccessMap : null, search.Logs);
        }
    }
}











