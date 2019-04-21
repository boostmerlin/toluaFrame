using UnityEngine;

public class UIAnimation
{
    readonly int SHOW;
    readonly int HIDE;
    public interface UIAnimatorLitener
    {
        void OnShowed();
        void OnHided();
    }
    Animator m_animator;
    public UIAnimation(Animator animator)
    {
        m_animator = animator;
        SHOW = Animator.StringToHash("Show");
        HIDE = Animator.StringToHash("Hide");
    }

    void setPlay(bool isShow)
    {
        if(!m_animator.enabled)
        {
            m_animator.enabled = true;
        }
        //m_animator.Play(isShow ? SHOW : HIDE);
        m_animator.SetBool("IsShow", isShow);
        m_animator.SetTrigger("Play");
    }

    public void SetAnimatorEnable(bool enable)
    {
        if (m_animator)
        {
            m_animator.enabled = enable;
        }
    }

    public bool HasAnimation()
    {
        return m_animator != null && m_animator.runtimeAnimatorController != null;
    }

    public bool HasShowAnimation()
    {
        return HasAnimation() 
            && m_animator.HasState(0, SHOW) 
            && !m_animator.runtimeAnimatorController.animationClips[0].empty;
    }

    public bool HasHideAnimation()
    {
        return HasAnimation()
            && m_animator.HasState(0, HIDE)
            && !m_animator.runtimeAnimatorController.animationClips[1].empty;
    }

    public bool IsPlaying()
    {
        if (!m_animator.isInitialized)
        {
            return false;
        }
        AnimatorStateInfo currentState = m_animator.GetCurrentAnimatorStateInfo(0);
        return (currentState.IsName("Show") || currentState.IsName("Hide"))
            && currentState.normalizedTime < 1;
    }

    public void Show()
    {
        if (!m_animator.gameObject.activeSelf)
        {
            m_animator.gameObject.SetActive(true);
        }
        setPlay(true);
    }

    public void Hide()
    {
        setPlay(false);
    }
}
