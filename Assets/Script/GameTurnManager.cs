using UnityEngine;

/// <summary>
/// ゲームの流れを管理する
/// ・手番、持ち時間など
/// </summary>
public class GameTurnManager : MonoBehaviour
{
    [Header("開始手番"), SerializeField] private StoneColor _startTurnStoneColor;
    [Header("現在の手番"), SerializeField] private StoneColor _currentTurnStoneColor;
    [Header("持ち時間"), SerializeField] private float _timeLimit;
    /// <summary>
    /// 現在の手番
    /// </summary>
    public StoneColor CurrentTurnStoneColor => _currentTurnStoneColor;
    
    private float _timeLimitTimer; // 待ち時間の経過時間

    private void Awake()
    {
        _currentTurnStoneColor = _startTurnStoneColor;
    }

    /// <summary>
    /// 現在の手番切替
    /// </summary>
    public void ChangeCurrentTurnStoneColor()
    {
        _currentTurnStoneColor = _currentTurnStoneColor == StoneColor.Black ?  StoneColor.White : StoneColor.Black;
    }
}
