using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCharacter : MonoBehaviour
{
    public CanvasGroup screenFade;
    public Vector3 location = new Vector3(0f, 1f, 0f);

    public void StartReset(CharacterState state)
    {
        screenFade.alpha = 0f;
        if (state.characterName == "Player")
            StartCoroutine(PerformReset(state.transform, true));
        else
            StartCoroutine(PerformReset(state.transform, false));
    }

    private IEnumerator PerformReset(Transform player, bool fade)
    {
        yield return new WaitForSeconds(0.5f);
        if (fade)
            screenFade.LeanAlpha(1f, 1f);
        yield return new WaitForSeconds(1f);
        player.transform.position = location;
        yield return new WaitForSeconds(0.5f);
        if (fade)
            screenFade.LeanAlpha(0f, 2f);
    }
}
