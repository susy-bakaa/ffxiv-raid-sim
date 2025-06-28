using UnityEngine;
using NaughtyAttributes;

public class DisableObjectTrigger : MonoBehaviour
{
    Collider m_collider;

    public bool log = false;
    [Tag]
    public string ableToHitTag;
    public bool initializeOnStart = true;
    public float triggerDelay = 0f;

    private int id = 0;
    private bool colliderWaDisabled = false;

    void Awake()
    {
        m_collider = GetComponent<Collider>();
        id = Random.Range(1000,10000);

        if (m_collider != null)
        {
            if (m_collider.enabled)
            {
                colliderWaDisabled = false;
            }
            else
            {
                colliderWaDisabled = true;
            }
        }
        else
        {
            colliderWaDisabled = true;
        }
    }

    void Start()
    {
        if (initializeOnStart)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        if (m_collider != null && triggerDelay > 0f)
        {
            m_collider.enabled = false;
            Utilities.FunctionTimer.Create(this, () => m_collider.enabled = true, triggerDelay, $"{id}_{gameObject.name}_delay", false, true);
        }
    }

    public void ResetTrigger()
    {
        if (m_collider != null)
        {
            if (colliderWaDisabled)
            {
                m_collider.enabled = false;
            }
            else
            {
                m_collider.enabled = true;
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        var otherGameObject = other.gameObject;

        if (other.CompareTag(ableToHitTag))
        {
            otherGameObject.SetActive(false);

            if (log)
            {
                Debug.Log($"[DisableNodeTrigger] \"{otherGameObject.name}\" was hit and disabled.");
            }
        }
    }
}
