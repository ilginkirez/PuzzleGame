using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Helpers;           // Singleton, Directions
using PuzzleGame.Gameplay.Cubes;        // Cube, CubeData, CubeType
using PuzzleGame.Gameplay.Grid;         // GridManager

namespace PuzzleGame.Gameplay.Managers
{
    /// JSON’dan level yükler/boşaltır, küpleri spawn eder.
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Resources Settings")]
        [Tooltip("Resources/Levels altındaki dosyalar (uzantısız). Örn: Levels/level1")]
        [SerializeField] private string[] levelResourcePaths = { "Levels/level1", "Levels/level2",  "Levels/level3",  "Levels/level4" };
        

        [Header("Spawn Settings")]
        [SerializeField] private GameObject cubePrefab;          // Cube + Arrow içeren prefab
        [SerializeField] private bool centerCameraAfterLoad = true;
        [SerializeField] private float cameraPaddingCells = 0.75f;
        [SerializeField] private float cameraHeight = 12f;

        [Header("Debug / Convenience")]
        [SerializeField] private bool autoStartInPlayMode = true;   // Play’de otomatik yükle
        [SerializeField] private int autoStartLevel = 2;

        private readonly List<Cube> spawnedCubes = new();
        private int currentIndex = 0;

        public int  CurrentLevel  => currentIndex + 1;
        public bool HasNextLevel  => currentIndex + 1 < levelResourcePaths.Length;

        public Action<int> OnLevelStarted;
        public Action<int> OnLevelCompleted;
        public Action<int> OnLevelFailed;
        public Action     OnAllLevelsCompleted;

        private void Start()
        {
            // Kolay test: Play’de otomatik başlat
            if (autoStartInPlayMode)
            {
                Debug.Log($"[LevelManager] Auto start enabled -> loading level {autoStartLevel}");
                StartLevel(autoStartLevel);
            }
            else
            {
                Debug.Log("[LevelManager] Auto start disabled. Call LevelManager.Instance.StartLevel(n) yourself.");
            }
        }

        // ---- Public API ----
        public void StartLevel(int levelNumber)
        {
            int idx = Mathf.Clamp(levelNumber - 1, 0, Mathf.Max(0, levelResourcePaths.Length - 1));
            Debug.Log($"[LevelManager] StartLevel({levelNumber}) -> index {idx}");
            LoadLevelByIndex(idx);
        }

        public void RestartLevel()
        {
            Debug.Log($"[LevelManager] RestartLevel -> index {currentIndex}");
            LoadLevelByIndex(currentIndex);
        }

        public void NextLevel()
        {
            if (!HasNextLevel)
            {
                Debug.LogWarning("[LevelManager] No next level. All levels completed.");
                OnAllLevelsCompleted?.Invoke();
                return;
            }
            LoadLevelByIndex(currentIndex + 1);
        }

        public int GetActiveCubeCount() => spawnedCubes.Count;

        // ---- Core ----
        private void LoadLevelByIndex(int idx)
        {
            UnloadCurrentLevel();

            currentIndex = idx;

            // Prefab bağlı mı?
            if (cubePrefab == null)
            {
                Debug.LogError("[LevelManager] Cube Prefab is NOT assigned in inspector.");
                return;
            }

            // JSON oku
            string resPath = levelResourcePaths[currentIndex];
            Debug.Log($"[LevelManager] Loading JSON from Resources/{resPath}.json");
            TextAsset ta = Resources.Load<TextAsset>(resPath);

            if (ta == null || string.IsNullOrWhiteSpace(ta.text))
            {
                Debug.LogError($"[LevelManager] Level JSON not found or empty at Resources/{resPath}.json");
                return;
            }

            LevelData data = null;
            try
            {
                data = JsonUtility.FromJson<LevelData>(ta.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelManager] JSON parse error for {resPath}: {ex}");
            }

            if (data == null)
            {
                Debug.LogError($"[LevelManager] Level JSON parse failed for {resPath}");
                return;
            }

            Debug.Log($"[LevelManager] JSON parsed. level={data.level}, moves={data.moves}, cubes={(data.cubes != null ? data.cubes.Length : 0)}");

            // Move limiti GameManager’a (senin sürümünde StartLevel(level, moves) vardı)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartLevel(data.level, data.moves);
            }
            else
            {
                Debug.LogWarning("[LevelManager] GameManager.Instance is null (okey for barebones tests).");
            }

