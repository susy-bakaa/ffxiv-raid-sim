using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    TargetNode node;

    Dictionary<GameObject, TargetController> players;

    void Awake()
    {
        node = transform.parent.GetComponent<TargetNode>();
        players = new Dictionary<GameObject, TargetController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (players.TryGetValue(other.gameObject, out TargetController cachedController))
            {
                if (node != null)
                {
                    node.AddNodeToController(cachedController.self);
                }
            }
            else if (other.transform.parent.TryGetComponent(out TargetController controller))
            {
                if (node != null)
                {
                    players.TryAdd(other.gameObject, controller);
                    node.AddNodeToController(controller.self);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (players.TryGetValue(other.gameObject, out TargetController cachedController))
            {
                if (node != null)
                {
                    node.RemoveNodeFromController(cachedController.self);
                }
            }
            else if (other.transform.parent.TryGetComponent(out TargetController controller))
            {
                if (node != null)
                {
                    players.TryAdd(other.gameObject, controller);
                    node.RemoveNodeFromController(controller.self);
                }
            }
        }
    }
}
