using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipperyStatusEffect : StatusEffect
{
    public float slideDistance = 20f;
    public float slideDuration = 1f;

    public override void OnApplication(CharacterState state)
    {
        base.OnApplication(state);

        if (state.playerController != null)
        {
            state.playerController.slideDistance = slideDistance;
            state.playerController.slideDuration = slideDuration;
            state.playerController.sliding = true;
        } 
        else if (state.aiController != null)
        {
            state.aiController.slideDistance = slideDistance;
            state.aiController.slideDuration = slideDuration;
            state.aiController.sliding = true;
        }
    }

    public override void OnExpire(CharacterState state)
    {
        base.OnExpire(state);
        if (state.playerController != null)
            state.playerController.sliding = false;
        else if (state.aiController != null)
            state.aiController.sliding = false;
    }

    public override void OnCleanse(CharacterState state)
    {
        base.OnCleanse(state);
        if (state.playerController != null)
            state.playerController.sliding = false;
        else if (state.aiController != null)
            state.aiController.sliding = false;
    }
}
