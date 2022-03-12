using System.Collections.Generic;
using System.Linq;
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
        private HashSet<int> _unusedHash;
        
        internal int[,] CurrentMap => _box.Map;

        internal SearcherNode(Searcher owner, HashSet<int> indexes)
        {
            _owner = owner;
            _box = new Box(owner.size);
            _unusedHash = indexes;
        }
        
        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        internal bool ProcessRecursive()
        {
            _owner.AddLog(_box);

            // 全部置いた
            if (_unusedHash.Count <= 0)
                return true;
            
            if (!_owner.Continuable(_box))
                return false;
            
            // 最初のピースを取り出す
            var index = _unusedHash.First();
            var data = _owner.data[index];
            foreach (var map in data.Maps)
            {
                var noAllValid = map.Any(p => !_box.Valid(p));
                // 置けない
                if (noAllValid)
                    continue;
                
                _box.Put(map, index);
                
                // おけたので削除
                _unusedHash.Remove(index);
                
                // 置けたら次をチェック
                if (ProcessRecursive())
                    return true;
                
                // 見つからなかったので今置いたやつを戻して次を探す
                _box.Reset(map);
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