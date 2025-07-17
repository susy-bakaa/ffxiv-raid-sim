using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleMovement : MonoBehaviour
    {
        public Vector3 target = Vector3.zero;
        public float speed = 0.1f;
        public bool OnUpdate = true;
        public bool loop = true;
        private Vector3 originalPosition;
        private bool movingToTarget = true;

        private void Awake()
        {
            movingToTarget = true;
            originalPosition = transform.localPosition;
        }

        private void Update()
        {       
            if (!OnUpdate)
                return;

            if (movingToTarget)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
                if (transform.localPosition == target)
                {
                    movingToTarget = false;
                }
            }
            else if (loop)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, speed * Time.deltaTime);
                if (transform.localPosition == originalPosition)
                {
                    movingToTarget = true;
                }
            }
        }

        public void ResetPosition()
        {
            transform.localPosition = originalPosition;
            movingToTarget = true;
        }

        public void Move()
        {
            if (speed > 0f && movingToTarget)
            {
                LeanTween.moveLocal(gameObject, target, speed);
                movingToTarget = false;
            }
            else if (movingToTarget)
            {
                transform.localPosition = target;
                movingToTarget = false;
            }
        }
    }
}