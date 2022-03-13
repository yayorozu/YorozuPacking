namespace Yorozu
{
    public static class PackingUtility
    {
        /// <summary>
        /// 空
        /// </summary>
        public const int EMPTY = -1;
        /// <summary>
        /// 無効領域
        /// </summary>
        public const int INVALID = -2;

        internal static string Print(this IPrint print)
        {
            if (print.Map == null)
                return "Null";

            var builder = new System.Text.StringBuilder();
            for (var y = 0; y < print.Map.GetLength(1); y++)
            {
                for (var x = 0; x < print.Map.GetLength(0); x++)
                {
                    if (print.Map[x, y] == EMPTY)
                    {
                        builder.Append($"\t*, ");
                    }
                    else if (print.Map[x, y] == INVALID)
                    {
                        builder.Append($"\t#, ");
                    }
                    else
                    {
                        builder.Append($"\t{print.Map[x, y]}, ");    
                    }
                }

                builder.AppendLine("");
            }

            return builder.ToString();
        }
    }
}