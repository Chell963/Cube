using Service;
using UnityEngine;

namespace Background
{
    public class Background : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backgroundSprite;

        private Transform _cameraTransform;
        private Vector3   _lastCameraPosition;

        private float _textureUnitSizeX;
        private float _textureUnitSizeY;

        private void Initialize()
        {
            _cameraTransform = Camera.main.transform;
            _lastCameraPosition = _cameraTransform.position;
            Sprite sprite = backgroundSprite.sprite;
            Texture2D texture = sprite.texture;
            float pixelsPerUnit = sprite.pixelsPerUnit;
            _textureUnitSizeX = texture.width / pixelsPerUnit;
            _textureUnitSizeY = texture.height / pixelsPerUnit;
        }

        private void Move()
        {
            _lastCameraPosition = _cameraTransform.position;

            Vector3 position = transform.position;
            float distanceX = Mathf.Abs(_lastCameraPosition.x - position.x);
            float distanceY = Mathf.Abs(_lastCameraPosition.y - position.y);

            if (distanceX >= _textureUnitSizeX)
            {
                float offsetPositionX = distanceX % _textureUnitSizeX;
                transform.position = new Vector3(_lastCameraPosition.x + offsetPositionX, position.y);
            }

            if (distanceY >= _textureUnitSizeY)
            {
                float offsetPositionY = distanceY % _textureUnitSizeY;
                transform.position = new Vector3(position.x, _lastCameraPosition.y + offsetPositionY);
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            Move();
        }
    }
}
