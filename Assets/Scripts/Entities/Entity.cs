using System;
using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour
    {
        //Событие смерти сущности
        public event Action<Entity> OnDeath;
        
        //Компонент взаимодействия сущности с физикой
        [SerializeField] protected Rigidbody2D rb;

        //Основные переменные сущности
        [SerializeField] protected int hitPoints = 1;
        [SerializeField] protected float entitySpeed = 3;
        [SerializeField] protected float rotationSpeed = 0.125f;
        [SerializeField] protected int   rotationModifier = 90;

        //Основные переменные-флаги жизненного цикла сущности
        protected bool IsMoving;
        protected bool IsDead;

        //Нанесение урона и смерть сущности при столкновении с чужой коллизией
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsDead)
            {
                //Уменьшение количества хитпоинтов сущности, если она жива
                hitPoints--;
                if (hitPoints > 0)
                {
                    Debug.Log($"Hit points left: {hitPoints}");
                }
                else
                {
                    //В случае уменьшения количества хитпоинов сущности до 0 вызывается событие смерти сущности
                    IsDead = true;
                    OnDeath?.Invoke(this);
                    Debug.Log("Is dead!");
                }
            }
        }

        //Обнуление переменной при инициализации сущности
        private void Start()
        {
            IsDead = false;
        }
        
        //Виртуальный метод перемещения сущности, перезаписывающийся в дочерних классах
        protected virtual void Move() { }
        
        //Виртуальный метод поворота сущности, перезаписывающийся в дочерних классах
        protected virtual void Rotate() { }

        //Расчет поворота сущности к заданной точке в пространстве
        protected Quaternion CalculateRotation(Vector2 targetPosition)
        {
            var entityRotation = transform.rotation;
            var entityPosition = (Vector2)transform.position;
            
            Vector2 vectorToTarget = entityPosition - targetPosition;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + rotationModifier;
            Quaternion rotationToTarget = Quaternion.AngleAxis(angle, Vector3.forward);
            return Quaternion.Slerp(entityRotation,rotationToTarget, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
