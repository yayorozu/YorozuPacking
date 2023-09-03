using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Yorozu
{
    internal abstract class AlgorithmNode
    {
        private WaitPackingSearch _owner;
        private List<Log> _logs;
        
        protected Box _box;
        protected Stack<int> _unusedStack;
        protected int allowCount => _owner.allowCount;
        protected bool all => _owner.all;
        protected bool validLog { get; }
        
        internal List<Log> Logs => _logs;
        internal int[,] CurrentMap => _box.Map;
        /// <summary>
        /// 成功したマップ
        /// </summary>
        internal List<int[,]> SuccessMap => _successMap;
        private List<int[,]> _successMap;
        
        internal AlgorithmNode(WaitPackingSearch owner, int startIndex)
        {
            _owner = owner;
            validLog = owner.logger != null;
            _box = new Box(owner);
            _logs = new List<Log>();
            _successMap = new List<int[,]>();
            _unusedStack = new Stack<int>(_owner.shapes.Count());
            for (var i = _owner.shapes.Count() - 1; i >= 0; i--) 
                _unusedStack.Push((startIndex + i) % _owner.shapes.Count());
        }
        
        internal abstract void Process(CancellationToken token);

        /// <summary>
        /// 成功地図を記録
        /// </summary>
        protected void AddSuccessLog()
        {
            var dst = new int[CurrentMap.GetLength(0), CurrentMap.GetLength(1)];
            for (var a = 0; a < CurrentMap.GetLength(0); a++)
            for (var b = 0; b < CurrentMap.GetLength(1); b++)
                dst[a, b] = CurrentMap[a, b];
            
            _successMap.Add(dst);
        }
        
        /// <summary>
        /// ログ追加
        /// </summary>
        protected void AddLog(Box box)
        {
            if (_owner.logScore < 0 || box.EmptyCount() > _owner.logScore) 
                return;
            
            _logs.Add(new Log(box.Copy(), box.EmptyCount()));
        }

        protected void Logger(string logText)
        {
            _owner.logger.Log(logText);
        }
    }
}