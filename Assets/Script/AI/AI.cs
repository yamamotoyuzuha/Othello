/// <summary>
/// オセロAIの基底クラス
/// </summary>
public abstract class AI
{
    protected BoardManager BoardManager;
    
    protected AI(BoardManager boardManager)
    {
        BoardManager = boardManager;
    }
    
    /// <summary>
    /// AIの実行処理
    /// </summary>
    /// <param name="massData">現在の盤面</param>
    /// <param name="stoneColor">AIの石の色</param>
    public abstract void ThinkingAI(MassData[,] massData, StoneColor stoneColor);
}
