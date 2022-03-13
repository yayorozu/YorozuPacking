using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[{nameof(PackingResult)}]");
            builder.AppendLine($"Success: {Success}");
            builder.AppendLine($"SuccessSearchCount : {_successMaps.Count}");
            builder.AppendLine($"CalcMilliseconds : {CalcMilliseconds}ms");
            if (Success)
            {
                builder.AppendLine($"SuccessMap: ");
                builder.Append($"{this.Print()}");
                builder.AppendLine($"------------------------------------------------");
            }
            if (Logs.Any())
            {
                builder.AppendLine($"Logs");
                foreach (var log in Logs)
                {
                    builder.AppendLine($"{log}");
                    builder.AppendLine($"------------------------------------------------");
                }
            }
            
            return builder.ToString();
        }
    }
}