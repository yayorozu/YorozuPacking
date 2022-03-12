using System.Collections.Generic;

namespace Yorozu
{
    public class CoverResult : IPrint
    {
        int[,] IPrint.Map => SuccessMap;
            
        public bool Success { get; }
        public int[,] SuccessMap { get; }
        public IEnumerable<Log> Logs { get; }
        /// <summary>
        /// 計算時間
        /// </summary>
        public int Milliseconds { get; }

        public CoverResult(bool success, int[,] successMap, int milliseconds, IEnumerable<Log> logs)
        {
            Success = success;
            SuccessMap = successMap;
            Milliseconds = milliseconds;
            Logs = logs;
        }

        public string SuccessMapString()
        {
            return this.Print();
        }
    }
}