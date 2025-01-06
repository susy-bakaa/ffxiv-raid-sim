using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static ActionController;
using static GlobalData;
using static GlobalData.Damage;

public class SpawnDamageTriggerMechanic : FightMechanic
{
    [Header("Spawn Damage Trigger Settings")]
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
    public bool faceTarget = false;
    [ShowIf("faceTarget")] public Axis axis = new Axis();
    public bool dealDamage = true;
    [ShowIf("dealDamage")] public float damageMultiplier = 1f;
    public bool increaseEnmity = false;

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

            if (log)
                Debug.Log($"Object was spawned at {spawned?.transform.position} using '{spawnLocation?.gameObject.name}' at {spawnLocation?.position} as target");

            SetupDamageTrigger(spawned, actionInfo);
        }
    }

    public void SetupDamageTrigger(GameObject spawned, ActionInfo actionInfo)
    {
        DamageTrigger damageTrigger = null;
        bool found = false;

        if (spawned.TryGetComponent(out damageTrigger))
        {
            found = true;
        }
        else
        {
            foreach (Transform child in spawned.transform)
            {
                if (child.TryGetComponent(out damageTrigger))
                {
                    found = true;
                    break;
                }
            }
        }

        if (damageTrigger != null && found)
        {
            if (owner != null)
                damageTrigger.owner = owner;
            if (actionInfo.action != null && actionInfo.action.data != null)
                damageTrigger.data = actionInfo.action.data;

            if (faceTarget)
            {
                if (actionInfo.target != null)
                {
                    // Calculate the look rotation
                    Vector3 directionToTarget = actionInfo.target.transform.position - spawned.transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

                    // Apply axis locking
                    Vector3 lockedEulerAngles = lookRotation.eulerAngles;

                    // Lock specific axes
                    if (!axis.x)
                        lockedEulerAngles.x = spawned.transform.eulerAngles.x;
                    if (!axis.y)
                        lockedEulerAngles.y = spawned.transform.eulerAngles.y;
                    if (!axis.z)
                        lockedEulerAngles.z = spawned.transform.eulerAngles.z;

                    // Apply the rotation
                    spawned.transform.rotation = Quaternion.Euler(lockedEulerAngles);
                }
                else
                {
                    Debug.LogWarning($"[SpawnDamageTriggerMechanic ({gameObject.name})] has faceTarget set to true, but no available target was found!");
                }
            }

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
