using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;
using static GlobalStructs.Damage;

public class SpawnDamageTriggerMechanic : FightMechanic
{
    public GameObject damageTriggerPrefab;
    public Transform spawnLocation;
    public float delay = 0f;
    public bool useActionDamage = true;
    public bool usePlayerHealth = false;
    public bool dealDamage = true;
    public float damageMultiplier = 1f;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (spawnLocation == null)
        {
            if (actionInfo.target != null)
            {
                GameObject spawned = Instantiate(damageTriggerPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);

                SetupDamageTrigger(spawned, actionInfo);
            }
            else if (actionInfo.source != null && actionInfo.action != null)
            {
                GameObject spawned = Instantiate(damageTriggerPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);

                SetupDamageTrigger(spawned, actionInfo);
            }
        }
        else
        {
            GameObject spawned = Instantiate(damageTriggerPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);

            SetupDamageTrigger(spawned, actionInfo);
        }
    }

    public void SetupDamageTrigger(GameObject spawned, ActionInfo actionInfo)
    {
        if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
        {
            if (delay > 0)
            {
                spawned.gameObject.SetActive(false);
                Utilities.FunctionTimer.Create(() =>
                {
                    spawned.gameObject.SetActive(true);
                    if (!damageTrigger.initializeOnStart)
                        damageTrigger.Initialize();
                }, delay, $"{damageTrigger}_{damageTrigger.GetHashCode()}_{mechanicName.Replace(" ", "")}_Activation_Delay", false, true);
            }
            else if (!damageTrigger.initializeOnStart)
            {
                damageTrigger.Initialize();
            }

            if (usePlayerHealth)
            {
                damageTrigger.isAShield = actionInfo.action.data.isAShield;
                damageTrigger.self = actionInfo.sourceIsPlayer;
                damageTrigger.damage = new Damage(Mathf.RoundToInt(damageMultiplier * actionInfo.source.health), false, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, "White Wind");
            }
            else if (useActionDamage)
            {
                damageTrigger.isAShield = actionInfo.action.data.isAShield;
                damageTrigger.self = actionInfo.sourceIsPlayer;
                damageTrigger.damage = new Damage(actionInfo.action.data.damage, Mathf.RoundToInt(actionInfo.action.data.damage.value * damageMultiplier));
            }
        }
    }
}
