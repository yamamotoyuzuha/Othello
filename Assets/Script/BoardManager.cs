using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盤面を管理する
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("縦のサイズ"), SerializeField] private int _vertical = 8;
    [Header("横のサイズ"), SerializeField] private int _horizontal = 8;
    [Header("石の生成位置（Parent）"), SerializeField] private Transform _stoneParent;
    [Header("石"), SerializeField] private GameObject _stonePrefab;
    [Header("石生成時のOffset"), SerializeField] private float _stoneOffset = 0.1f;
    [Header("盤面位置"), SerializeField] private List<BoardTransform> _stoneTransforms;
        
    private int[,] _board;
    private BoardData[,] _boardData;
    private Material[,] _boardMaterial;
    private GameObject[,] _stones;

    private void Awake()
    {
        _board = new int[_vertical, _horizontal];
        _stones = new GameObject[_vertical, _horizontal];
        BoardDataReset();
        StoneGenerate();
    }

    /// <summary>
    /// 盤面の初期化
    /// </summary>
    private void BoardDataReset()
    {
        _boardData = new BoardData[_vertical, _horizontal];
        for (int i = 0; i < _vertical; i++)
        {
            for (int j = 0; j < _horizontal; j++)
            {
                _boardData[i, j] = new BoardData(false, ColorType.None);
            }
        }
    }

    /// <summary>
    /// 石の生成
    /// </summary>
    private void StoneGenerate()
    {
        for (int i = 0; i < _vertical; i++)
        {
            for (int j = 0; j < _horizontal; j++)
            {
                var stone = Instantiate(_stonePrefab, _stoneParent);
                _stones[i, j] = stone;
                
                var pos = _stoneTransforms[i]._transforms[j].position;
                pos.y += _stoneOffset;
                stone.transform.position = pos;
            }
        }
    }
}

[Serializable]
public class BoardTransform
{
    public Transform[] _transforms;
}