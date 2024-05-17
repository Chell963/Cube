using System;
using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour
    {
        public event Action<Entity> OnDeath;
        
        [SerializeField] protected Rigidbody2D rb;

        [SerializeField] protected int hitPoints = 1;
        [SerializeField] protected float entitySpeed = 3;
        [SerializeField] protected float rotationSpeed = 0.125f;
        [SerializeField] protected int   rotationModifier = 90;

        protected bool IsMoving;
        protected bool IsDead;

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsDead)
            {
                hitPoints--;
                if (hitPoints > 0)
                {
                    Debug.Log($"Hit points left: {hitPoints}");
                }
                else
                {
                    IsDead = true;
                    OnDeath?.Invoke(this);
                    Debug.Log("Is dead!");
                }
            }
        }

        private void Start()
        {
            IsDead = false;
        }
        
        protected virtual void Move() { }
        
        protected virtual void Rotate() { }

        protected Quaternion CalculateRotation(Vector2 targetPosition)
        {
            var entityRotation = transform.rotation;
            var entityPosition = (Vector2)transform.position;
            
            Vector2 vectorToTarget = entityPosition - targetPosition;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + rotationModifier;
            Quaternion rotationToTarget = Quaternion.AngleAxis(angle, Vector3.forward);
            return Quaternion.Slerp(entityRotation,rotationToTarget, rotationSpeed * Time.deltaTime);
        }
    }
}
