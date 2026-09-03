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
    
    private StoneColor _previousTurnStoneColor; // 前の手番
    private float _blackTimeLimitTimer; // 黒の持ち時間
    private float _whiteTimeLimitTimer; // 白の持ち時間

    private void Start()
    {
        _currentTurnStoneColor = _startTurnStoneColor;
        ScoreBoardUIManager.Instance.SetTurn(_currentTurnStoneColor == StoneColor.Black ? "Black" : "White");
        _blackTimeLimitTimer = _timeLimit;
        _whiteTimeLimitTimer = _timeLimit;
    }

    private void Update()
    {
        TimeLimitTimer();
    }

    /// <summary>
    /// 現在の手番切替
    /// </summary>
    public void ChangeCurrentTurnStoneColor()
    {
        _currentTurnStoneColor = _currentTurnStoneColor == StoneColor.Black ?  StoneColor.White : StoneColor.Black;
        ScoreBoardUIManager.Instance.SetTurn(_currentTurnStoneColor == StoneColor.Black ? "Black" : "White");
        _blackTimeLimitTimer = _timeLimit;
        _whiteTimeLimitTimer = _timeLimit;
    }

    /// <summary>
    /// 経過時間の更新
    /// </summary>
    private void TimeLimitTimer()
    {
        switch (_currentTurnStoneColor)
        {
            case StoneColor.Black:
                _blackTimeLimitTimer -= Time.deltaTime;
                ScoreBoardUIManager.Instance.SetTime((int)_blackTimeLimitTimer);
                if (_blackTimeLimitTimer <= 0)
                {
                    Debug.LogWarning("黒の時間切れ");
                }
                break;
            case StoneColor.White:
                _whiteTimeLimitTimer -= Time.deltaTime;
                ScoreBoardUIManager.Instance.SetTime((int)_whiteTimeLimitTimer);
                if (_whiteTimeLimitTimer <= 0)
                {
                    Debug.LogWarning("白の時間切れ");
                }
                break;
        }
    }
}
