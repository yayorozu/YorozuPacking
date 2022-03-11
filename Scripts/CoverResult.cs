using System.Collections.Generic;

namespace Yorozu
{
    public class CoverResult : IPrint
    {
        int[,] IPrint.Map => SuccessMap;
            
        public bool Success { get; }
        public int[,] SuccessMap { get; }
        public IEnumerable<Log> Logs { get; }

        public CoverResult(bool success, int[,] successMap)
        {
            Success = success;
            SuccessMap = successMap;
        }
            
        public CoverResult(bool success, int[,] successMap, IEnumerable<Log> logs) : this(success, successMap)
        {
            Logs = logs;
        }

        public string SuccessMapString()
        {
            return this.Print();
        }
    }
}