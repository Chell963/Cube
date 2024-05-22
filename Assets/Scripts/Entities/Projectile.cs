using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Entities
{
    public class Projectile : MonoBehaviour
    {
        //Компонент взаимодействия снаряда с физикой
        [SerializeField] protected Rigidbody2D rb;
        //Основные переменные снаряда
        [SerializeField] protected float projectileSpeed = 3;
        [SerializeField] protected float projectileLifetime = 2;
        
        //Токен отмены уничтожения снаряда
        private CancellationTokenSource _lifetimeTokenSource = new CancellationTokenSource();

        //Передвижение снаряда
        private void Move()
        {
            //Передвижение снаряда по траектории поворота снаряда
            Vector2 forwardDirection = transform.up;
            rb.velocity = new Vector2(
                forwardDirection.x * projectileSpeed * Time.fixedDeltaTime, 
                forwardDirection.y * projectileSpeed * Time.fixedDeltaTime);
        }

        //Уничтожение снаряда по таймеру его жизни
        private async void DestroyCountdown()
        {
            await Task.Delay(TimeSpan.FromSeconds(projectileLifetime));
            if (_lifetimeTokenSource.IsCancellationRequested) return;
            Destroy(gameObject);
        }
        
        //Уничтожение снаряда при столкновении с врагом
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Enemy _) || !other.isTrigger) return;
            Destroy(gameObject);
        }

        //Запуск таймера уничтожения снаряда
        private void Start()
        {
            DestroyCountdown();
        }

        //Актививация токена отмены при уничтожении
        private void OnDestroy()
        {
            if (!_lifetimeTokenSource.IsCancellationRequested)
            {
                _lifetimeTokenSource?.Cancel();
            }
        }

        //Передвижение снаряда каждый кадр игры
        private void FixedUpdate()
        {
            Move();
        }
    }
}
