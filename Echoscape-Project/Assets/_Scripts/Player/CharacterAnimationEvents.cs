using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimationEvents : MonoBehaviour
{
    #region Character Events
    [Header("Character")]
    public UnityEvent OnFootstep;

    private void OnFootStep(AnimationEvent animationEvent)
    {
        OnFootstep.Invoke();
    }

    #endregion

    #region Inventory
    [Header("Inventory")]
    public UnityEvent OnBackWeaponReached;
    public UnityEvent OnWeaponSwapFinished;

    private void BackWeaponReached(AnimationEvent animationEvent)
    {
        OnBackWeaponReached.Invoke();
    }

    private void WeaponSwapFinished(AnimationEvent animationEvent)
    {
        OnWeaponSwapFinished.Invoke();
    }
    #endregion
}
