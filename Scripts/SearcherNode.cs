using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 設定をもとに探索を行う
    /// </summary>
    internal class SearcherNode
    {
        private Searcher _owner;
        private Box _box;
        private List<int> _unusedHash;
        
        internal int[,] CurrentMap => _box.Map;

        internal SearcherNode(Searcher owner, List<int> indexes)
        {
            _owner = owner;
            _box = new Box(owner.size);
            _unusedHash = indexes;
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        /// <param name="token"></param>
        internal bool ProcessRecursive(CancellationToken token)
        {
            // キャンセルされた
            if (token.IsCancellationRequested)
                return false;
            
            // 全部置いた もしくは全部埋めた
            if (_unusedHash.Count <= 0 || _box.EmptyCount() == 0)
                return true;
            
            _owner.AddLog(_box);

            if (!_owner.Continuable(_box))
                return false;
            
            // 最初のピースを取り出す
            var index = _unusedHash.First();
            var data = _owner.data[index];
            foreach (var points in data.Maps)
            {
                var noAllValid = points.Any(p => !_box.Valid(p));
                // 置けない
                if (noAllValid)
                    continue;
                
                _box.Put(points, data.Index);
                
                // おけたので削除
                _unusedHash.Remove(index);
                
                // 置けたら次をチェック
                if (ProcessRecursive(token))
                    return true;
                
                // 見つからなかったので今置いたやつを戻して次を探す
                _box.Reset(points);
                _unusedHash.Add(index);
            }
            
            // 全部見つからないとき最後じゃなければ順番を入れ替える
            if (_unusedHash.Count() >= 2)
            {
                
            }

            // 解無し
            return false;
        }
    }
}