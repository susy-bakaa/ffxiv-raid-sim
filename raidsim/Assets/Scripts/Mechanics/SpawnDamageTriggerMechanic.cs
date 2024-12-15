using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalData;
using static GlobalData.Damage;

public class SpawnDamageTriggerMechanic : FightMechanic
{
    public CharacterState owner;
    public TargetController targetController;
    public GameObject damageTriggerPrefab;
    public bool enableInstead = false;
    public Transform spawnLocation;
    public float delay = 0f;
    public bool autoAssignOwner = false;
    public bool useActionDamage = true;
    public bool usePlayerHealth = false;
    public bool useTargetControllerCurrentTargetAsLocation = false;
    public bool dealDamage = true;
    public bool increaseEnmity = false;
    public float damageMultiplier = 1f;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (autoAssignOwner && owner == null && actionInfo.source != null)
            owner = actionInfo.source;

        if (targetController != null && targetController.currentTarget != null)
        {
            spawnLocation = targetController.currentTarget.transform;
        }

        if (spawnLocation == null)
        {
            if (actionInfo.target != null)
            {
                GameObject spawned;

                if (!enableInstead)
                    spawned = Instantiate(damageTriggerPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);
                else
                    spawned = damageTriggerPrefab;

                SetupDamageTrigger(spawned, actionInfo);
            }
            else if (actionInfo.source != null && actionInfo.action != null)
            {
                GameObject spawned;

                if (!enableInstead)
                    spawned = Instantiate(damageTriggerPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);
                else
                    spawned = damageTriggerPrefab;

                SetupDamageTrigger(spawned, actionInfo);
            }
        }
        else
        {
            GameObject spawned;

            if (!enableInstead)
                spawned = Instantiate(damageTriggerPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);
            else
                spawned = damageTriggerPrefab;

            SetupDamageTrigger(spawned, actionInfo);
        }
    }

    public void SetupDamageTrigger(GameObject spawned, ActionInfo actionInfo)
    {
        if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
        {
            if (owner != null)
                damageTrigger.owner = owner;
            if (actionInfo.action != null && actionInfo.action.data != null)
                damageTrigger.data = actionInfo.action.data;

            if (delay > 0)
            {
                spawned.gameObject.SetActive(false);
                Utilities.FunctionTimer.Create(this, () =>
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

            if (actionInfo.action != null)
            {
                if (usePlayerHealth)
                {
                    damageTrigger.isAShield = actionInfo.action.data.isShield;
                    damageTrigger.self = actionInfo.sourceIsPlayer;
                    damageTrigger.damage = new Damage(Mathf.RoundToInt(damageMultiplier * actionInfo.source.health), false, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, actionInfo.source, "White Wind");
                }
                else if (useActionDamage)
                {
                    damageTrigger.isAShield = actionInfo.action.data.isShield;
                    damageTrigger.self = actionInfo.sourceIsPlayer;
                    damageTrigger.damage = new Damage(actionInfo.action.data.damage, Mathf.RoundToInt(actionInfo.action.data.damage.value * damageMultiplier));

                    if (increaseEnmity)
                    {
                        damageTrigger.increaseEnmity = true;
                        damageTrigger.topEnmity = actionInfo.action.data.topEnmity;
                    }
                }
            }
        }
    }
}
