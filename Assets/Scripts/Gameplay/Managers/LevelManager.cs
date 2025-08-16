using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Grid;

namespace PuzzleGame.Gameplay.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Resources Settings")]
        [SerializeField] private string[] levelResourcePaths =
        {
            "Levels/level1",
            "Levels/level2",
            "Levels/level3",
            "Levels/level4"
        };

        [Header("Camera Settings")]
        [SerializeField] private bool centerCameraAfterLoad = true;
        [SerializeField] private float cameraPaddingCells = 1f;
        [SerializeField] private float cameraHeight = 10f;

        // 🔹 Inspector’dan ayarlanabilir offset
        [SerializeField] private float cameraOffsetX = 0.5f;
        [SerializeField] private float cameraOffsetZ = 0.5f;

        [Header("Level Management")]
        [SerializeField] private int defaultStartLevel = 1;
        [SerializeField] private bool loadDefaultLevelOnStart = false; // kapalı

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private readonly List<Cube> spawnedCubes = new();
        private int currentIndex = -1;
        private bool isInitialized = false;

        public int CurrentLevel => currentIndex >= 0 ? currentIndex + 1 : 0;
        public bool HasNextLevel => currentIndex + 1 < levelResourcePaths.Length;
        public bool IsLevelLoaded => currentIndex >= 0 && spawnedCubes.Count > 0;

        public Action<int> OnLevelStarted;
        public Action<int> OnLevelCompleted;
        public Action<int> OnLevelFailed;
        public Action OnAllLevelsCompleted;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InitializeLevelManager();
        }

        private void InitializeLevelManager()
        {
            if (isInitialized) return;

            if (!CheckDependencies())
            {
                DebugLog("[LevelManager] Dependencies not ready, retrying...");
                Invoke(nameof(InitializeLevelManager), 0.05f);
                return;
            }

            isInitialized = true;
            DebugLog("[LevelManager] Initialized successfully");
        }

        private bool CheckDependencies()
        {
            return GameManager.Instance != null &&
                   GridManager.Instance != null &&
                   PoolManager.Instance != null;
        }

        public void StartLevel(int levelNumber)
        {
            if (!isInitialized)
            {
                InitializeLevelManager();
                if (!isInitialized) return;
            }

            int idx = Mathf.Clamp(levelNumber - 1, 0, levelResourcePaths.Length - 1);
            LoadLevelByIndex(idx);
        }

        public void RestartLevel()
        {
            if (currentIndex >= 0) LoadLevelByIndex(currentIndex);
            else StartLevel(1);
        }

        public void NextLevel()
        {
            if (!HasNextLevel)
            {
                OnAllLevelsCompleted?.Invoke();
                return;
            }
            LoadLevelByIndex(currentIndex + 1);
        }

        private void LoadLevelByIndex(int idx)
        {
            if (idx < 0 || idx >= levelResourcePaths.Length) return;

            UnloadCurrentLevel();
            currentIndex = idx;

            var ta = Resources.Load<TextAsset>(levelResourcePaths[currentIndex]);
            if (ta == null || string.IsNullOrWhiteSpace(ta.text))
            {
                Debug.LogError($"[LevelManager] Level JSON not found: {levelResourcePaths[currentIndex]}");
                return;
            }

            LevelData data;
            try { data = JsonUtility.FromJson<LevelData>(ta.text); }
            catch (Exception e) { Debug.LogError($"JSON parse error: {e.Message}"); return; }

            GameManager.Instance?.StartLevel(data.level, data.moves);
            SpawnCubes(data);

            // 🔹 Kamera JSON’dan gelsin
            if (data.camera != null)
            {
                cameraHeight       = data.camera.height;
                cameraOffsetX      = data.camera.offsetX;
                cameraOffsetZ      = data.camera.offsetZ;
                cameraPaddingCells = data.camera.padding;
            }

            if (centerCameraAfterLoad) FitCameraToContent();
            OnLevelStarted?.Invoke(CurrentLevel);
        }


        private void SpawnCubes(LevelData data)
        {
            if (data.cubes == null) return;

            foreach (var c in data.cubes)
            {
                Direction dir = ParseDirection(c.direction);
                Color col = ParseColor(c.color, Color.white);
                Vector3Int gridPos = new(c.x, 0, c.z);
                Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);

                var cd = new CubeData(CubeType.Basic, gridPos, dir, col);
                var cube = SpawnCube(cd, worldPos);
                if (cube != null)
                {
                    spawnedCubes.Add(cube);
                    GridManager.Instance.RegisterObject(cube);
                }
            }
        }

        private void UnloadCurrentLevel()
        {
            if (spawnedCubes.Count > 0)
            {
                PoolManager.Instance?.ReturnAll<Cube>();
                spawnedCubes.Clear();
            }
        }

        private Cube SpawnCube(CubeData data, Vector3 worldPos)
        {
            var cube = PoolManager.Instance.Get<Cube>();
            if (cube == null) return null;
            cube.transform.position = worldPos;
            cube.Initialize(data);
            return cube;
        }

        private static Direction ParseDirection(string s)
        {
            return s?.ToLowerInvariant() switch
            {
                "up" => Direction.Up,
                "down" => Direction.Down,
                "left" => Direction.Left,
                "right" => Direction.Right,
                _ => Direction.Right
            };
        }

        private static Color ParseColor(string s, Color fallback)
        {
            if (ColorUtility.TryParseHtmlString(s, out var parsed)) return parsed;
            return fallback;
        }

        private void FitCameraToContent()
        {
            if (spawnedCubes.Count == 0 || Camera.main == null) return;

            float cell = GridManager.Instance.CellSize;
            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            foreach (var c in spawnedCubes)
            {
                var gp = c.GridPosition;
                minX = Mathf.Min(minX, gp.x);
                maxX = Mathf.Max(maxX, gp.x);
                minZ = Mathf.Min(minZ, gp.z);
                maxZ = Mathf.Max(maxZ, gp.z);
            }

            int minSize = 6;
            int levelWidth = Mathf.Max((maxX - minX + 1), minSize);
            int levelHeight = Mathf.Max((maxZ - minZ + 1), minSize);

            float worldWidth = levelWidth * cell;
            float worldHeight = levelHeight * cell;

            // 🔹 Blokların ortası
            Vector3 centerWorld = new(
                (minX + maxX) * 0.5f * cell,
                0f,
                (minZ + maxZ) * 0.5f * cell
            );

            // 🔹 Inspector offset uygula
            centerWorld.x += cameraOffsetX;
            centerWorld.z += cameraOffsetZ;

            var cam = Camera.main;
            cam.orthographic = true;
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.transform.position = new Vector3(centerWorld.x, cameraHeight, centerWorld.z);

            float aspect = cam.aspect;
            float padding = cameraPaddingCells * cell;

            cam.orthographicSize = Mathf.Max(
                (worldHeight / 2f) + padding,
                (worldWidth / 2f) / aspect + padding
            );
        }

        public void CompleteLevel() => CompleteLevelFast();
        public void FailLevel() => FailLevelFast();

        private void CompleteLevelFast()
        {
            OnLevelCompleted?.Invoke(CurrentLevel);
            StartCoroutine(UnloadCurrentLevelAsync(false));
        }

        private void FailLevelFast()
        {
            OnLevelFailed?.Invoke(CurrentLevel);
            StartCoroutine(UnloadCurrentLevelAsync(true));
        }

        private System.Collections.IEnumerator UnloadCurrentLevelAsync(bool withDelay)
        {
            if (spawnedCubes.Count > 0 && PoolManager.Instance != null)
            {
                var cubesToReturn = new List<Cube>(spawnedCubes);
                spawnedCubes.Clear();

                int counter = 0;
                foreach (var cube in cubesToReturn)
                {
                    if (cube != null)
                        PoolManager.Instance.Return(cube);

                    counter++;
                    if (withDelay && counter % 10 == 0)
                        yield return null;
                }
            }
        }

        private void DebugLog(string msg)
        {
            if (enableDebugLogs) Debug.Log(msg);
        }

        [ContextMenu("Debug - Show Current Level")]
        private void DebugShowCurrentLevel()
        {
            DebugLog($"Current Level Index: {currentIndex}");
            DebugLog($"Current Level Number: {CurrentLevel}");
            DebugLog($"Has Next Level: {HasNextLevel}");
            DebugLog($"Is Level Loaded: {IsLevelLoaded}");
            DebugLog($"Spawned Cubes: {spawnedCubes.Count}");
        }

        [ContextMenu("Debug - Start Level 1")]
        private void DebugStartLevel1() => StartLevel(1);

        [ContextMenu("Debug - Start Level 4")]
        private void DebugStartLevel4() => StartLevel(4);
    }
}
