namespace Yorozu
{
    public class Log : IPrint
    {
        private int[,] _map;
        private int _empty;
        
        public int[,] Map => _map;
        public int EmptyCount => _empty; 

        internal Log(int[,] map, int empty)
        {
            _map = map;
            _empty = empty;
        }
        
        public override string ToString()
        {
            return this.Print();
        }
    }
}