            // Küpleri spawn et
            if (data.cubes == null || data.cubes.Length == 0)
            {
                Debug.LogWarning("[LevelManager] LevelData.cubes is empty.");
            }
            else
            {
                foreach (var c in data.cubes)
                {
                    Direction dir = ParseDirection(c.direction);
                    Color col     = ParseColor(c.color, Color.white);

                    Vector3Int gridPos = new(c.x, 0, c.z);
                    Vector3 worldPos   = GridManager.Instance.GridToWorldPosition(gridPos);

                    var cd   = new CubeData(CubeType.Basic, gridPos, dir, col);
                    var cube = SpawnCube(cd, worldPos);

                    spawnedCubes.Add(cube);
                    GridManager.Instance.RegisterObject(cube);
                }
            }

            if (centerCameraAfterLoad)
                FitCameraToContent();

            OnLevelStarted?.Invoke(CurrentLevel);
            Debug.Log($"[LevelManager] Level {CurrentLevel} started. Moves: {data.moves}, Cubes: {spawnedCubes.Count}");
        }

        private void UnloadCurrentLevel()
        {
            if (spawnedCubes.Count > 0)
                Debug.Log($"[LevelManager] UnloadCurrentLevel -> destroying {spawnedCubes.Count} cubes");

            for (int i = spawnedCubes.Count - 1; i >= 0; i--)
            {
                var cube = spawnedCubes[i];
                if (cube)
                {
                    GridManager.Instance.UnregisterObject(cube);
                    Destroy(cube.gameObject);     // İleride: pool.Return(cube)
                }
            }
            spawnedCubes.Clear();
        }

        // ---- Helpers ----
        private Cube SpawnCube(CubeData data, Vector3 worldPos)
        {
            var go = Instantiate(cubePrefab, worldPos, Quaternion.identity);
            var cube = go.GetComponent<Cube>();
            if (cube == null)
            {
                Debug.LogError("[LevelManager] cubePrefab has no Cube component!");
                return null;
            }
            cube.Initialize(data);
            return cube;
        }

        private static Direction ParseDirection(string s)
        {
            if (string.IsNullOrEmpty(s)) return Direction.Right;
            switch (s.ToLowerInvariant())
            {
                case "up":    return Direction.Up;
                case "down":  return Direction.Down;
                case "left":  return Direction.Left;
                case "right": return Direction.Right;
            }
            return Direction.Right;
        }

        private static Color ParseColor(string s, Color fallback)
        {
            if (string.IsNullOrEmpty(s)) return fallback;
            if (ColorUtility.TryParseHtmlString(s, out var parsed)) return parsed;
            switch (s.ToLowerInvariant())
            {
                case "white": return Color.white;
                case "black": return Color.black;
                case "red":   return Color.red;
                case "green": return Color.green;
                case "blue":  return Color.blue;
                default:      return fallback;
            }
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

            float width  = (maxX - minX + 1) * cell;
            float height = (maxZ - minZ + 1) * cell;

            Vector3 centerWorld = new(
                (minX + maxX + 1) * 0.5f * cell - cell * 0.5f,
                0f,
                (minZ + maxZ + 1) * 0.5f * cell - cell * 0.5f
            );

            var cam = Camera.main;
            cam.orthographic = true;
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.transform.position = new Vector3(centerWorld.x, cameraHeight, centerWorld.z);

            float aspect = 9f / 16f; // mobil
            float pad = cameraPaddingCells * cell;
            float sizeByH = height * 0.5f + pad;
            float sizeByW = (width * 0.5f) / aspect + pad;
            cam.orthographicSize = Mathf.Max(sizeByH, sizeByW);

            Debug.Log($"[LevelManager] Camera centered. size={cam.orthographicSize:0.00}, center={centerWorld}");
        }

        // ---- Results (GameManager çağırır) ----
        public void CompleteLevel() => OnLevelCompleted?.Invoke(CurrentLevel);
        public void FailLevel()     => OnLevelFailed?.Invoke(CurrentLevel);
    }
}
