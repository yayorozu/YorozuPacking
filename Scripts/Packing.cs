using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 瓶詰め詰めアルゴリズムを利用して瓶詰めを行う
    /// Bottom-Left Algorithm
    /// </summary>
    public static class Packing
    {
        public const int EMPTY = -1;
        
        /// <summary>
        /// サイズを指定してそのサイズ内に入るかどうかを判定する
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static bool Evol(int width, int height, IEnumerable<int[,]> shapes)
        {
            var boolShapes = shapes.Select(Convert);
            return Evol(width, height, boolShapes);
            
            static bool[,] Convert(int[,] shape)
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
        
        public static bool Evol(int width, int height, IEnumerable<bool[,]> shapes)
        {
            var count = shapes.Count();
            
            var data = new ItemData[count];
            var size = new Vector2Int(width, height);
            for (var i = 0; i < count; i++)
            {
                data[i] = new ItemData(shapes.ElementAt(i), size);
            }

            var evolve = new Evolve(size, data);
            var success = evolve.Exec();
            
            return success;
        }
    }

    /// <summary>
    /// 解を求める
    /// </summary>
    internal class Evolve
    {
        private Bottle _bottle;
        private ItemData[] _data;
        private HashSet<int> _unusedHash;
        private int _minAmount;
        
        internal Bottle Bottle => _bottle;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size"></param>
        /// <param name="data"></param>
        /// <param name="startIndex">全パターン試せるように</param>
        internal Evolve(Vector2Int size, ItemData[] data)
        {
            _bottle = new Bottle(size);
            _data = data;
            _unusedHash = new HashSet<int>(_data.Length);
            for (var i = 0; i < _data.Length; i++)
            {
                _unusedHash.Add(i);
            }

            _minAmount = data.Max(d => d.Amount);
        }

        /// <summary>
        /// 検索開始
        /// </summary>
        internal bool Exec()
        {
            // 全部置いた
            if (_unusedHash.Count <= 0)
                return true;

            // 空き領域が最小サイズより少ない
            if (_bottle.EmptyCount() < _minAmount)
                return false;

            // 最初のピースを取り出す
            var index = _unusedHash.First();
            var data = _data[index];
            foreach (var map in data.Maps)
            {
                var noAllValid = map.Any(p => !_bottle.Valid(p));
                // 置けない
                if (noAllValid)
                    continue;
                
                // TODO 空き領域チェック
                
                _bottle.Put(map, index);
                
                // おけたので削除
                _unusedHash.Remove(index);
                
                // 置けたら次をチェック
                if (Exec())
                    return true;
                
                // 見つからなかったので今置いたやつを戻して次を探す
                _bottle.Reset(map);
                _unusedHash.Add(index);
            }

            // 解無し
            return false;
        }
    }
}











