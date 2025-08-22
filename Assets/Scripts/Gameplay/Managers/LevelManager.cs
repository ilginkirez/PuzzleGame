using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Grid;
using DG.Tweening;

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
        [SerializeField] private float cameraOffsetX = 0.5f;
        [SerializeField] private float cameraOffsetZ = 0.5f;

        [Header("Level Management")]
        [SerializeField] private int defaultStartLevel = 1;
        [SerializeField] private bool loadDefaultLevelOnStart = false;

        [Header("Color Palette")] // 🔹 YENİ: Inspector'dan ayarlanabilir renkler
        [SerializeField] private Color primaryColor = new Color(1f, 0.4f, 0.8f, 1f);    // #FF66CC - Pembe
        [SerializeField] private Color secondaryColor = new Color(0.608f, 0.42f, 1f, 1f); // #9B6BFF - Mor
        [SerializeField] private Color accent1Color = new Color(1f, 0.847f, 0.302f, 1f);   // #FFD84D - Sarı
        [SerializeField] private Color accent2Color = new Color(1f, 0.647f, 0.145f, 1f);   // #FFA525 - Turuncu

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // 🔹 YENİ: Renk mapping dictionary
        private Dictionary<string, Color> colorMap;

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
            // 🔹 YENİ: Renk mapping'ini başlat
            InitializeColorMap();
        }

        private void Start()
        {
            InitializeLevelManager();
        }

        // 🔹 YENİ: Renk mapping'ini başlat
        private void InitializeColorMap()
        {
            colorMap = new Dictionary<string, Color>
            {
                { "#FF66CC", primaryColor },   // Pembe -> Primary
                { "#9B6BFF", secondaryColor }, // Mor -> Secondary  
                { "#FFD84D", accent1Color },   // Sarı -> Accent1
                { "#FFA525", accent2Color }    // Turuncu -> Accent2
            };

            DebugLog("[LevelManager] Color mapping initialized");
        }

        // 🔹 YENİ: Hex rengini mapping'den al
        private Color GetMappedColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return Color.white;

            // Önce mapping'den kontrol et
            if (colorMap.ContainsKey(hexColor.ToUpperInvariant()))
            {
                return colorMap[hexColor.ToUpperInvariant()];
            }

            // Mapping'de yoksa hex parse et
            if (ColorUtility.TryParseHtmlString(hexColor, out Color parsed))
            {
                return parsed;
            }

            // Son çare
            return Color.white;
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

            if (data.camera != null)
            {
                cameraHeight       = data.camera.height;
                cameraOffsetX      = data.camera.offsetX;
                cameraOffsetZ      = data.camera.offsetZ;
                cameraPaddingCells = data.camera.padding;
            }

            if (centerCameraAfterLoad) 
            {
                SetupCameraBeforeSpawn(data);
            }

            StartCoroutine(SpawnCubesCoroutine(data));

            OnLevelStarted?.Invoke(CurrentLevel);
        }

        private void SetupCameraBeforeSpawn(LevelData data)
        {
            if (data.cubes == null || data.cubes.Length == 0) return;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            foreach (var c in data.cubes)
            {
                minX = Mathf.Min(minX, c.x);
                maxX = Mathf.Max(maxX, c.x);
                minZ = Mathf.Min(minZ, c.z);
                maxZ = Mathf.Max(maxZ, c.z);
            }

            float cell = GridManager.Instance.CellSize;
            int minSize = 6;
            int levelWidth = Mathf.Max((maxX - minX + 1), minSize);
            int levelHeight = Mathf.Max((maxZ - minZ + 1), minSize);

            float worldWidth = levelWidth * cell;
            float worldHeight = levelHeight * cell;

            Vector3 centerWorld = new(
                (minX + maxX) * 0.5f * cell,
                0f,
                (minZ + maxZ) * 0.5f * cell
            );

            centerWorld.x += cameraOffsetX;
            centerWorld.z += cameraOffsetZ;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                cam.transform.position = new Vector3(centerWorld.x, cameraHeight, centerWorld.z);

                float aspect = cam.aspect;
                float padding = cameraPaddingCells * cell;

                cam.orthographicSize = Mathf.Max(
                    (worldHeight / 2f) + padding,
                    (worldWidth / 2f) / aspect + padding
                );

                DebugLog($"[LevelManager] Camera pre-positioned at: {cam.transform.position}");
            }
        }

        private System.Collections.IEnumerator SpawnCubesCoroutine(LevelData data)
        {
            if (data.cubes == null) yield break;

            var rows = new Dictionary<int, List<(CubeData, Vector3)>>();

            foreach (var c in data.cubes)
            {
                Direction dir = ParseDirection(c.direction);
                Color col = GetMappedColor(c.color); // 🔹 YENİ: Mapping'den renk al
                Vector3Int gridPos = new(c.x, 0, c.z);
                Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);

                var cd = new CubeData(CubeType.Basic, gridPos, dir, col);

                if (!rows.ContainsKey(gridPos.z))
                    rows[gridPos.z] = new List<(CubeData, Vector3)>();

                rows[gridPos.z].Add((cd, worldPos));
            }

            foreach (var row in rows.OrderBy(r => r.Key))
            {
                foreach (var (cd, worldPos) in row.Value)
                {
                    var cube = SpawnCubeAnimated(cd, worldPos);
                    if (cube != null)
                    {
                        spawnedCubes.Add(cube);
                        GridManager.Instance.RegisterObject(cube);
                    }
                }

                yield return new WaitForSeconds(0.12f);
            }
        }

        private Cube SpawnCubeAnimated(CubeData data, Vector3 worldPos)
        {
            var cube = PoolManager.Instance.Get<Cube>();
            if (cube == null) return null;

            cube.Initialize(data);

            Vector3 startPos = worldPos + Vector3.up * 1.2f;

            cube.transform.position = startPos;
            cube.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(cube.transform.DOMove(worldPos, 0.35f).SetEase(Ease.OutQuad));
            seq.Join(cube.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
            seq.SetDelay(UnityEngine.Random.Range(0f, 0.15f));

            return cube;
        }

        private void SpawnCubes(LevelData data)
        {
            if (data.cubes == null) return;

            foreach (var c in data.cubes)
            {
                Direction dir = ParseDirection(c.direction);
                Color col = GetMappedColor(c.color); // 🔹 YENİ: Mapping'den renk al
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

            Vector3 centerWorld = new(
                (minX + maxX) * 0.5f * cell,
                0f,
                (minZ + maxZ) * 0.5f * cell
            );

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