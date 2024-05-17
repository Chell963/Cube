using System;
using UnityEngine;

namespace Entities
{
    public class Enemy : Entity
    {
        public event Action<Enemy, bool> OnVisibilityChanged;

        private Player _playerToFollow;

        public void InitializeEnemy(Player playerToFollow)
        {
            _playerToFollow = playerToFollow;
        }

        private void OnBecameVisible()
        {
            OnVisibilityChanged?.Invoke(this, true);
        }
        
        private void OnBecameInvisible()
        {
            OnVisibilityChanged?.Invoke(this, false);
        }
        
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Projectile _) || !other.isTrigger) return;
            base.OnTriggerEnter2D(other);
        }
        
        protected override void Move()
        {
            Vector2 forwardDirection = transform.up;
            IsMoving = !IsDead;
            rb.velocity = IsMoving 
                ? new Vector2(forwardDirection.x * entitySpeed, forwardDirection.y * entitySpeed) 
                : Vector2.zero;
        }
        
        protected override void Rotate()
        {
            transform.rotation = CalculateRotation(_playerToFollow.transform.position);
        }

        private void FixedUpdate()
        {
            if (_playerToFollow == null)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            Rotate();
            Move();
        }
    }
}
