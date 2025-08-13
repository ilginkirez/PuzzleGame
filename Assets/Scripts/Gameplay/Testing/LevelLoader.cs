using UnityEngine;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Grid;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Core.Enums;

public class LevelLoader : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject cubePrefab;         // Cube + Arrow içeren prefab
    [SerializeField] private TextAsset levelJson;           // Resources yerine Inspector’dan da verebilirsin
    [SerializeField] private string resourcesPath = "Levels/level1"; // Resources kullanacaksan

    private void Start()
    {
        var json = levelJson ? levelJson.text : Resources.Load<TextAsset>(resourcesPath)?.text;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Level JSON not found.");
            return;
        }

        var data = JsonUtility.FromJson<LevelData>(json);
        if (data == null)
        {
            Debug.LogError("Level JSON parse failed.");
            return;
        }

        // Grid boyutu (opsiyonel)
        if (data.gridSize != null && data.gridSize.Length == 3)
        {
            // GridManager.gridSize serialized olduğu için doğrudan set etmiyoruz;
            // istersen GridManager’a public init metodu ekleyebilirsin.
            Debug.Log($"Grid target size: {data.gridSize[0]}x{data.gridSize[1]}x{data.gridSize[2]} (info)");
        }

        // Game start
        GameManager.Instance.StartLevel(data.level, data.moves);

        // Spawn
        foreach (var c in data.cubes)
        {
            var go = Instantiate(cubePrefab);
            var cube = go.GetComponent<Cube>();
            if (cube == null)
            {
                Debug.LogError("Cube prefab missing Cube component.");
                Destroy(go);
                continue;
            }

            var dir = (Direction)System.Enum.Parse(typeof(Direction), c.direction);
            var col = Color.white;
            if (ColorUtility.TryParseHtmlString(c.color, out var parsed)) col = parsed;

            var gd = new CubeData(
                CubeType.Basic,
                new Vector3Int(c.x, 0, c.z),
                dir,
                col
            );

            cube.Initialize(gd);
            GridManager.Instance.RegisterObject(cube);
        }
    }
}