using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Entities
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected float projectileSpeed = 3;
        [SerializeField] protected float projectileLifetime = 2;
        
        private CancellationTokenSource _lifetimeTokenSource = new CancellationTokenSource();

        private void Move()
        {
            Vector2 forwardDirection = transform.up;
            rb.velocity = new Vector2(
                forwardDirection.x * projectileSpeed * Time.fixedDeltaTime, 
                forwardDirection.y * projectileSpeed * Time.fixedDeltaTime);
        }

        private async void DestroyCountdown()
        {
            await Task.Delay(TimeSpan.FromSeconds(projectileLifetime));
            if (_lifetimeTokenSource.IsCancellationRequested) return;
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Enemy _) || !other.isTrigger) return;
            Destroy(gameObject);
        }

        private void Start()
        {
            DestroyCountdown();
        }

        private void OnDestroy()
        {
            if (!_lifetimeTokenSource.IsCancellationRequested)
            {
                _lifetimeTokenSource?.Cancel();
            }
        }

        private void FixedUpdate()
        {
            Move();
        }
    }
}
