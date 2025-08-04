using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Targeting;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Events
{
    public class SetupEnemy : MonoBehaviour
    {
        public List<TargetNode> enemyTargets = new List<TargetNode>();
        public bool chooseBasedOnRandomEventResult = true;
        public int randomEventResultId = -1;

        public void SetupEnemyTarget(PartyMember enemy)
        {
            if (enemy.targetController != null)
            {
                if (chooseBasedOnRandomEventResult)
                {
                    if (enemyTargets != null && enemyTargets.Count > 0)
                    {
                        int r = FightTimeline.Instance.GetRandomEventResult(randomEventResultId);
                        if (r > -1 && r < enemyTargets.Count)
                        {
                            enemy.targetController.SetTarget(enemyTargets[r]);
                        }
                        else
                        {
                            enemy.targetController.SetTarget(enemyTargets[0]);
                        }
                    }
                }
                else if (enemyTargets != null && enemyTargets.Count > 0)
                {
                    enemy.targetController.SetTarget(enemyTargets[0]);
                }
            }
        }
    }
}