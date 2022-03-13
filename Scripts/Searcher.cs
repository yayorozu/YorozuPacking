using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Yorozu
{
    /// <summary>
    /// 解を求める
    /// </summary>
    internal class Searcher
    {
        internal ItemData[] data => _owner.data;

        private WaitPackingSearch _owner;
        
        private List<int[,]> _successMaps;
        private List<Log> _logs;
        
        private int _logScore;
        private bool _all;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Searcher(WaitPackingSearch owner, int logScore, bool all)
        {
            _owner = owner;
            _logScore = logScore;
            _all = all;
            
            _successMaps = new List<int[,]>();
            _logs = new List<Log>();
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        internal void Process(bool parallel, Action<PackingResult> endCallback)
        {
            _ = ProcessImpl(parallel, endCallback);
        }

        private async Task ProcessImpl(bool isParallel, Action<PackingResult> callback)
        {
            var begin = DateTime.Now;
            var source = new CancellationTokenSource();
            if (isParallel)
                await Parallel(source);
            else
                await Sequence(source);

            var elapsedTime = (DateTime.Now - begin).Milliseconds;

            // 結果をセット
            var result = new PackingResult(_successMaps, elapsedTime, _logs);

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
                if (success && !_all)
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
            
            var node = new SearcherNode(this, new Box(_owner), indexes);
            var success = node.ProcessRecursive(source.Token);
            if (success)
            {
                // 見つかったら止める
                if (!_all)
                {
                    source.Cancel();
                }

                _successMaps.Add(node.CurrentMap.Copy());
            }

            return Task.FromResult(success);
        }

        /// <summary>
        /// ログ追加
        /// </summary>
        internal void AddLog(Box box)
        {
            if (_logScore < 0 || box.EmptyCount() > _logScore) 
                return;
            
            _logs.Add(new Log(box.Copy(), box.EmptyCount()));
        }
        
        /// <summary>
        /// 現在の状態で埋めを継続できるか確認する
        /// </summary>
        internal bool Continuable(Box box)
        {
            // 空き領域が最小サイズより少ない場合は置けない
            if (box.EmptyCount() < _owner.minAmount) 
                return false;

            return true;
        }
    }
}