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
    
    // 周辺8マスの移動方向
    private readonly int[,] _surroundings =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 },
    };
    private readonly int _rows = 8;
    private readonly int _columns = 8;
    
    
    // TODO：置く処理（めくる）
    // TODO：手番
    // TODO：持ち時間
    // TODO：AIの実装
    

    private void Awake()
    {
        _boardRenderers = new Renderer[_rows, _columns];
        _stones = new GameObject[_rows, _columns];
        _massData = new MassData[_rows, _columns];
        
        BoardInitialization();
        StoneInitialization();
        
        CanPutBoard();
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
    /// 指定された座標に石を生成する
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
    /// 指定された座標のマスの色を選択中に変更する
    /// </summary>
    /// <param name="currentRow"></param>
    /// <param name="currentColumn"></param>
    public void SelectBoardColor(int currentRow, int currentColumn)
    {
        _boardRenderers[currentRow, currentColumn].material = _selectMaterial;
    }

    /// <summary>
    /// 指定された座標のマスの色を元に戻す
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void DeselectBoardColor(int row, int column)
    {
        var cunPut = CheckSurroundingStone(row, column, StoneColor.Black);
        if (cunPut) // 置ける
        {
            _boardRenderers[row, column].material = _canPutMaterial;
        }
        else
        {
            _boardRenderers[row, column].material = _normalMaterial;
        }
    }

    // TODO：置く処理を実装する
    /// <summary>
    /// 置く
    /// </summary>
    public void PutStone()
    {
        
    }
    
    /// <summary>
    /// 指定された石の色を置くことができるマス目を取得する
    /// </summary>
    private void CanPutBoard()
    {
        // 置くことが可能なマスを保持する
        List<MassData> emptyList = new List<MassData>();

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                // まだ置かれていないマスを判定
                if(_massData[i, j].StoneColor == StoneColor.None)
                    emptyList.Add(_massData[i, j]);
            }
        }
        
        if(emptyList.Count == 0) return;

        // 置くことができるかを判定
        foreach (var empty in emptyList)
        {
            var record = empty.Record;
            var row = record[0] - 'a';
            var col = record[1] - '1';
            
            var cunPut = CheckSurroundingStone(row, col, StoneColor.Black);
            if (cunPut) // 置ける
            {
                _boardRenderers[row, col].material = _canPutMaterial;
            }
        }
    }

    /// <summary>
    /// 周辺８マスから置くことができる
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <param name="currentStoneColor">手番の石の色</param>
    /// <returns>true：置くことができる　false：置くことができない</returns>
    private bool CheckSurroundingStone(int row, int column, StoneColor currentStoneColor)
    {
        for (int i = 0; i < _surroundings.GetLength(0); i++)
        {
            // 移動方向
            var dirX = row + _surroundings[i, 0];
            var dirY = column + _surroundings[i, 1];
            
            // 範囲外
            if(dirX < 0 || dirX >= _rows || dirY < 0 || dirY >= _columns) continue;
            // 石が置かれていない
            if(_massData[dirX, dirY].StoneColor == StoneColor.None) continue;
            // 調べた石と同じ色
            if(_massData[dirX, dirY].StoneColor == currentStoneColor) continue;
            
            // 相手の石が続いているのを判定する
            if(ContinuedStone(dirX, dirY, _surroundings[i, 0], _surroundings[i, 1], currentStoneColor)) return true;
        }
        
        return false;
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
    private bool ContinuedStone(int searchX, int searchY, int dirX, int dirY, StoneColor color)
    {
        var check = new List<MassData>();
        
        var x = searchX;
        var y = searchY;

        while (true)
        {
            // 現在のマス
            var current = _massData[x, y];

            // 置いた石に到達
            if (current.StoneColor == color)
            {
                // 1個でもあれば石の色を変更
                if (check.Count > 0) return true;

                return false;
            }
                
            // 何も置かれていない
            if(current.StoneColor == StoneColor.None) return false;
                
            // 相手の石を追加
            check.Add(current);

            // 次の移動先
            x += dirX;
            y += dirY;
                
            // 範囲外
            if(x < 0 || x >= _rows || y < 0 || y >= _columns) return false;
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