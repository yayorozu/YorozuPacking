using System.Linq;

namespace Yorozu
{
    internal class BottomLeftSearcher : SearcherAbstract
    {
        private BottomLeftItemData[] _data;
        
        public BottomLeftSearcher(WaitPackingSearch owner, Algorithm algorithm) : base(owner)
        {
            _data = owner.shapes
                    .Select((v, i) => new BottomLeftItemData(v, owner.size, i, algorithm))
                    // 大きい順に並べ替え
                    .OrderByDescending(d => d.Amount)
                    .ToArray()
                ;
        }
        
        protected override AlgorithmNode GetNode(int startIndex)
        {
            return new BottomLeftAlgorithm(_owner, startIndex, _data);
        }
    }
}