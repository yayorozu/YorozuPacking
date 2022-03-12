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
    public class WaitCoverSearch : CustomYieldInstruction
    {
        public override bool keepWaiting => _wait;

        public CoverResult Result => _result;
        private CoverResult _result; 
        
        public const int EMPTY = -1;

        private ItemData[] _data;
        private Vector2Int _size;
        private bool _wait;
        
        public WaitCoverSearch(int width, int height, IEnumerable<int[,]> shapes)
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

        public WaitCoverSearch(int width, int height, IEnumerable<bool[,]> shapes)
        {
            SetData(width, height, shapes);
        }

        private void SetData(int width, int height, IEnumerable<bool[,]> shapes)
        {
            _size = new Vector2Int(width, height);
            _data = shapes
                .Select((v, i) => new ItemData(v, _size, i))
                // スコア順にソートしたほうが遅いt
                //.OrderByDescending(d => d.Score)
                .ToArray();
            ;
        }

        /// <summary>
        /// 探索開始
        /// 見つからなかった場合は指定した値以下の空き結果を記録して返す
        /// </summary>
        /// <param name="parallel"></param>
        /// <param name="logScore"></param>
        /// <returns></returns>
        public void Evaluate(bool parallel = false, int logScore = -1)
        {
            _wait = true;
            var search = new Searcher(_size, _data, logScore);
            
            search.Process(parallel, SearchFinish);
            
            void SearchFinish(CoverResult result)
            {
                _result = result;
                _wait = false;   
            }
        }
    }
}











