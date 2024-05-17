using System;
using Entities;
using UnityEngine;

namespace Service
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector2 offset;
        
        private Vector2 _velocity = Vector3.zero;
        
        private Player _player;

        public void InitializePlayerCamera(Player newPlayer)
        {
            _player = newPlayer;
        }

        private void SmoothMove()
        {
            Vector3 cameraPosition = transform.position;
            Vector2 playerPosition = _player.transform.position;
            Vector2 desiredPosition = playerPosition + offset;
            Vector2 smoothedPosition = Vector2.SmoothDamp(cameraPosition, desiredPosition, ref _velocity, 
                smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(smoothedPosition.x,smoothedPosition.y, cameraPosition.z);
        }

        private void LateUpdate()
        {
            if (_player == null) return;
            SmoothMove();
        }
    }
}
