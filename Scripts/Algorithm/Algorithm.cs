namespace Yorozu
{
    internal enum Algorithm
    {
        /// <summary>
        /// 下左優先
        /// </summary>
        BottomLeft,
        /// <summary>
        /// 左下優先
        /// 形によってはこっちのほうがいいこともある
        /// </summary>
        LeftBottom,
        /// <summary>
        /// Knuth's Algorithm X の 実装 Exact Cover Problem を解くためのアルゴリズム
        /// 基本的な考え方はバックトラックによる全探索
        /// </summary>
        X,
        /// <summary>
        /// Xの効率化を計ったアルゴリズムより早くなるらしい
        /// Dancing Links X
        /// </summary>
        DLX,
    }
}