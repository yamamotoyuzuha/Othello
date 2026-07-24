using UnityEngine;

namespace InGame.Board
{
    /// <summary>
    /// 盤面を管理する
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("縦のサイズ")] 
        [SerializeField] private int _vertical = 8;
        [Header("横のサイズ")]
        [SerializeField] private int _horizontal = 8;
        
        private int[,] _board;
        private BoardData[,] _boardData;
        private Material[,] _boardMaterial;

        private void Awake()
        {
            _board = new int[_horizontal, _vertical];
            BoardDataReset();
        }

        /// <summary>
        /// 盤面の初期化
        /// </summary>
        private void BoardDataReset()
        {
            _boardData = new BoardData[_horizontal, _vertical];
            for (int i = 0; i < _horizontal; i++)
            {
                for (int j = 0; j < _vertical; j++)
                {
                    _boardData[i, j] = new BoardData(false, ColorType.None);
                }
            }
            
            // 最初の配置を行う（中心４つの石を白黒にする）
            
        }
    }
}
