
/// <summary>
/// オセロの盤面データ
/// </summary>
public class BoardData
{
    /// <summary>
    /// 盤面に石が置かれているか
    /// true：置かれている　false：おかれていない
    /// </summary>
    public bool IsArrangement { get; private set; }
    /// <summary>
    ///　石の色
    /// </summary>
    public ColorType Type { get; private set; }
        
    public BoardData(bool b, ColorType c)
    {
        IsArrangement = b;
        Type = c;
    }
}