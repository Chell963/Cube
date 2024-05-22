using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Entities
{
    public class Player : Entity
    {
        //Компоненты снаряда
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform  projectileSpawnPosition;
        [SerializeField] private float      projectileFireCooldown;
        
        //Переменные-флаги для взаимодействия с врагами
        private bool _enemyDetected;
        private bool _readyToFire = true;
        
        //Массив замеченных игроком врагов
        private List<Enemy> _enemiesDetected = new List<Enemy>();
        //Компонент джойстика
        private FloatingJoystick _floatingJoystick;
        
        //Токен отмены смерти игрока
        private CancellationTokenSource _isDeadTokenSource = new CancellationTokenSource();

        //Инициализация джойстика игрока
        public void InitializePlayerJoystick(FloatingJoystick newJoystick)
        {
            _floatingJoystick = newJoystick;
        }

        //Проверка замеченных врагов и попытка выстрела пули
        public void CheckEnemyDetection(bool isDetected)
        {
            _enemyDetected = isDetected;
            FireProjectile();
        }

        //Передача игроку массива замеченных врагов
        public void SetDetectedEnemies(List<Enemy> enemiesList)
        {
            _enemiesDetected = enemiesList;
        }

        //Перемещение игрока
        protected override void Move()
        {
            //Перемещение игрока в направлении джойстика
            Vector2 joystickDirection = _floatingJoystick.Direction;
            IsMoving = joystickDirection.x != 0 || joystickDirection.y != 0;
            rb.velocity = IsMoving 
                ? new Vector2(
                    joystickDirection.x * entitySpeed * Time.fixedDeltaTime, 
                    joystickDirection.y * entitySpeed * Time.fixedDeltaTime) 
                : Vector2.zero;
        }
        
        //Поворот игрока
        protected override void Rotate()
        {
            var playerPosition = (Vector2)transform.position;
            if (_enemyDetected)
            {
                //Поворот игрока к ближейшему врагу
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
                //Поворот игрока в сторону направления джойстика
                if (IsMoving)
                {
                    transform.rotation = CalculateRotation(playerPosition + _floatingJoystick.Direction);
                }
            }
        }
        
        //Нанесение урона игроку при столкновении с врагом
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Enemy _) || !other.isTrigger) return;
            base.OnTriggerEnter2D(other);
        }

        //Передвижение и поворот игрока каждый кадр игры, если он жив и у него инициализирован джойстик
        private void FixedUpdate()
        {
            if (IsDead || _floatingJoystick == null) return;
            Rotate();
            Move();
        }
        
        //Обнуление переменных при уничтожении игрока
        private void OnDestroy()
        {
            _enemyDetected = false;
            _isDeadTokenSource?.Cancel();
        }

        //Выстрел снаряда
        private async void FireProjectile()
        {
            while (_enemyDetected)
            {
                if (_readyToFire)
                {
                    //Выстрел снаряда по таймеру
                    _readyToFire = false;
                    float homingTime = 0.5f;
                    await Task.Delay(TimeSpan.FromSeconds(homingTime));
                    if (_isDeadTokenSource.IsCancellationRequested) return;
                    //Инициализация снаряда и передача ему переменных позиции и поворота
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
                    //Ожидание тика процессора, если ничего не происходит
                    await Task.Yield();
                }
            }
        }
    }
}
