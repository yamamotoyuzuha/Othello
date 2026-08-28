using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盤面を管理する
/// </summary>
public class BoardManager : MonoBehaviour
{
    [SerializeField] private BoardExploration _boardExploration;
    [Header("石の生成位置（Parent）"), SerializeField] private Transform _stoneParent;
    [Header("石"), SerializeField] private GameObject _stonePrefab;
    [Header("石生成時のOffset"), SerializeField] private float _stoneOffset = 0.1f;
    [Header("盤面位置"), SerializeField] private List<BoardTransform> _stoneTransforms;

    private Material[,] _boardMaterial;
    /// <summary>
    /// 全マスの石
    /// </summary>
    private GameObject[,] _stones;
    /// <summary>
    /// 全マス目の情報
    /// </summary>
    private MassData[,] _massData;
    
    private readonly int _rows = 8;
    private readonly int _columns = 8;

    private void Awake()
    {
        _stones = new GameObject[_rows, _columns];
        _massData = new MassData[_rows, _columns];
        
        BoardInitialization();
        StoneInitialization();
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
                // 棋譜
                var num = (i + 1).ToString();
                var alphabetNum = alphabet.ToString();
                var record = alphabetNum + num;

                // 必要なデータの生成
                var massData = new MassData(StoneColor.None, record);
                _massData[i, j] = massData;
            }
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
}

/// <summary>
/// 盤面の位置
/// </summary>
[Serializable]
public class BoardTransform
{
    public Transform[] _transforms;
}