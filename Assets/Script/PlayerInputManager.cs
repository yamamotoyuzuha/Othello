using UnityEngine;

/// <summary>
/// プレイヤーの入力を管理する
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private GameTurnManager _gameTurnManager;
    [SerializeField] private GameRecordManager _gameRecordManager;
    [Header("カーソル初期位置")]
    [SerializeField] private int _currentRow;
    [SerializeField] private int _currentColumn;
    
    private void Start()
    {
        _boardManager.SelectBoardColor(_currentRow, _currentColumn);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(!_boardManager.IsWithinRange(_currentRow, _currentColumn - 1)) return;
            _boardManager.DeselectBoardColor(_currentRow, _currentColumn);
            _currentColumn--;
            _boardManager.SelectBoardColor(_currentRow, _currentColumn);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(!_boardManager.IsWithinRange(_currentRow, _currentColumn + 1)) return;
            _boardManager.DeselectBoardColor(_currentRow, _currentColumn);
            _currentColumn++;
            _boardManager.SelectBoardColor(_currentRow, _currentColumn);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(!_boardManager.IsWithinRange(_currentRow - 1, _currentColumn)) return;
            _boardManager.DeselectBoardColor(_currentRow, _currentColumn);
            _currentRow--;
            _boardManager.SelectBoardColor(_currentRow, _currentColumn);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(!_boardManager.IsWithinRange(_currentRow + 1, _currentColumn)) return;
            _boardManager.DeselectBoardColor(_currentRow, _currentColumn);
            _currentRow++;
            _boardManager.SelectBoardColor(_currentRow, _currentColumn);
        }

        // 置く
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _boardManager.PutStone(_currentRow, _currentColumn);
            _gameTurnManager.ChangeCurrentTurnStoneColor();
            _boardManager.CanPutBoardUpdate();
        }
        
        GameRecordInput();
    }

    /// <summary>
    /// 棋譜を動かす
    /// </summary>
    private void GameRecordInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _gameRecordManager.MoveGameRecord(-1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            _gameRecordManager.MoveGameRecord(1);
        }
    }
}
