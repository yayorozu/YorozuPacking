using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// Bottom-Left Algorithm を利用して 箱詰めを行う
    /// </summary>
    public class WaitPackingSearch : CustomYieldInstruction
    {
        public override bool keepWaiting => _wait;

        public PackingResult Result => _result;
        private PackingResult _result; 
        
        internal ItemData[] data { get; private set; }
        internal Vector2Int size { get; private set; }
        internal IList<Vector2Int> invalidPositions { get; private set; }
        internal int minAmount { get; private set; }
        
        private bool _wait;
        
        public WaitPackingSearch(int width, int height, IEnumerable<int[,]> shapes, IList<Vector2Int> invalidPositions = null)
        {
            SetData(width, height, shapes.Select(Convert), invalidPositions);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="shapes"></param>
        /// <param name="invalidPositions">使えない領域リスト</param>
        public WaitPackingSearch(int width, int height, IEnumerable<bool[,]> shapes, IList<Vector2Int> invalidPositions = null)
        {
            SetData(width, height, shapes, invalidPositions);
        }
        
        private void SetData(int width, int height, IEnumerable<bool[,]> shapes, IList<Vector2Int> invalidPositions)
        {
            size = new Vector2Int(width, height);
            data = shapes
                .Select((v, i) => new ItemData(v, size, i))
                // スコア順にソートしたほうが遅いt
                //.OrderByDescending(d => d.Score)
                .ToArray()
            ;

            minAmount = data.Max(d => d.Amount);
            this.invalidPositions = invalidPositions;
        }

        /// <summary>
        /// 探索開始
        /// 見つからなかった場合は指定した値以下の空き結果を記録して返す
        /// </summary>
        /// <param name="parallel"></param>
        /// <param name="logScore"></param>
        /// <param name="all">正解が見つかっても全部パターン探索する</param>
        /// <returns></returns>
        public void Evaluate(bool parallel = false, int logScore = -1, bool all = false)
        {
            _wait = true;
            var search = new Searcher(this, logScore, all);
            
            search.Process(parallel, SearchFinish);
            
            void SearchFinish(PackingResult result)
            {
                _result = result;
                _wait = false;   
            }
        }
    }
}











