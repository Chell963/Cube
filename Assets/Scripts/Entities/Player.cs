using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Entities
{
    public class Player : Entity
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform  projectileSpawnPosition;
        [SerializeField] private float      projectileFireCooldown;
        
        private bool _enemyDetected;
        private bool _readyToFire = true;
        
        private List<Enemy> _enemiesDetected = new List<Enemy>();
        private FloatingJoystick _floatingJoystick;
        
        private CancellationTokenSource _isDeadTokenSource = new CancellationTokenSource();

        public void InitializePlayerJoystick(FloatingJoystick newJoystick)
        {
            _floatingJoystick = newJoystick;
        }

        public void CheckEnemyDetection(bool isDetected)
        {
            _enemyDetected = isDetected;
            FireProjectile();
        }

        public void SetDetectedEnemies(List<Enemy> enemiesList)
        {
            _enemiesDetected = enemiesList;
        }

        protected override void Move()
        {
            Vector2 joystickDirection = _floatingJoystick.Direction;
            IsMoving = joystickDirection.x != 0 || joystickDirection.y != 0;
            rb.velocity = IsMoving 
                ? new Vector2(
                    joystickDirection.x * entitySpeed * Time.fixedDeltaTime, 
                    joystickDirection.y * entitySpeed * Time.fixedDeltaTime) 
                : Vector2.zero;
        }
        
        protected override void Rotate()
        {
            var playerPosition = (Vector2)transform.position;
            if (_enemyDetected)
            {
                Enemy targetEnemy = _enemiesDetected[0];
                float distanceToNearestEnemy = Vector2.Distance(playerPosition, targetEnemy.transform.position);
                foreach (var enemy in _enemiesDetected)
                {
                    float distanceToEnemy = Vector2.Distance(playerPosition, enemy.transform.position);
                    if (distanceToEnemy < distanceToNearestEnemy)
                    {
                        targetEnemy = enemy;
                        distanceToNearestEnemy = distanceToEnemy;
                    }
                }
                Vector2 enemyPosition = targetEnemy.transform.position;
                transform.rotation = CalculateRotation(enemyPosition);
            }
            else
            {
                if (IsMoving)
                {
                    transform.rotation = CalculateRotation(playerPosition + _floatingJoystick.Direction);
                }
            }
        }
        
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Enemy _) || !other.isTrigger) return;
            base.OnTriggerEnter2D(other);
        }

        private void FixedUpdate()
        {
            if (IsDead || _floatingJoystick == null) return;
            Rotate();
            Move();
        }
        
        private void OnDestroy()
        {
            _enemyDetected = false;
            _isDeadTokenSource?.Cancel();
        }

        private async void FireProjectile()
        {
            while (_enemyDetected)
            {
                if (_readyToFire)
                {
                    _readyToFire = false;
                    float homingTime = 0.5f;
                    await Task.Delay(TimeSpan.FromSeconds(homingTime));
                    if (_isDeadTokenSource.IsCancellationRequested) return;
                    Projectile projectileSpawned = Instantiate(projectilePrefab, transform.parent);
                    Transform projectileTransform = projectileSpawned.transform;
                    projectileTransform.position = projectileSpawnPosition.position;
                    projectileTransform.rotation = transform.rotation;
                    await Task.Delay(TimeSpan.FromSeconds(projectileFireCooldown));
                    if (_isDeadTokenSource.IsCancellationRequested) return;
                    _readyToFire = true;
                }
                else
                {
                    await Task.Yield();
                }
            }
        }
    }
}
