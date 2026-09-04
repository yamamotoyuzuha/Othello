using UnityEngine;

/// <summary>
/// 石のアニメーション
/// </summary>
public class OthelloAnimation : MonoBehaviour
{
    [Header("黒"), SerializeField] private string _triggerNameBlack;
    [Header("白"), SerializeField] private string _triggerNameWhite;
    [Header("ブレンドツリーの値"), SerializeField] private string _blendTreeName;
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 待機アニメーションを再生
    /// </summary>
    /// <param name="stoneColor">石の色</param>
    public void SetIdle(StoneColor stoneColor)
    {
        if (stoneColor == StoneColor.Black)
        {
            _animator.SetFloat(_blendTreeName, 1);
        }
        else if (stoneColor == StoneColor.White)
        {
            _animator.SetFloat(_blendTreeName, 0);
        }
    }

    /// <summary>
    /// めくるアニメーションを再生
    /// </summary>
    /// <param name="stoneColor">石の色</param>
    public void AnimationPlay(StoneColor stoneColor)
    {
        if (stoneColor == StoneColor.Black)
        {
            _animator.SetTrigger(_triggerNameWhite);
        }
        else if (stoneColor == StoneColor.White)
        {
            _animator.SetTrigger(_triggerNameBlack);
        }
    }
}
