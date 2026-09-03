
/// <summary>
/// マス目の情報
/// </summary>
public class MassData
{
    /// <summary>
    /// 石の色
    /// </summary>
    public StoneColor StoneColor;
    /// <summary>
    /// 棋譜
    /// </summary>
    public string Record { get; private set; }
    
    private StoneColor _stoneColor;

    public MassData(StoneColor colorType, string record)
    {
        StoneColor = colorType;
        Record = record;
    }
}
