using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yorozu
{
    /// <summary>
    /// 解を求める
    /// </summary>
    internal abstract class SearcherAbstract
    {
        protected WaitPackingSearch _owner;
        protected List<int[,]> _successMaps;
        private List<Log> _logs;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal SearcherAbstract(WaitPackingSearch owner)
        {
            _owner = owner;
            
            _successMaps = new List<int[,]>();
            _logs = new List<Log>();
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        internal async Task<PackingResult> Process(bool parallel, CancellationTokenSource source)
        {
            var begin = DateTime.Now;
            if (parallel)
                await Parallel(source);
            else
                await Sequence(source);

            var elapsedTime = (DateTime.Now - begin).Milliseconds;

            // 結果をセット
            var result = new PackingResult(_successMaps, elapsedTime, _logs);

            return result;
        }

        /// <summary>
        /// 並列検索
        /// </summary>
        private async Task Parallel(CancellationTokenSource source)
        {
            var tasks = new List<Task>();

            for (var i = 0; i < _owner.shapes.Count(); i++)
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
            for (var i = 0; i < _owner.shapes.Count(); i++)
            {
                var success = await SearchTask(i, source);
                if (success && !_owner.all)
                    return;
            }
        }

        protected abstract AlgorithmNode GetNode(int startIndex);

        /// <summary>
        /// 非同期で一気に投げる
        /// </summary>
        private Task<bool> SearchTask(int startIndex, CancellationTokenSource source)
        {
            var node = GetNode(startIndex);
            
            node.Process(source.Token);
            var success = node.SuccessMap.Any();
            // 見つかったら止める
            if (success)
            {
                _successMaps.AddRange(node.SuccessMap);
                
                if (!_owner.all)
                    source.Cancel();
            }
            _logs.AddRange(node.Logs);

            return Task.FromResult(success);
        }
    }
}