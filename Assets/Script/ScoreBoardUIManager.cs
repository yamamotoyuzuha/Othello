using TMPro;
using UnityEngine;

/// <summary>
/// スコアボードに表示するUIの管理
/// </summary>
public class ScoreBoardUIManager : MonoBehaviour
{
    public static ScoreBoardUIManager Instance { get; private set; }
    
    [Header("石の数"), SerializeField] private TextMeshProUGUI _stoneCountText;
    [Header("持ち時間"), SerializeField] private TextMeshProUGUI _timeText;
    [Header("手番"), SerializeField] private TextMeshProUGUI _turnText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetStoneCount(int black, int white)
    {
        _stoneCountText.text = $"B:{black} / W:{white}";
    }

    public void SetTime(int time)
    {
        _timeText.text = time.ToString();
    }

    public void SetTurn(string turn)
    {
        _turnText.text = turn;
    }
}
