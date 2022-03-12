using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 解を求める
    /// </summary>
    internal class Searcher
    {
        internal Vector2Int size { get; }
        internal ItemData[] data { get; }

        private int _minAmount;
        private int _logScore;
        private int[,] _successMap;
        private List<Log> _logs;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size"></param>
        /// <param name="data"></param>
        /// <param name="logScore"></param>
        internal Searcher(Vector2Int size, ItemData[] data, int logScore = -1)
        {
            this.size = size;
            this.data = data;
            _logScore = logScore;
            
            _minAmount = data.Max(d => d.Amount);
            _logs = new List<Log>();
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        internal void Process(bool isParallel, Action<CoverResult> endCallback)
        {
            _ = ProcessImpl(isParallel, endCallback);
        }

        private async Task ProcessImpl(bool isParallel, Action<CoverResult> callback)
        {
            var begin = DateTime.Now;
            var source = new CancellationTokenSource();
            if (isParallel)
                await Parallel(source);
            else
                await Sequence(source);

            var elapsedTime = (DateTime.Now - begin).Milliseconds;

            // 結果をセット
            var result = new CoverResult(_successMap != null, _successMap, elapsedTime, _logs);

            callback.Invoke(result);
        }

        /// <summary>
        /// 並列検索
        /// </summary>
        private async Task Parallel(CancellationTokenSource source)
        {
            var tasks = new List<Task>();

            for (var i = 0; i < data.Length; i++)
            {
                tasks.Add(SearchTask(i, source));
            }
            try
            {
                await Task.WhenAll(tasks);
            }
            catch {}
        }

        /// <summary>
        /// 逐次検索
        /// </summary>
        private async Task Sequence(CancellationTokenSource source)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var success = await SearchTask(i, source);
                if (success)
                    return;
            }
        }

        /// <summary>
        /// 非同期で一気に投げる
        /// </summary>
        private Task<bool> SearchTask(int startIndex, CancellationTokenSource source)
        {
            var indexes = new List<int>(data.Length);
            for (var i = 0; i < data.Length; i++) 
                indexes.Add((startIndex + i) % data.Length);
            
            var node = new SearcherNode(this, indexes);
            var success = node.ProcessRecursive(source.Token);
            if (success)
            {
                // 見つかったら止める
                source.Cancel();
                _successMap = node.CurrentMap;
            }

            return Task.FromResult(success);
        }

        /// <summary>
        /// ログ追加
        /// </summary>
        internal void AddLog(Box box)
        {
            if (_logScore < 0 || box.EmptyCount() == 0 || box.EmptyCount() > _logScore) 
                return;
            
            _logs.Add(new Log(box.Copy(), box.EmptyCount()));
        }
        
        /// <summary>
        /// 現在の状態で埋めを継続できるか確認する
        /// </summary>
        internal bool Continuable(Box box)
        {
            // 空き領域が最小サイズより少ない場合は置けない
            if (box.EmptyCount() < _minAmount) 
                return false;

            return true;
        }
    }
}