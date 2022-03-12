// 全通り検索が有効か
//#define PERMUTATION

using System.Collections.Generic;
using System.Linq;
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
        
        internal IEnumerable<Log> Logs => _logs;
        internal int[,] SuccessMap { get; private set; }
        
        private int _minAmount;
        private int _logScore;
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
        internal bool Process()
        {
#if PERMUTATION
            var indexes = new int[data.Length];
            for (var i = 0; i < indexes.Length; i++) 
                indexes[i] = i;

            // 全通りやる必要があるため、全部の順番を取得
            foreach (var pattern in Permutation(indexes))
            {
                var list = new HashSet<int>(pattern.Length);
                foreach (var index in pattern) 
                    list.Add(index);
#else
            for (var i = 0; i < data.Length; i++)
            {
                var list = new HashSet<int>(data.Length);
                for (var j = 0; j < data.Length; j++)
                {
                    list.Add((i + j) % data.Length);
                }
#endif
                var node = new SearcherNode(this, list);
                var success = node.ProcessRecursive();
                if (!success) 
                    continue;
                
                SuccessMap = node.CurrentMap;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// ログ追加
        /// </summary>
        internal void AddLog(Box box)
        {
            if (_logScore < 0 || box.EmptyCount() > _logScore) 
                return;
            
            if (box.EmptyCount() == 0)
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
        
#if PERMUTATION
        /// <summary>
        /// 全組み合わせを取得
        /// </summary>
        private IEnumerable<int[]> Permutation(IEnumerable<int> items) 
        {
            if (items.Count() == 1) 
            {
                yield return new int[] { items.First() };
                yield break;
            }
            
            foreach (var item in items) 
            {
                var leftSide = new int[] { item };
                var unused = items.Except(leftSide);
                foreach (var rightSide in Permutation(unused)) 
                {
                    yield return leftSide.Concat(rightSide).ToArray();
                }
            }
        }
#endif
        
    }
}