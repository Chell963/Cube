using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Service
{
    public class Controller : MonoBehaviour
    {
        private event Action<bool> OnEnemyDetected;
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highestScore;
        [SerializeField] private Button startGameButton;
        [SerializeField] private GameObject startScreen;
        [SerializeField] private Button exitGameButton;
        [SerializeField] private GameObject endScreen;
        [SerializeField] private Button restartButton;
        [Header("Gameplay")]
        [SerializeField] private Player playerPrefab;
        [SerializeField] private Enemy  enemyPrefab;
        [SerializeField] private Transform entitiesParent;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private FloatingJoystick playerJoystick;
        
        [SerializeField] private int maxEnemiesCount;
        [SerializeField] private int minEnemyCountdown;
        [SerializeField] private int maxEnemyCountdown;

        private bool _isPlaying;

        private Player _player;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Enemy> _enemiesDetected = new List<Enemy>();

        private int _xStartSpawnOffset = 17;
        private int _yStartSpawnOffset = 12;
        private int _xSpawnOffset = 2;
        private int _ySpawnOffset = 1;

        private int _enemiesCount;

        private CancellationTokenSource _endGameTokenSource = new CancellationTokenSource();

        private int _score;

        private void Start()
        {
            Application.targetFrameRate = 45;
            
            startScreen.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(false);
            playerJoystick.gameObject.SetActive(false);
            startGameButton.onClick.AddListener(StartGame);
            exitGameButton.onClick.AddListener(ExitGame);
            restartButton.onClick.AddListener(Restart);

            if (PlayerPrefs.HasKey("HighestScore"))
            {
                int savedScore = PlayerPrefs.GetInt("HighestScore");
                highestScore.text = $"Highest score: {savedScore}";
            }
            else
            {
                PlayerPrefs.SetInt("HighestScore", 0);
            }
        }

        private void OnDestroy()
        {
            if (_isPlaying)
            {
                _endGameTokenSource?.Cancel();
            }
            startGameButton.onClick.RemoveListener(StartGame);
            exitGameButton.onClick.RemoveListener(ExitGame);
        }

        private void Restart()
        {
            int savedScore = PlayerPrefs.GetInt("HighestScore");
            highestScore.text = $"Highest score: {savedScore}";
            _score = 0;
            scoreText.text = $"Score: {_score}";
            
            startScreen.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(false);
            foreach (var enemy in _enemies)
            {
                Destroy(enemy.gameObject);
            }

            Vector3 cameraDeathPosition = playerCamera.transform.position;
            playerCamera.transform.position = new Vector3(0, 0, cameraDeathPosition.z);
            
            _enemiesCount = 0;
            _enemies.Clear();
            _enemiesDetected.Clear();
        }

        private void ExitGame()
        {
            Application.Quit();
        }

        private void StartGame()
        {
            startScreen.gameObject.SetActive(false);
            playerJoystick.gameObject.SetActive(true);
            _isPlaying = true;
            _player = Instantiate(playerPrefab, entitiesParent);
            _player.InitializePlayerJoystick(playerJoystick);
            _player.SetDetectedEnemies(_enemiesDetected);
            _player.OnDeath += DestroyPlayer;
            playerCamera.InitializePlayerCamera(_player);
            OnEnemyDetected += _player.CheckEnemyDetection;
            _endGameTokenSource = new CancellationTokenSource();
            SpawnEnemy();
        }

        private void EndGame()
        {
            int savedScore = PlayerPrefs.GetInt("HighestScore");
            if (_score > savedScore)
            {
                PlayerPrefs.SetInt("HighestScore", _score);
            }
            
            _endGameTokenSource?.Cancel();
            endScreen.gameObject.SetActive(true);
            playerJoystick.gameObject.SetActive(false);
            _isPlaying = false;
            _player.OnDeath -= DestroyPlayer;
            OnEnemyDetected -= _player.CheckEnemyDetection;
        }

        private async void SpawnEnemy()
        {
            while (_isPlaying)
            {
                if (_enemiesCount < maxEnemiesCount)
                {
                    float enemyCountdown = Random.Range(minEnemyCountdown, maxEnemyCountdown + 1);
                    await Task.Delay(TimeSpan.FromSeconds(enemyCountdown));
                    if (_endGameTokenSource.IsCancellationRequested) return;
                    Enemy spawnedEnemy = Instantiate(enemyPrefab, entitiesParent);
                    
                    Vector3 playerPosition = _player.transform.position;
                    bool isNegativeX = Convert.ToBoolean(Random.Range(0,1 + 1));
                    bool isNegativeY = Convert.ToBoolean(Random.Range(0,1 + 1));
                    
                    int randomX = Random.Range(0, _xSpawnOffset + 1);
                    int randomY = Random.Range(0, _xSpawnOffset + 1);
                    
                    int spawnX = isNegativeX 
                        ? (int)playerPosition.x - _xStartSpawnOffset 
                        : (int)playerPosition.x + _xStartSpawnOffset;
                    int spawnY = isNegativeY 
                        ? (int)playerPosition.y - _yStartSpawnOffset 
                        : (int)playerPosition.y + _yStartSpawnOffset;
                    
                    Vector3 enemySpawnPosition = new Vector3(spawnX + randomX, spawnY + randomY, playerPosition.z);
                    
                    spawnedEnemy.transform.position = enemySpawnPosition;
                    spawnedEnemy.InitializeEnemy(_player);
                    spawnedEnemy.OnDeath += DestroyEnemy;
                    spawnedEnemy.OnVisibilityChanged += DetectEnemy;
                    
                    _enemies.Add(spawnedEnemy);
                    _enemiesCount++;
                }
                else
                {
                    await Task.Yield();
                }
            }
        }

        private void DestroyEnemy(Entity entityToDestroy)
        {
            Enemy enemyEntity = (Enemy)entityToDestroy;
            enemyEntity.OnDeath -= DestroyEnemy;
            enemyEntity.OnVisibilityChanged -= DetectEnemy;
            
            if (_enemies.Contains(enemyEntity))
            {
                _enemies.Remove(enemyEntity);
            }
            if (_enemiesDetected.Contains(enemyEntity))
            {
                _enemiesDetected.Remove(enemyEntity);
                OnEnemyDetected?.Invoke(_enemiesDetected.Count != 0);
            }
            Destroy(enemyEntity.gameObject);

            _enemiesCount--;
            _score++;
            scoreText.text = $"Score: {_score}";
        }

        private void DestroyPlayer(Entity entityToDestroy)
        {
            Player playerEntity = (Player)entityToDestroy;
            EndGame();
            Destroy(playerEntity.gameObject);
        }

        private void DetectEnemy(Enemy enemyToDetect, bool isDetected)
        {
            if (isDetected)
            {
                _enemiesDetected.Add(enemyToDetect);
            }
            else
            {
                if (_enemiesDetected.Contains(enemyToDetect))
                {
                    _enemiesDetected.Remove(enemyToDetect);
                }
            }
            OnEnemyDetected?.Invoke(_enemiesDetected.Count != 0);
        }
    }
}
