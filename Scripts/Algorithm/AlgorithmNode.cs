using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Yorozu
{
    internal abstract class AlgorithmNode
    {
        private WaitPackingSearch _owner;
        private List<Log> _logs;
        
        protected Box _box;
        protected Stack<int> _unusedStack;
        protected int allowCount => _owner.allowCount;

        internal List<Log> Logs => _logs;
        internal int[,] CurrentMap => _box.Map;
        
        internal AlgorithmNode(WaitPackingSearch owner, int startIndex)
        {
            _owner = owner;
            _box = new Box(owner);
            _unusedStack = new Stack<int>(_owner.shapes.Count());
            for (var i = _owner.shapes.Count() - 1; i >= 0; i--) 
                _unusedStack.Push((startIndex + i) % _owner.shapes.Count());
        }
        
        internal abstract bool Process(CancellationToken token);
        
        /// <summary>
        /// ログ追加
        /// </summary>
        protected void AddLog(Box box)
        {
            if (_owner.logScore < 0 || box.EmptyCount() > _owner.logScore) 
                return;
            
            _logs.Add(new Log(box.Copy(), box.EmptyCount()));
        }
    }
}