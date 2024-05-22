using UnityEngine;

namespace Background
{
    public class Background : MonoBehaviour
    {
        //Компонент спрайта заднего фона
        [SerializeField] private SpriteRenderer backgroundSprite;

        //Tranform и позиция камеры
        private Transform _cameraTransform;
        private Vector3   _lastCameraPosition;

        //Реальные размеры текстуры
        private float _textureUnitSizeX;
        private float _textureUnitSizeY;

        //Инициализация заднего фона
        private void Initialize()
        {
            //Кэширование Transform и позиции камеры
            _cameraTransform = Camera.main.transform;
            _lastCameraPosition = _cameraTransform.position;
            //Получение спрайта и текстуры заднего фона
            Sprite sprite = backgroundSprite.sprite;
            Texture2D texture = sprite.texture;
            //Получение и кэширование реальных размеров текстуры
            float pixelsPerUnit = sprite.pixelsPerUnit;
            _textureUnitSizeX = texture.width / pixelsPerUnit;
            _textureUnitSizeY = texture.height / pixelsPerUnit;
        }

        //Перемещение заднего фона
        private void Move()
        {
            //Кэширование позиции камеры
            _lastCameraPosition = _cameraTransform.position;

            //Получение позиции заднего фона и дистанции его от позиции камеры
            Vector3 position = transform.position;
            float distanceX = Mathf.Abs(_lastCameraPosition.x - position.x);
            float distanceY = Mathf.Abs(_lastCameraPosition.y - position.y);

            //Условие перемещения заднего фона по оси X
            if (distanceX >= _textureUnitSizeX)
            {
                float offsetPositionX = distanceX % _textureUnitSizeX;
                transform.position = new Vector3(_lastCameraPosition.x + offsetPositionX, position.y);
            }
            
            //Условие перемещения заднего фона по оси Y
            if (distanceY >= _textureUnitSizeY)
            {
                float offsetPositionY = distanceY % _textureUnitSizeY;
                transform.position = new Vector3(position.x, _lastCameraPosition.y + offsetPositionY);
            }
        }

        //Инициализация заднего фона при старте игры
        private void Start()
        {
            Initialize();
        }

        //Передвижение заднего фона в каждый кадр игры
        private void LateUpdate()
        {
            Move();
        }
    }
}
