using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// Bottom Left 法
    /// </summary>
    internal class BottomLeftAlgorithm : AlgorithmNode
    {
        private IReadOnlyList<BottomLeftItemData> _data;
        /// <summary>
        /// 許容空き数
        /// </summary>
        private readonly int _maxSize;

        public BottomLeftAlgorithm(WaitPackingSearch option, int startIndex, BottomLeftItemData[] data) : base(option, startIndex)
        {
            _data = data;
            _maxSize = data.Max(d => d.Amount);
        }

        internal override void Process(CancellationToken token)
        {
            ProcessRecursive(token);
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        private bool ProcessRecursive(CancellationToken token)
        {
            // キャンセルされた
            if (token.IsCancellationRequested)
                return false;
            
            // 全部置いた もしくは全部埋めた
            if (_unusedStack.Count <= 0 || _box.EmptyCount() <= allowCount)
            {
                AddSuccessLog();
                return true;
            }

            AddLog(_box);

            if (!Continuable(_box))
                return false;
            
            // 最初のピースを取り出す
            var index = _unusedStack.Peek();
            var data = _data[index];
            foreach (var points in data.Maps)
            {
                var noAllValid = points.Any(p => !_box.Valid(p));
                // 置けない
                if (noAllValid)
                    continue;
                
                _box.Put(points, data.Index);
                
                // おけたので削除
                _unusedStack.Pop();
                
                // 置けたら次をチェック
                if (ProcessRecursive(token))
                    return !all;
                
                // 見つからなかったので今置いたやつを戻して次を探す
                _box.Reset(points);
                _unusedStack.Push(index);
            }
            
            // 解無し
            return false;
        }
        
        /// <summary>
        /// 現在の状態で埋めを継続できるか確認する
        /// </summary>
        private bool Continuable(Box box)
        {
            // 空き領域が最小サイズより少ない場合は置けない
            if (box.EmptyCount() < _maxSize) 
                return false;

            return true;
        }
    }
}