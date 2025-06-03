using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static GlobalData;

public class DisableEnemyMechanic : FightMechanic
{
    public List<CharacterState> enemies = new List<CharacterState>();
    public bool setNameplate = false;
    public bool setTargetable = false;
    [FormerlySerializedAs("inverted")] public bool enableInstead = false;
    public bool destroyInstead = false;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (enemies != null && enemies.Count > 0)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enableInstead)
                {
                    if (setNameplate)
                        enemies[i].ToggleNameplate(false);
                    if (setTargetable)
                        enemies[i].ToggleTargetable(false);

                    if (!destroyInstead)
                    {
                        enemies[i].disabled = true;
                        enemies[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        enemies[i].disabled = true;
                        Destroy(enemies[i].gameObject, 0.1f);
                    }
                }
                else
                {
                    if (setNameplate)
                        enemies[i].ToggleNameplate(true);
                    if (setTargetable)
                        enemies[i].ToggleTargetable(true);

                    enemies[i].disabled = false;
                    enemies[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
