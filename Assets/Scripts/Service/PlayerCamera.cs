using Entities;
using UnityEngine;

namespace Service
{
    public class PlayerCamera : MonoBehaviour
    {
        //Скорость плавного перемещения камеры и её смещение от точки следования
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector2 offset;
        
        //Скорость плавного перемещения в виде вектора
        private Vector2 _velocity = Vector3.zero;
        
        //Сущность игрока (точка следования камеры)
        private Player _player;

        //Инициализация камеры
        public void InitializePlayerCamera(Player newPlayer)
        {
            //Кэширование сущности игрока в камеру
            _player = newPlayer;
        }

        //Плавное перемещегте камеры
        private void SmoothMove()
        {
            //Получние различных позиций, а именно - позиции камеры, позиции игрока, нужной позиции и сглаженной (плавной) позциии
            Vector3 cameraPosition = transform.position;
            Vector2 playerPosition = _player.transform.position;
            Vector2 desiredPosition = playerPosition + offset;
            Vector2 smoothedPosition = Vector2.SmoothDamp(cameraPosition, desiredPosition, ref _velocity, 
                smoothSpeed * Time.deltaTime);
            //Задание текущей позиции в виде сглаженной (плавной) позиции
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, cameraPosition.z);
        }

        //Плавное перемещение камеры, если существует сущность игрока
        private void LateUpdate()
        {
            if (_player == null) return;
            SmoothMove();
        }
    }
}
