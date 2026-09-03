using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 棋譜の管理
/// </summary>
public class GameRecordManager : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private GameTurnManager _gameTurnManager;
    [Header("棋譜データ"), TextArea, SerializeField] private string _gameRecordData;
    
    /// <summary>
    /// 棋譜の入力
    /// </summary>
    private readonly List<string> _gameRecordInput = new List<string>();
    /// <summary>
    /// 棋譜の出力
    /// </summary>
    private readonly List<string> _gameRecordOutput = new List<string>();
    /// <summary>
    /// 現在の棋譜
    /// </summary>
    private int _currentGameRecord;
    
    private void Start()
    {
        LoadGameRecordData(_gameRecordData);
    }

    /// <summary>
    /// 棋譜データを読み込む
    /// </summary>
    /// <param name="gameRecord">棋譜データ</param>
    private void LoadGameRecordData(string gameRecord)
    {
        var countNum = 0;
        // 棋譜データを分割する
        for (int i = 0; i < gameRecord.Length / 2; i++)
        {
            // ２文字ずつ取り出す
            var alpCount = countNum;
            var numCount = countNum + 1;
            var alphabet = gameRecord[alpCount].ToString();
            var num = gameRecord[numCount].ToString();
            _gameRecordInput.Add(alphabet + num);
                
            countNum = numCount + 1;
        }
    }

    /// <summary>
    /// 棋譜の通りに動かす
    /// </summary>
    /// <param name="gameRecord">1なら次、-1なら前の状態に動かす</param>
    public void MoveGameRecord(int gameRecord)
    {
        if (gameRecord > 0)
        {
            if(_currentGameRecord >= _gameRecordInput.Count) return;
            var record = _gameRecordInput[_currentGameRecord].ToUpper(); // 大文字に統一
            // 棋譜を配列の座標に変換する
            var row = record[1] - '1';
            var col = record[0] - 'A';

            if (!_boardManager.PutStone(row, col)) // 置くことができなかった場合
            {
                Debug.LogWarning($"置くことが出来なかった{record}");
                return;
            }

            _gameTurnManager.ChangeCurrentTurnStoneColor();
            _currentGameRecord++;
            _boardManager.CanPutBoardUpdate();
        }
        else
        {
            if(_currentGameRecord <= 0) return;
            _currentGameRecord--;
            _boardManager.UndoPutStone();
        }
    }
}
