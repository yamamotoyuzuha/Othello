using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盤面を管理する
/// </summary>
public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameTurnManager _gameTurnManager;
    [Header("石の生成位置（Parent）"), SerializeField] private Transform _stoneParent;
    [Header("石"), SerializeField] private GameObject _stonePrefab;
    [Header("石生成時のOffset"), SerializeField] private float _stoneOffset = 0.1f;
    [Header("盤面オブジェクト"), SerializeField] private List<GameObject> _boardObjects;
    [Header("盤面位置"), SerializeField] private List<BoardTransform> _stoneTransforms;
    [Header("盤面の色")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _canPutMaterial;
    [SerializeField] private Material _selectMaterial;

    /// <summary>
    /// 盤面オブジェクト
    /// </summary>
    private Renderer[,] _boardRenderers;
    /// <summary>
    /// 各マスの石
    /// </summary>
    private GameObject[,] _stones;
    /// <summary>
    /// 各マス目の情報
    /// </summary>
    private MassData[,] _massData;
    /// <summary>
    /// 置ける位置を保持
    /// ・手番が切り替わるごとにクリアを行う
    /// </summary>
    private readonly List<CanPutBoardPositions> _canPutBoardPositions = new List<CanPutBoardPositions>();
    /// <summary>
    /// 今まで打った手の保持
    /// </summary>
    private readonly List<CanPutBoardPositions> _putBoardHistory = new List<CanPutBoardPositions>();
    
    // 周辺8マスの移動方向
    private readonly int[,] _surroundings =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 },
    };
    private readonly int _rows = 8;
    private readonly int _columns = 8;
    
    
    // TODO：持ち時間
    // TODO：AIの実装
    

    private void Awake()
    {
        _boardRenderers = new Renderer[_rows, _columns];
        _stones = new GameObject[_rows, _columns];
        _massData = new MassData[_rows, _columns];
        
        BoardInitialization();
        StoneInitialization();
        
        CanPutBoardUpdate();
    }

    /// <summary>
    /// 盤面の初期化
    /// </summary>
    private void BoardInitialization()
    {
        var alphabet = 'a';

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                // 盤面の取得
                var board = _boardObjects[i].transform.GetChild(j).gameObject;
                if(board.TryGetComponent<Renderer>(out var render)) _boardRenderers[i, j] = render;
                
                // 棋譜
                var num = (i + 1).ToString();
                var alphabetNum = alphabet.ToString();
                var record = alphabetNum + num;
                
                // 必要なデータの生成
                var massData = new MassData(StoneColor.None, record);
                _massData[i, j] = massData;
                
                alphabet++;
            }
            
            alphabet = 'a';
        }
    }

    /// <summary>
    /// 石の初期化
    /// </summary>
    private void StoneInitialization()
    {
        // オセロの初期カラーを設定する
        _massData[3,3].StoneColor = StoneColor.White;
        _massData[3,4].StoneColor = StoneColor.Black;
        _massData[4,3].StoneColor = StoneColor.Black;
        _massData[4,4].StoneColor = StoneColor.White;
        
        // 石の生成
        StoneGenerate(3, 3, StoneColor.White);
        StoneGenerate(3, 4, StoneColor.Black);
        StoneGenerate(4, 3, StoneColor.Black);
        StoneGenerate(4, 4, StoneColor.White);
    }
    
    /// <summary>
    /// 指定された位置に石を生成する
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <param name="color">色</param>
    private void StoneGenerate(int row, int column, StoneColor color)
    {
        var stone = Instantiate(_stonePrefab, _stoneParent);
        _stones[row, column] = stone;
                
        var pos = _stoneTransforms[row]._transforms[column].position;
        pos.y += _stoneOffset;
        stone.transform.position = pos;
        
        // 白の場合、石を回転させて反転させる
        if(color == StoneColor.White) stone.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
    }

    /// <summary>
    /// 範囲内か判定する
    /// </summary>
    /// <param name="currentRow"></param>
    /// <param name="currentColumn"></param>
    /// <returns>true：範囲内　false：範囲外</returns>
    public bool IsWithinRange(int currentRow, int currentColumn)
    {
        if(currentRow < 0 || currentRow >= _rows || currentColumn < 0 || currentColumn >= _columns) return false;

        return true;
    }

    /// <summary>
    /// 指定された位置が置けるかどうかを返す
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <returns>true：置ける　false：置けない</returns>
    private bool IsCanPutPosition(int row, int column)
    {
        foreach (var canPut in _canPutBoardPositions)
        {
            if (canPut.PutPosition.row == row && canPut.PutPosition.column == column) return true;
        }

        return false;
    }

    /// <summary>
    /// 指定された位置のマスの色を選択中に変更する
    /// </summary>
    /// <param name="currentRow"></param>
    /// <param name="currentColumn"></param>
    public void SelectBoardColor(int currentRow, int currentColumn)
    {
        _boardRenderers[currentRow, currentColumn].material = _selectMaterial;
    }

    /// <summary>
    /// 指定された位置のマスの色を元に戻す
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void DeselectBoardColor(int row, int column)
    {
        // 置ける位置なのかを判定し、置ける位置なら色を置ける位置の色に変更
        _boardRenderers[row, column].material = IsCanPutPosition(row, column) ? _canPutMaterial : _normalMaterial;
    }
    
    /// <summary>
    /// 石を置く
    /// </summary>
    /// <param name="row">置く石の行</param>
    /// <param name="column">置く石の列</param>
    public bool PutStone(int row, int column)
    {
        // 現在の手番で置けない場合、処理をしない
        if(!IsCanPutPosition(row, column)) return false;
        
        // 盤面の石の色を現在の手番の石の色に変更
        _massData[row, column].StoneColor = _gameTurnManager.CurrentTurnStoneColor;
        _boardRenderers[row, column].material = _normalMaterial;
        
        // 石の生成を行う
        var stone = Instantiate(_stonePrefab, _stoneParent);
        _stones[row, column] = stone;
        // 石の位置設定
        var pos = _stoneTransforms[row]._transforms[column].position;
        pos.y += _stoneOffset;
        stone.transform.position = pos;
            
        // 白の場合、石を回転させて反転させる
        if(_gameTurnManager.CurrentTurnStoneColor == StoneColor.White) 
            stone.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
        
        // 挟まれた石をめくる
        FlipStone(row, column);

        return true;
    }

    /// <summary>
    /// 置いた石を元に戻す
    /// </summary>
    /// <param name="row">元に戻す石の行</param>
    /// <param name="column">元に戻す石の列</param>
    public void UndoPutStone(int row, int column)
    {
        // 元に戻す石の情報等を削除する
        _massData[row, column].StoneColor = StoneColor.None;
        Destroy(_stones[row, column]);
        _stones[row, column] = null;
        
        // 最後に行った手
        var last = _putBoardHistory[^1];
        // 前の手番の石
        var previousColor = _gameTurnManager.CurrentTurnStoneColor == StoneColor.Black ?
            StoneColor.White : StoneColor.Black;

        // めくった石の情報も削除する
        foreach (var position in last.FlipPositions)
        {
            var flipRow = position.row;
            var flipCol = position.column;
            // 石の情報等を元に戻す
            _massData[flipRow, flipCol].StoneColor = previousColor;
            _stones[flipRow, flipCol].transform.rotation = 
                Quaternion.Euler(previousColor == StoneColor.Black ? new Vector3(0, 0, 0) : new Vector3(180, 0, 0));
        }
        
        _putBoardHistory.RemoveAt(_putBoardHistory.Count - 1);
    }

    /// <summary>
    /// 指定された位置に置いたことで、挟まれた石をめくる
    /// </summary>
    /// <param name="row">置いた石の行</param>
    /// <param name="column">置いた石の列</param>
    private void FlipStone(int row, int column)
    {
        var putData = new CanPutBoardPositions((row, column));
        foreach (var canPut in _canPutBoardPositions)
        {
            // 置いた石の位置と一致しない場合、処理をスキップ
            if(canPut.PutPosition.row != row || canPut.PutPosition.column != column) continue;

            // めくることができる石を全て取得し、めくる
            foreach (var position in canPut.FlipPositions)
            {
                var flipRow = position.row;
                var flipCol = position.column;
                // 履歴を保持
                putData.AddFlipPosition(flipRow, flipCol);
                
                // 盤面の情報を変更
                _massData[flipRow, flipCol].StoneColor = _gameTurnManager.CurrentTurnStoneColor;
                // 表示されている石の向きを変更
                _stones[flipRow, flipCol].transform.rotation = 
                    Quaternion.Euler(_gameTurnManager.CurrentTurnStoneColor == StoneColor.Black ? new Vector3(0, 0, 0) : new Vector3(180, 0, 0));
            }
            
            _putBoardHistory.Add(putData);
            return;
        }
    }

    /// <summary>
    /// 現在の手番の石を置くことができるマスと、めくれる石の更新
    /// </summary>
    public void CanPutBoardUpdate()
    {
        _canPutBoardPositions.Clear();

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                // 置かれている場合、処理を行わない
                if (_massData[i, j].StoneColor != StoneColor.None) continue;

                // 手番の石を置いたとき、めくれる石を取得
                var flipPositions = CheckSurroundingStone(i, j, _gameTurnManager.CurrentTurnStoneColor);
                if (flipPositions.Count > 0) 
                {
                    // 置ける位置とめくれる石位置の石を保持
                    var canPut = new CanPutBoardPositions((i, j));
                    foreach (var flipPosition in flipPositions)
                    {
                        canPut.AddFlipPosition(flipPosition.row, flipPosition.col);
                    }
                    
                    _canPutBoardPositions.Add(canPut);
                    _boardRenderers[i, j].material = _canPutMaterial;
                }
                else
                {
                    _boardRenderers[i, j].material = _normalMaterial;
                }
            }
        }
    }

    /// <summary>
    /// 周辺８方向で手番のじゃない石を挟めるか調べる
    /// </summary>
    /// <param name="row">石を置く行</param>
    /// <param name="column">石を置く列</param>
    /// <param name="currentStoneColor">手番の石の色</param>
    /// <returns>true：置くことができる　false：置くことができない</returns>
    private List<(int row, int col)> CheckSurroundingStone(int row, int column, StoneColor currentStoneColor)
    {
        var flipPositions = new List<(int, int)>();

        for (int i = 0; i < _surroundings.GetLength(0); i++)
        {
            // 移動方向
            var dirX = row + _surroundings[i, 0];
            var dirY = column + _surroundings[i, 1];

            // 範囲外
            if(!IsWithinRange(dirX, dirY)) continue;
            // 石が置かれていない
            if(_massData[dirX, dirY].StoneColor == StoneColor.None) continue;
            // 調べた石と同じ色
            if(_massData[dirX, dirY].StoneColor == currentStoneColor) continue;

            // めくれる石を取得
            var pos = ContinuedStone(dirX, dirY, _surroundings[i, 0], _surroundings[i, 1], currentStoneColor);
            flipPositions.AddRange(pos);
        }

        return flipPositions;
    }

    /// <summary>
    /// 移動先に手番の石の色が続いているのかを調べる
    /// </summary>
    /// <param name="searchX">調べる石のXの位置</param>
    /// <param name="searchY">調べる石のYの位置</param>
    /// <param name="dirX">Xの移動方向</param>
    /// <param name="dirY">Yの移動方向</param>
    /// <param name="color">手番の石の色</param>
    /// <returns>true：続いている　false：続いていない</returns>
    private List<(int row, int col)> ContinuedStone(int searchX, int searchY, int dirX, int dirY, StoneColor color)
    {
        var check = new List<(int, int)>();

        var x = searchX;
        var y = searchY;

        while (true)
        {
            // 現在のマス
            var current = _massData[x, y];

            // 置いた石に到達
            if (current.StoneColor == color) return check;

            // 何も置かれていない
            if(current.StoneColor == StoneColor.None) return new List<(int row, int col)>();

            // 相手の石を追加
            check.Add((x, y));

            // 次の移動先
            x += dirX;
            y += dirY;

            // 範囲外
            if(!IsWithinRange(x, y)) return new List<(int row, int col)>();
        }
    }
}

/// <summary>
/// 盤面の位置
/// </summary>
[Serializable]
public class BoardTransform
{
    public Transform[] _transforms;
}

/// <summary>
/// 置ける位置の情報を保持
/// </summary>
public class CanPutBoardPositions
{
    /// <summary>
    /// 置ける位置
    /// </summary>
    public (int row, int column) PutPosition { get; private set; }
    /// <summary>
    /// めくることができる位置
    /// </summary>
    public List<(int row, int column)> FlipPositions { get; private set; }

    public CanPutBoardPositions((int row, int column) putPositions)
    {
        PutPosition = putPositions;
        FlipPositions = new List<(int row, int column)>();
    }
    
    /// <summary>
    /// めくる位置を追加する
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    public void AddFlipPosition(int row, int column)
    {
        FlipPositions.Add((row, column));
    }
}