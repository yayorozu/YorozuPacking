using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Yorozu
{
    /// <summary>
    /// 行列を作って消していく
    /// </summary>
    internal class XSearcher : SearcherAbstract
    {
        internal XSearcher(WaitPackingSearch owner) : base(owner)
        {
        }

        protected override bool ValidAllIndexSearch => false;

        protected override AlgorithmNode GetNode(int startIndex)
        {
            return new XAlgorithm(_owner, startIndex);
        }
    }

    /// <summary>
    /// Index関係無く処理するため1回で終わる
    /// </summary>
    internal class XAlgorithm : AlgorithmNode
    {
        /// <summary>
        /// 削除した縦のIndex
        /// </summary>
        private HashSet<int> _deletedRows;
        private XItemData[] _data;
        private int _length;
        private CancellationToken _token;
        
        internal XAlgorithm(WaitPackingSearch owner, int startIndex) : base(owner, startIndex)
        {
            _data = owner.shapes
                    .Select((v, i) => new XItemData(v, owner.size, i))
                    .ToArray()
                ;
            
            _length = owner.size.x * owner.size.y;
            _deletedRows = new HashSet<int>(_length);
        }

        internal override void Process(CancellationToken token)
        {
            _token = token;
            ProcessRecursive();
        }

        private bool ProcessRecursive()
        {
            // キャンセルされた
            if (_token.IsCancellationRequested)
                return false;

            var deleteRows = FindMinSumRow();
            // 全部置いたか許容数埋まった場合終わり
            if (_length - _deletedRows.Count <= allowCount)
            {
                _box.Clear();
                // データを適応してマップを作成
                foreach (var data in _data)
                {
                    var points = data.FixPositions();
                    _box.Put(points, data.Index);
                }
                
                AddSuccessLog();
                return true;
            }
            // もうこれ以上削除できないのに正解判定にひっかからなかった場合は違う
            if (deleteRows.Count <= 0)
                return false;

            // 関連列を削除していく
            foreach (var deleteRow in deleteRows)
            {
                if (DeleteRelativeRows(deleteRow))
                    return true;
            }

            // どれも見つからなかった
            return false;
        }

        /// <summary>
        /// 指定した行の中で数値がある箇所を候補にする
        /// </summary>
        private bool DeleteRelativeRows(int row)
        {
            foreach (var data in _data)
            {
                if (data.IsFix)
                    continue;
                
                // 対象列を含むデータ一覧を探す、見つかればその行が持ってる列を不可にする
                foreach (var tuple in data.HasRows(row))
                {
                    data.UpdateSelectColum(tuple.column);
                    
                    foreach (var rowIndex in tuple.hash) 
                        _deletedRows.Add(rowIndex);

                    // 対象列を含むデータを候補から外す
                    foreach (var data2 in _data)
                    {
                        if (data2.IsFix)
                            continue;
                        
                        if (data == data2)
                            continue;
                        
                        data2.DeleteColumns(tuple.hash);
                    }

                    if (validLog)
                    {
                        var fixDataIndexes = string.Join(",", _data.Where(d => d.IsFix).Select(d => d.Index.ToString()).ToArray());
                        PrintColumns($"Delete Row: {row}, FixDataIndex: {data.Index} FixedIndexes: {fixDataIndexes}");
                    }

                    // 問題なければ更に確認
                    if (ProcessRecursive())
                        return true;
                    
                    // もとに戻す
                    foreach (var data2 in _data)
                    {
                        if (data2.IsFix)
                            continue;
                        
                        if (data == data2)
                            continue;
                        
                        data2.UndoDeleteColumns();
                    }
                    
                    // 確定フラグを戻す
                    data.UpdateSelectColum(null);
                    
                    // 続けられないので戻す
                    foreach (var rowIndex in tuple.hash) 
                        _deletedRows.Remove(rowIndex);
                    
                    if (validLog)
                    {
                        PrintColumns($"Undo Row: {row}, FixDataIndex: {data.Index}");
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// 合計値が少ない列を探す
        /// </summary>
        /// <returns></returns>
        private List<int> FindMinSumRow()
        {
            var dic = new SortedDictionary<int, List<int>>();
            for (var row = 0; row < _length; row++)
            {
                if (_deletedRows.Contains(row))
                    continue;

                var sum = 0;
                foreach (var data in _data)
                {
                    if (data.IsFix)
                        continue;
                    sum += data[row];
                }
                
                // 0の列は探しても仕方ないので除外
                if (sum == 0)
                    continue;
                
                if (!dic.ContainsKey(sum))
                {
                    dic.Add(sum, new List<int>());
                }
                dic[sum].Add(row);
            }
            
            // 全部できた
            if (dic.Count <= 0)
                return new List<int>();

            var key = dic.Keys.First();
            if (validLog)
            {
                var rowsString = string.Join(",", dic[key].Select(r => r.ToString()).ToArray());
                PrintColumns($"Sum: {key} TargetRows: {rowsString}");
            }

            return dic[key];
        }
        
        /// <summary>
        /// 現在のカラムの状態を表示
        /// </summary>
        private void PrintColumns(string additional = "")
        {
            var builder = new StringBuilder();
            var index = 0;
            
            builder.AppendLine(additional);

            builder.Append("                       ");
            for (var r = 0; r < _length; r++)
            {
                if (_deletedRows.Contains(r))
                    continue;

                builder.Append($"{r:D2}..");
            }

            builder.AppendLine("");

            foreach (var data in _data)
            {
                if (!data.IsFix)
                {
                    foreach (var column in data.PrintValidColumns(_deletedRows, index))
                    {
                        builder.AppendLine(column);
                    }
                }

                index += data.PatternCount;
            }

            Logger(builder.ToString());
        }
    }
}