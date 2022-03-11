namespace Yorozu
{
    internal static class CoverUtility
    {
        internal static string Print(this IPrint print)
        {
            if (print.Map == null)
                return "Null";

            var builder = new System.Text.StringBuilder();
            for (var y = 0; y < print.Map.GetLength(1); y++)
            {
                for (var x = 0; x < print.Map.GetLength(0); x++)
                {
                    if (print.Map[x, y] >= 0)
                    {
                        builder.Append($"\t{print.Map[x, y]}, ");    
                    }
                    else
                    {
                        builder.Append($"\t , ");
                    }
                }

                builder.AppendLine("");
            }

            return builder.ToString();
        }
    }
}