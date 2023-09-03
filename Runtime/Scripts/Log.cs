namespace Yorozu
{
    public class Log : IPrint
    {
        public int[,] Map { get; }
        public int EmptyCount  { get; } 

        internal Log(int[,] map, int empty)
        {
            Map = map;
            EmptyCount = empty;
        }
        
        public override string ToString()
        {
            return this.Print();
        }
    }
}