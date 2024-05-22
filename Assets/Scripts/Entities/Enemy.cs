using System;
using UnityEngine;

namespace Entities
{
    public class Enemy : Entity
    {
        //Событие замечания игрока
        public event Action<Enemy, bool> OnVisibilityChanged;

        //Сущность игрока, за которой следует враг
        private Player _playerToFollow;

        //Инициализация врага и передача ему сущности игрока
        public void InitializeEnemy(Player playerToFollow)
        {
            _playerToFollow = playerToFollow;
        }

        //Событие Unity, когда врага замечает камера
        private void OnBecameVisible()
        {
            OnVisibilityChanged?.Invoke(this, true);
        }
        
        //Событие Unity, когда камера теряет врага из виду
        private void OnBecameInvisible()
        {
            OnVisibilityChanged?.Invoke(this, false);
        }
        
        //Нанесения урона при столкновении с снарядом
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Projectile _) || !other.isTrigger) return;
            base.OnTriggerEnter2D(other);
        }
        
        //Перемещение врага
        protected override void Move()
        {
            //Перемещение врага в сторону игрока
            Vector2 forwardDirection = transform.up;
            IsMoving = !IsDead;
            rb.velocity = IsMoving 
                ? new Vector2(
                    forwardDirection.x * entitySpeed * Time.fixedDeltaTime, 
                    forwardDirection.y * entitySpeed * Time.fixedDeltaTime) 
                : Vector2.zero;
        }
        
        //Поворот врага
        protected override void Rotate()
        {
            //Поворот врага в сторону игрока
            transform.rotation = CalculateRotation(_playerToFollow.transform.position);
        }

        //Передвижение и поворот врага каждый кадр игры, если существует сущность игрока
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
