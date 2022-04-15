using System.Linq;

namespace Yorozu
{
    internal class BottomLeftSearcher : SearcherAbstract
    {
        internal BottomTopItemData[] data;
        
        public BottomLeftSearcher(WaitPackingSearch owner, Algorithm algorithm) : base(owner)
        {
            data = owner.shapes
                    .Select((v, i) => new BottomTopItemData(v, owner.size, i, algorithm))
                    // 大きい順に並べ替え
                    .OrderByDescending(d => d.Amount)
                    .ToArray()
                ;
        }
        
        protected override AlgorithmNode GetNode(int startIndex)
        {
            return new BottomTopAlgorithm(_owner, startIndex, data);
        }
    }
}