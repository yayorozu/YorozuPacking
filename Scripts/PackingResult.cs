using System.Collections.Generic;
using System.Linq;

namespace Yorozu
{
    public class PackingResult : IPrint
    {
        int[,] IPrint.Map => SuccessMap;
        private List<int[,]> _successMaps;

        public bool Success => _successMaps.Count > 0;
        public int[,] SuccessMap => Success ? _successMaps.First() : null;
        public IEnumerable<int[,]> SuccessMaps => _successMaps;
        public IEnumerable<Log> Logs { get; }
        
        /// <summary>
        /// 計算時間
        /// </summary>
        public int CalcMilliseconds { get; }

        public PackingResult(List<int[,]> successMaps, int elapsedTime, List<Log> logs)
        {
            _successMaps = successMaps;
            CalcMilliseconds = elapsedTime;
            Logs = logs;

        }

        public string SuccessMapString()
        {
            return this.Print();
        }
    }
}