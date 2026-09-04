using System;
using System.Collections;
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
    /// 石のアニメーション
    /// </summary>
    private OthelloAnimation[,] _othelloAnimations;
    /// <summary>
    /// 各マス目の情報
    /// </summary>
    private MassData[,] _massData;
    /// <summary>
    /// 置ける位置を保持
    /// ・手番が切り替わるごとにクリアを行う
    /// </summary>
    private readonly List<CanPutBoardPositions> _canPutBoardPositions = new();
    /// <summary>
    /// 今まで打った手の保持
    /// ・パスの場合、nullで登録する
    /// </summary>
    private readonly List<CanPutBoardPositions> _putBoardHistory = new();
    /// <summary>
    /// 周辺8マスの移動方向
    /// </summary>
    private readonly int[,] _surroundings =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 },
    };
    private readonly int _rows = 8;
    private readonly int _columns = 8;
    /// <summary>
    /// AIの待機時間
    /// </summary>
    private readonly float _aiThinkingTime = 2f;
    /// <summary>
    /// パスした回数
    /// </summary>
    private int _passCount;
    /// <summary>
    /// 対戦中のAI
    /// </summary>
    private AI _ai;
    /// <summary>
    /// AIを実行したか
    /// true：実行中　false：実行中でない
    /// </summary>
    private bool _isAIThinking;
    #region 現在の選択中のマス
    private int _currentRow;
    private int _currentColumn;
    #endregion

    private void Awake()
    {
        _ai = new NegaMaxAI(this);
    }

    private void Start()
    {
        _boardRenderers = new Renderer[_rows, _columns];
        _stones = new GameObject[_rows, _columns];
        _othelloAnimations = new  OthelloAnimation[_rows, _columns];
        _massData = new MassData[_rows, _columns];
        
        BoardInitialization();
        StoneInitialization();
        CanPutBoardUpdate();
    }

    private void Update()
    {
        if(GameManager.Instance.IsGameEnd || GameManager.Instance.IsRecord) return;
        if (_gameTurnManager.CurrentTurnStoneColor == StoneColor.White && GameManager.Instance.IsUseAI && !_isAIThinking)
        {
            StartCoroutine(AI());
        }
    }

    /// <summary>
    /// AI
    /// </summary>
    IEnumerator　AI()
    {
        _isAIThinking = true;
        
        yield return new WaitForSeconds(_aiThinkingTime);
        
        // AIの実行
        _ai.ThinkingAI(_massData, _gameTurnManager.CurrentTurnStoneColor);
        // 手番の変更と盤面更新
        _gameTurnManager.ChangeCurrentTurnStoneColor();
        CanPutBoardUpdate();
        SelectBoardColor(_currentRow, _currentColumn);

        _isAIThinking = false;
    }

    /// <summary>
    /// 盤面の初期化
    /// </summary>
    private void BoardInitialization()
    {
        var alphabet = 'A';

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
            
            alphabet = 'A';
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

        var black = 2;
        var white = 2;
        ScoreBoardUIManager.Instance.SetStoneCount(black, white);
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
        
        // アニメーション
        _othelloAnimations[row, column] = stone.GetComponent<OthelloAnimation>();
        _othelloAnimations[row, column].SetIdle(color);
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
    /// 盤面が全て埋まってるか判定する
    /// </summary>
    /// <returns>true：埋まっている　false：埋まっていない</returns>
    private bool IsAllFullStone()
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (_massData[i, j].StoneColor == StoneColor.None) return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// 石の個数を更新
    /// </summary>
    private void UpdateStoneCount()
    {
        var black = 0;
        var white = 0;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (_massData[i, j].StoneColor == StoneColor.Black) black++;
                else if (_massData[i, j].StoneColor == StoneColor.White) white++;
            }
        }
        
        ScoreBoardUIManager.Instance.SetStoneCount(black, white);
    }

    /// <summary>
    /// 指定された位置のマスの色を選択中に変更する
    /// </summary>
    /// <param name="currentRow"></param>
    /// <param name="currentColumn"></param>
    public void SelectBoardColor(int currentRow, int currentColumn)
    {
        _boardRenderers[currentRow, currentColumn].material = _selectMaterial;
        _currentRow = currentRow;
        _currentColumn = currentColumn;
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
    /// <returns>true：置ける　false：置けない</returns>
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
        // アニメーション
        _othelloAnimations[row, column] = stone.GetComponent<OthelloAnimation>();
        _othelloAnimations[row, column].SetIdle(_gameTurnManager.CurrentTurnStoneColor);
        // 石の位置設定
        var pos = _stoneTransforms[row]._transforms[column].position;
        pos.y += _stoneOffset;
        stone.transform.position = pos;
        
        // 挟まれた石をめくる
        FlipStone(row, column);
        // 石の個数を更新
        UpdateStoneCount();
        
        // 途中勝利の判定を行う
        if (IsMidwayVictory())
        {
            Debug.LogWarning($"途中勝利した手番：{_gameTurnManager.CurrentTurnStoneColor}");
        }

        return true;
    }

    /// <summary>
    /// 置いた石を元に戻す
    /// </summary>
    public void UndoPutStone()
    {
        if(_putBoardHistory.Count == 0) return;

        // パスの場合
        if (_putBoardHistory[^1] == null)
        {
            _putBoardHistory.RemoveAt(_putBoardHistory.Count - 1);
            if (_passCount > 0) _passCount--;

            // パスの前の手番に戻す
            _gameTurnManager.ChangeCurrentTurnStoneColor();
        }
        
        // 置いた石を取り消し
        var lastIndex = _putBoardHistory.Count - 1;
        var last = _putBoardHistory[lastIndex];
        
        var row = last.PutPosition.row;
        var column = last.PutPosition.column;
        
        // 元に戻す石の情報等を削除する
        _massData[row, column].StoneColor = StoneColor.None;
        Destroy(_stones[row, column]);
        _stones[row, column] = null;
        
        _gameTurnManager.ChangeCurrentTurnStoneColor();
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
            // アニメーション
            _othelloAnimations[flipRow, flipCol].AnimationPlay(previousColor);
        }
        
        _putBoardHistory.RemoveAt(lastIndex);
        
        CanPutBoardUpdate(false);
        UpdateStoneCount();
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
                // アニメーション
                _othelloAnimations[flipRow, flipCol].AnimationPlay(_gameTurnManager.CurrentTurnStoneColor);
            }
            
            _putBoardHistory.Add(putData);
            return;
        }
    }

    /// <summary>
    /// 現在の手番の石を置くことができるマスと、めくれる石の更新
    /// </summary>
    /// <param name="isPass">true：パス判定を行う　false：行わない</param>
    public void CanPutBoardUpdate(bool isPass = true)
    {
        while (true)
        {
            _canPutBoardPositions.Clear();

            // 盤面が全て埋まっているか、黒と白のどちらかが全滅していたら終了
            if (IsAllFullStone() || IsMidwayVictory())
            {
                CheckGameResult();
                return;
            }

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _boardRenderers[i, j].material = _normalMaterial;
                    
                    if(_massData[i, j].StoneColor != StoneColor.None) continue;

                    // 現在の手番で置ける位置を全て取得する
                    var flipPositions = CheckSurroundingStone(_massData, i, j, _gameTurnManager.CurrentTurnStoneColor);
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
                }
            }

            // 置ける場所があれば、処理を終了
            if (_canPutBoardPositions.Count > 0)
            {
                _passCount = 0;
                return;
            }
            
            // パスの判定を行わない
            if(!isPass) return;
            
            // パスの処理
            _passCount++;
            _putBoardHistory.Add(null);
            Debug.LogWarning($"パスを行いました。{_gameTurnManager.CurrentTurnStoneColor}はパス。{_passCount}");

            if (_passCount >= 2)
            {
                Debug.LogWarning("パスが２回行われたので、ゲームを終了");
                CheckGameResult();
                return;
            }
            
            // 手番を交代
            _gameTurnManager.ChangeCurrentTurnStoneColor();
        }
    }

    /// <summary>
    /// 周辺８方向で手番のじゃない石を挟めるか調べる
    /// </summary>
    /// <param name="massData">調べる盤面</param>
    /// <param name="row">石を置く行</param>
    /// <param name="column">石を置く列</param>
    /// <param name="currentStoneColor">手番の石の色</param>
    /// <returns>true：置くことができる　false：置くことができない</returns>
    private List<(int row, int col)> CheckSurroundingStone(MassData[,] massData, int row, int column, StoneColor currentStoneColor)
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
            if(massData[dirX, dirY].StoneColor == StoneColor.None) continue;
            // 調べた石と同じ色
            if(massData[dirX, dirY].StoneColor == currentStoneColor) continue;

            // めくれる石を取得
            var pos = ContinuedStone(massData, dirX, dirY, _surroundings[i, 0], _surroundings[i, 1], currentStoneColor);
            flipPositions.AddRange(pos);
        }

        return flipPositions;
    }

    /// <summary>
    /// 移動先に手番の石の色が続いているのかを調べる
    /// </summary>
    /// <param name="massData">調べる盤面</param>
    /// <param name="searchX">調べる石のXの位置</param>
    /// <param name="searchY">調べる石のYの位置</param>
    /// <param name="dirX">Xの移動方向</param>
    /// <param name="dirY">Yの移動方向</param>
    /// <param name="color">手番の石の色</param>
    /// <returns>true：続いている　false：続いていない</returns>
    private List<(int row, int col)> ContinuedStone(MassData[,] massData, int searchX, int searchY, int dirX, int dirY, StoneColor color)
    {
        var check = new List<(int, int)>();

        var x = searchX;
        var y = searchY;

        while (true)
        {
            // 現在のマス
            var current = massData[x, y];

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

    /// <summary>
    /// ゲーム終了時の勝敗結果を判定する
    /// </summary>
    private void CheckGameResult()
    {
        var black = 0;
        var white = 0;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if(_massData[i, j].StoneColor == StoneColor.Black) black++;
                else if(_massData[i, j].StoneColor == StoneColor.White) white++;
            }
        }

        if (black > white) // 黒の勝利
        {
            Debug.LogWarning("黒の勝利");
            GameManager.Instance.GameEnd(StoneColor.Black);
        }
        else if (white > black) // 白の勝利
        {
            Debug.LogWarning("白の勝利");
            GameManager.Instance.GameEnd(StoneColor.White);
        }
        
        // 引き分け
        GameManager.Instance.GameEnd(StoneColor.None);
    }

    /// <summary>
    /// 途中勝利が発生しているか判定し、勝者を決める
    /// </summary>
    /// <returns>true：途中勝利　false：途中勝利ではない</returns>
    private bool IsMidwayVictory()
    {
        var black = false;
        var white = false;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (_massData[i, j].StoneColor == StoneColor.Black) black = true;
                else if (_massData[i, j].StoneColor == StoneColor.White) white = true;
            }
        }

        // 両方の石が存在している
        if (black && white) return false;

        if (black) // 黒の勝利
        {
            GameManager.Instance.GameEnd(StoneColor.Black);
            return true;
        }

        if (white) // 白の勝利
        {
            GameManager.Instance.GameEnd(StoneColor.White);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 置くことができる場所を取得する
    /// </summary>
    /// <param name="massData">判定を行う盤面</param>
    /// <param name="stoneColor">判定を行う石の色</param>
    /// <returns>置くことができる場所</returns>
    public List<CanPutBoardPositions> GetCanPutBoardPositions(MassData[,] massData, StoneColor stoneColor)
    {
        var canPutBoardPositions = new List<CanPutBoardPositions>();

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                // 置かれている場合、処理を行わない
                if (massData[i, j].StoneColor != StoneColor.None) continue;

                // 手番の石を置いたとき、めくれる石を取得
                var flipPositions = CheckSurroundingStone(massData, i, j, stoneColor);
                if(flipPositions.Count == 0) continue;
                
                var canPut = new CanPutBoardPositions((i, j));
                foreach (var flipPosition in flipPositions)
                {
                    canPut.AddFlipPosition(flipPosition.row, flipPosition.col);
                }
                canPutBoardPositions.Add(canPut);
            }
        }
        
        return canPutBoardPositions;
    }

    /// <summary>
    /// 盤面のコピーを取得する
    /// </summary>
    /// <returns>盤面のコピーデータ</returns>
    public MassData[,] CopyBoard(MassData[,] massData)
    {
        var copy = new MassData[_rows, _columns];

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                var original = massData[i, j];
                copy[i, j] = new MassData(original.StoneColor, original.Record);
            }
        }
        
        return copy;
    }

    /// <summary>
    /// 石を仮で置く
    /// </summary>
    /// <param name="massData">置く盤面</param>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <param name="stoneColor">置く石の色</param>
    /// <returns>置いた後の盤面</returns>
    public MassData[,] PutStoneTemporarily(MassData[,] massData, int row, int column, StoneColor stoneColor)
    {
        // 石を置く
        massData[row, column].StoneColor = stoneColor;
        // 石をめくる
        var flipPositions = CheckSurroundingStone(massData, row, column, stoneColor);
        foreach (var flipPosition in flipPositions)
        {
            massData[flipPosition.row, flipPosition.col].StoneColor = stoneColor;
        }
        
        return massData;
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