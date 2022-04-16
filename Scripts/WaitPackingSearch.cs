using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        internal IEnumerable<bool[,]> shapes { get; private set; }
        internal Vector2Int size { get; private set; }
        internal IList<Vector2Int> invalidPositions { get; private set; }
        internal int logScore { get; private set; }
        internal bool all { get; private set; }
        internal int allowCount { get; private set; }

        private bool parallel;
        private bool _wait;
        
        public WaitPackingSearch(int width, int height, IEnumerable<int[,]> shapes)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="shapes"></param>
        public WaitPackingSearch(int width, int height, IEnumerable<bool[,]> shapes)
        {
            SetData(width, height, shapes);
        }
        
        private void SetData(int width, int height, IEnumerable<bool[,]> shapes)
        {
            size = new Vector2Int(width, height);
            this.shapes = shapes;
        }
        
        /// <summary>
        /// 探索開始
        /// 見つからなかった場合は指定した値以下の空き結果を記録して返す
        /// </summary>
        public void Evaluate(Algorithm algorithm = Algorithm.BottomLeft)
        {
            _wait = true;
            
            _ = Task.Run(Process);

            async Task Process()
            {
                var source = new CancellationTokenSource();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += change =>
                {
                    if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    {
                        if (source != null)
                            source.Cancel();
                    }
                };
#endif
                var searcher = GetSearcher(algorithm);
                _result = await searcher.Process(parallel, source);
                _wait = false;
            }
        }

        private SearcherAbstract GetSearcher(Algorithm algorithm)
        {
            return algorithm switch
            {
                Algorithm.BottomLeft => new BottomLeftSearcher(this, algorithm),
                Algorithm.LeftBottom => new BottomLeftSearcher(this, algorithm),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
            };
        }
        
        // ----------
        // Options
        // -----------

        /// <summary>
        /// 無効エリアの定義
        /// </summary>
        public WaitPackingSearch SetInValidPosition(IList<Vector2Int> invalidPositions)
        {
            this.invalidPositions = invalidPositions;
            return this;
        }

        /// <summary>
        /// 並列に計算するオプション
        /// </summary>
        /// <returns></returns>
        public WaitPackingSearch SetParallel()
        {
            parallel = true;
            return this;
        }
        
        /// <summary>
        /// ログを有効にする空き数
        /// </summary>
        public WaitPackingSearch SetLogScore(int score)
        {
            logScore = score;
            return this;
        }
        
        /// <summary>
        /// 計算が終わるまで回す
        /// </summary>
        public WaitPackingSearch SetAllFind()
        {
            all = true;
            return this;
        }

        /// <summary>
        /// 空きの許容範囲
        /// </summary>
        /// <returns></returns>
        public WaitPackingSearch SetAllowEmptyCount(int count)
        {
            allowCount = Mathf.Max(0, count);
            return this;
        }
    }
}











