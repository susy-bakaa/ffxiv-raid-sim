using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnemyMechanic : FightMechanic
{
    public List<CharacterState> enemies = new List<CharacterState>();
    public bool inverted = false;
    public bool destroyInstead = false;

    public override void TriggerMechanic(ActionController.ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (enemies != null && enemies.Count > 0)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!inverted)
                {
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
                    enemies[i].disabled = false;
                    enemies[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
