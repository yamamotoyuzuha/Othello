using System;
using UnityEngine;

/// <summary>
/// negamax法
/// １，AIが置ける候補の場所を全探索
/// ２，再帰処理で手番を切り替えながら、先を読む分まで盤面を進める
/// ３，盤面を元にスコアを計算。１番最後に仮置きしたところから１手前に向かって比較していく
/// これを候補の場所分、行い１番いい手を決める
/// </summary>
public class NegaMaxAI : AI
{
    private readonly int _searchDepth;
    
    /// <summary>
    /// 評価値
    /// </summary>
    private static readonly int[,] EvaluateStoneStatesScore = 
    {
        { 30, -12, 0, -1, -1, 0, -12, 30 },
        { -12, -15, -3, -3, -3, -3, -15, -12 },
        { 0, -3, 0, -1, -1, 0, -3, 0 },
        { -1, -3, -1, -1, -1, -1, -3, -1 },
        { -1, -3, -1, -1, -1, -1, -3, -1 },
        { 0, -3, 0, -1, -1, 0, -3, 0 },
        { -12, -15, -3, -3, -3, -3, -15, -12 },
        { 30, -12, 0, -1, -1, 0, -12, 30 },
    };

    public NegaMaxAI(BoardManager boardManager, int searchDepth = 3) : base(boardManager)
    {
        _searchDepth = searchDepth;
    }
    
    public override void ThinkingAI(MassData[,] massData, StoneColor stoneColor)
    {
        // 現在の盤面をコピー
        var copyMassData = BoardManager.CopyBoard(massData);
        // 最適な手を取得する
        var result = SearchNegaMaxStone(copyMassData, stoneColor, _searchDepth);
        if (result == (-1, -1)) // 手が見つからなかった
        {
            Debug.LogWarning("見つかりませんでした");
            return;
        }
        
        // 実際に石を置く
        BoardManager.PutStone(result.Item1, result.Item2);
    }

    /// <summary>
    /// 候補の中から高い評価値を得られる手を探索する
    /// </summary>
    /// <param name="massData">現在の盤面</param>
    /// <param name="stoneColor">手番の石の色</param>
    /// <param name="depth">探索の深さ</param>
    /// <returns>最適な手の位置（row、column）</returns>
    private (int, int) SearchNegaMaxStone(MassData[,] massData, StoneColor stoneColor, int depth)
    {
        (int, int) resultStoneIndex = (-1, -1);
        
        var maxScore = int.MinValue;
        
        // 現在の手番で置ける位置を全て取得する
        var canPutPosition = BoardManager.GetCanPutBoardPositions(massData, stoneColor);
        foreach (var canPut in canPutPosition)
        {
            //Debug.LogWarning($"候補手: {stoneColor} ({canPut.PutPosition.row}, {canPut.PutPosition.column})");
            // 盤面をコピー
            var copyMassData = BoardManager.CopyBoard(massData);
            // 仮で石を置き、置いた後の盤面を取得する
            var putStone = BoardManager.PutStoneTemporarily(copyMassData, canPut.PutPosition.row, canPut.PutPosition.column, stoneColor);
            // 相手が置いた評価値を反転して、自分の評価値とする
            var score = -1 * GetNegaMaxScore(putStone, GetStoneColor(stoneColor), depth - 1);

            // 評価値を更新
            if (maxScore < score)
            {
                maxScore = score;
                resultStoneIndex = (canPut.PutPosition.row, canPut.PutPosition.column);
            }
        }
        return resultStoneIndex;
    }

    /// <summary>
    /// 評価値を計算
    /// </summary>
    /// <param name="massData">現在の盤面</param>
    /// <param name="stone">手番の石の色</param>
    /// <param name="depth">探索の深さ</param>
    /// <param name="isPass">パスしたかどうか</param>
    /// <returns>最大スコア</returns>
    private int GetNegaMaxScore(MassData[,] massData, StoneColor stone, int depth, bool isPass = false)
    {
        // 探索上限に達したら
        if(depth == 0) return EvaluateStoneStates(massData, stone);

        var maxScore = int.MinValue;

        // 現在の手番で置ける位置を全て取得する
        var canPutPosition = BoardManager.GetCanPutBoardPositions(massData, stone);
        foreach (var canPut in canPutPosition)
        {
            // 仮で石を置き、置いた後の盤面を取得する
            var copyMassData = BoardManager.CopyBoard(massData);
            var putStone =
                BoardManager.PutStoneTemporarily(copyMassData, canPut.PutPosition.row, canPut.PutPosition.column, stone);
            maxScore = Math.Max(maxScore, -1 * GetNegaMaxScore(putStone, GetStoneColor(stone), depth - 1));
        }

        // 見つからなかった場合
        if (maxScore == int.MinValue)
        {
            // ２回連続パスの場合、評価関数を実行
            if(isPass) return EvaluateStoneStates(massData, stone);
            // 相手の手番にして、評価値を反転して返す
            return -1 * GetNegaMaxScore(massData, GetStoneColor(stone), depth - 1, true);
        }

        return maxScore;
    }
    
    /// <summary>
    /// 指定した石の色で合計評価値を算出する
    /// </summary>
    /// <param name="massData">評価する盤面</param>
    /// <param name="stone">指定する石の色</param>
    /// <returns>評価スコア</returns>
    private int EvaluateStoneStates(MassData[,] massData, StoneColor stone)
    {
        var blackScore = 0;
        var whiteScore = 0;
        
        // 黒と白それぞれの合計評価スコアを算出
        for (int i = 0; i < massData.GetLength(0); i++)
        {
            for (int j = 0; j < massData.GetLength(1); j++)
            {
                var score = EvaluateStoneStatesScore[i, j];
                if (massData[i, j].StoneColor == StoneColor.Black)
                {
                    blackScore += score;
                }
                else if (massData[i, j].StoneColor == StoneColor.White)
                {
                    whiteScore += score;
                }
            }
        }
        
        // 指定した石の色でスコアを返す
        if (stone == StoneColor.Black)
        {
            return blackScore - whiteScore;
        }
        return whiteScore - blackScore;
    }

    /// <summary>
    /// 反対の石の色を取得する
    /// </summary>
    /// <param name="stoneColor">基準の石の色</param>
    /// <returns>反対の石の色</returns>
    private StoneColor GetStoneColor(StoneColor stoneColor)
    {
        return stoneColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
    }
}
