using UnityEngine;

/// <summary>
/// ゲームの管理
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("AIを使うか"), SerializeField] private bool _isUseAI;
    [Header("棋譜を使うか"), SerializeField] private bool _isRecord;
    
    public bool IsUseAI => _isUseAI;
    public bool IsRecord => _isRecord;
    
    public bool IsGameEnd { get; private set; }
    public StoneColor WinnerStone { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    /// <param name="winnerStone">勝利した石の色</param>
    public void GameEnd(StoneColor winnerStone)
    {
        IsGameEnd = true;
        WinnerStone = winnerStone;
    }
}
