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
        //Событие замечания врага
        private event Action<bool> OnEnemyDetected;
        
        //Компоненты UI
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highestScore;
        [SerializeField] private Button startGameButton;
        [SerializeField] private GameObject startScreen;
        [SerializeField] private Button exitGameButton;
        [SerializeField] private GameObject endScreen;
        [SerializeField] private Button restartButton;
        //Компоненты геймпля
        [Header("Gameplay")]
        [SerializeField] private Player playerPrefab;
        [SerializeField] private Enemy  enemyPrefab;
        [SerializeField] private Transform entitiesParent;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private FloatingJoystick playerJoystick;
        
        //Перенные отвечающие за появление врагов
        [SerializeField] private int maxEnemiesCount;
        [SerializeField] private int minEnemyCountdown;
        [SerializeField] private int maxEnemyCountdown;

        private bool _isPlaying;

        private Player _player;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Enemy> _enemiesDetected = new List<Enemy>();

        //Переменные отвечающие за смещение при появлении врагов
        private int _xStartSpawnOffset = 17;
        private int _yStartSpawnOffset = 12;
        private int _xSpawnOffset = 2;
        private int _ySpawnOffset = 1;

        private int _enemiesCount;

        //Токен отмены конца игры
        private CancellationTokenSource _endGameTokenSource = new CancellationTokenSource();

        private int _score;

        //Инициализация игры загрузка/сохранение количества очков
        private void Start()
        {
            Application.targetFrameRate = 45;
            
            //Инициализация UI компонентов и подписка кнопок на нажатие
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

        //Отписка кнопок на нажатие при уничтожении объекта
        private void OnDestroy()
        {
            if (_isPlaying)
            {
                _endGameTokenSource?.Cancel();
            }
            startGameButton.onClick.RemoveListener(StartGame);
            exitGameButton.onClick.RemoveListener(ExitGame);
            restartButton.onClick.RemoveListener(Restart);
        }

        //Перезапуск игры
        private void Restart()
        {
            //Обнуление счета
            int savedScore = PlayerPrefs.GetInt("HighestScore");
            highestScore.text = $"Highest score: {savedScore}";
            _score = 0;
            scoreText.text = $"Score: {_score}";
            
            //Уничтожение врагов
            startScreen.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(false);
            foreach (var enemy in _enemies)
            {
                Destroy(enemy.gameObject);
            }

            //Обнуление позиции камеры
            Vector3 cameraDeathPosition = playerCamera.transform.position;
            playerCamera.transform.position = new Vector3(0, 0, cameraDeathPosition.z);
            
            _enemiesCount = 0;
            _enemies.Clear();
            _enemiesDetected.Clear();
        }

        //Выход из игры
        private void ExitGame()
        {
            Application.Quit();
        }

        //Старт игры
        private void StartGame()
        {
            startScreen.gameObject.SetActive(false);
            playerJoystick.gameObject.SetActive(true);
            _isPlaying = true;
            //Создание игрока и подписок на события
            _player = Instantiate(playerPrefab, entitiesParent);
            _player.InitializePlayerJoystick(playerJoystick);
            _player.SetDetectedEnemies(_enemiesDetected);
            _player.OnDeath += DestroyPlayer;
            //Инициализция камеры
            playerCamera.InitializePlayerCamera(_player);
            OnEnemyDetected += _player.CheckEnemyDetection;
            _endGameTokenSource = new CancellationTokenSource();
            //Начало появления врагов
            SpawnEnemy();
        }

        //Конец игры
        private void EndGame()
        {
            //Присвоение большего счета
            int savedScore = PlayerPrefs.GetInt("HighestScore");
            if (_score > savedScore)
            {
                PlayerPrefs.SetInt("HighestScore", _score);
            }
            
            //Отписка от собыйтий
            _endGameTokenSource?.Cancel();
            endScreen.gameObject.SetActive(true);
            playerJoystick.gameObject.SetActive(false);
            _isPlaying = false;
            _player.OnDeath -= DestroyPlayer;
            OnEnemyDetected -= _player.CheckEnemyDetection;
        }

        //Появление врагов
        private async void SpawnEnemy()
        {
            while (_isPlaying)
            {
                if (_enemiesCount < maxEnemiesCount)
                {
                    //Таймер появления врагов
                    float enemyCountdown = Random.Range(minEnemyCountdown, maxEnemyCountdown + 1);
                    await Task.Delay(TimeSpan.FromSeconds(enemyCountdown));
                    if (_endGameTokenSource.IsCancellationRequested) return;
                    Enemy spawnedEnemy = Instantiate(enemyPrefab, entitiesParent);
                    
                    //Расчет позиции поялвения врагов
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
                    
                    //Инициализция врага и подписка на ивенты
                    spawnedEnemy.transform.position = enemySpawnPosition;
                    spawnedEnemy.InitializeEnemy(_player);
                    spawnedEnemy.OnDeath += DestroyEnemy;
                    spawnedEnemy.OnVisibilityChanged += DetectEnemy;
                    
                    _enemies.Add(spawnedEnemy);
                    _enemiesCount++;
                }
                else
                {
                    //Ожидание тика процессора, если ничего не происходит
                    await Task.Yield();
                }
            }
        }

        //Уничтожение врага
        private void DestroyEnemy(Entity entityToDestroy)
        {
            //Уничтожение врага и отписка от событий
            Enemy enemyEntity = (Enemy)entityToDestroy;
            enemyEntity.OnDeath -= DestroyEnemy;
            enemyEntity.OnVisibilityChanged -= DetectEnemy;
            
            //Очистка массивов с врагами от уничтоженного врага
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
            
            //Вычет количества врагов и увеличение счета
            _enemiesCount--;
            _score++;
            scoreText.text = $"Score: {_score}";
        }

        //Уничтожение игрока
        private void DestroyPlayer(Entity entityToDestroy)
        {
            //Уничтожение игрока и запуск конца игры
            Player playerEntity = (Player)entityToDestroy;
            EndGame();
            Destroy(playerEntity.gameObject);
        }

        //Замечание врага
        private void DetectEnemy(Enemy enemyToDetect, bool isDetected)
        {
            //Замечение врага и редактирование массива в этой зависимости
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
