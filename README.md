# YorozuCover
Bottom-Left Algorithm を利用して対象の図形を四角形に詰めれるか確認するライブラリ

```c#
private IEnumerator Calc()
{
    var shapes = new List<bool[,]>();
    // 対象のサイズと利用する図形データを渡す
    var cover = new WaitPackingSearch(_width, _height, shapes);
    // 計算を開始する
    cover.Evaluate(parallel: false, all: false);

    // 計算を開始後条件を満たすと処理が終わる 
    yield return cover;
    
    // 結果を参照して全部詰められたかがわかる
    cover.Result.Success;
 
    // 成功していた場合はどう埋めたのか確認できる
    cover.Result.SuccessMap   
    
   
   
   
   
    // logScore を指定すると指定した数埋まっていなかったとしてもログとして記録しておく
    cover.Evaluate(parallel: false, all: false, logScore: 3);
    
    // データは以下に記録される
    cover.Result.Logs
}
```