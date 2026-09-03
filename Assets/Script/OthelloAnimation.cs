using UnityEngine;

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

    public void SetIdle(StoneColor stoneColor)
    {
        if (stoneColor == StoneColor.Black)
        {
            _animator.SetFloat(_blendTreeName, 0);
        }
        else if (stoneColor == StoneColor.White)
        {
            _animator.SetFloat(_blendTreeName, 1);
        }
    }

    public void AnimationPlay(StoneColor stoneColor)
    {
        if (stoneColor == StoneColor.Black)
        {
            _animator.SetTrigger(_triggerNameBlack);
        }
        else if (stoneColor == StoneColor.White)
        {
            _animator.SetTrigger(_triggerNameWhite);
        }
    }
}
