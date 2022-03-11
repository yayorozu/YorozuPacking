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
        private Box _box;
        private ItemData[] _data;
        private HashSet<int> _unusedHash;
        private int _minAmount;
        private int _logScore;
        private List<Log> _logs;

        internal IEnumerable<Log> Logs => _logs;
        internal int[,] LastMap => _box.Map;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size"></param>
        /// <param name="data"></param>
        internal Searcher(Vector2Int size, ItemData[] data, int logScore = -1)
        {
            _box = new Box(size);
            _data = data;
            _logScore = logScore;
            _unusedHash = new HashSet<int>(_data.Length);
            for (var i = 0; i < _data.Length; i++)
            {
                _unusedHash.Add(i);
            }

            _minAmount = data.Max(d => d.Amount);
            _logs = new List<Log>();
        }

        /// <summary>
        /// 再帰的に探索開始
        /// </summary>
        internal bool ProcessRecursive()
        {
            // ログ
            if (_logScore >= 0 && _box.EmptyCount() <= _logScore)
            {
                _logs.Add(new Log(_box.Copy(), _box.EmptyCount()));
            }
            
            // 全部置いた
            if (_unusedHash.Count <= 0)
                return true;


            // 空き領域が最小サイズより少ない場合は置けない
            if (_box.EmptyCount() < _minAmount)
                return false;

            // 最初のピースを取り出す
            var index = _unusedHash.First();
            var data = _data[index];
            foreach (var map in data.Maps)
            {
                var noAllValid = map.Any(p => !_box.Valid(p));
                // 置けない
                if (noAllValid)
                    continue;
                
                _box.Put(map, index);
                
                // おけたので削除
                _unusedHash.Remove(index);
                
                Debug.LogError(_box.ToString());
                
                // 置けたら次をチェック
                if (ProcessRecursive())
                    return true;
                
                // 見つからなかったので今置いたやつを戻して次を探す
                _box.Reset(map);
                _unusedHash.Add(index);
            }

            // 解無し
            return false;
        }
    }
}