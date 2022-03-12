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
        public int CalcMilliseconds { get; }

        public CoverResult(bool success, int[,] successMap, int calcMilliseconds, IEnumerable<Log> logs)
        {
            Success = success;
            SuccessMap = successMap;
            CalcMilliseconds = calcMilliseconds;
            Logs = logs;
        }

        public string SuccessMapString()
        {
            return this.Print();
        }
    }
}