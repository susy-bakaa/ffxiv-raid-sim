using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(HudElement))]
public class StatusEffectIcon : MonoBehaviour
{
    public StatusEffect effect;

    private CharacterState character;
    private EventTrigger eventTrigger;
    private HudElement hudElement;

    public void Initialize(CharacterState character, StatusEffect effect)
    {
        this.character = character;
        this.effect = effect;

        if (character.characterName.ToLower().Contains("player"))
            Setup();
    }

    public void RemoveEffect(BaseEventData e)
    {
        hudElement.OnPointerExit((PointerEventData)e);
        character.RemoveEffect(effect.data, false, character, effect.uniqueTag, effect.stacks);
    }

    private void Setup()
    {
        hudElement = GetComponent<HudElement>();
        eventTrigger = GetComponentInChildren<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { RemoveEffect(eventData); });
        eventTrigger.triggers.Add(entry);
    }
}
