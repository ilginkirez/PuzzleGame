using UnityEngine;
using PuzzleGame.Gameplay.Cubes;

namespace PuzzleGame.Core
{
    public class CubeFactory : MonoBehaviour
    {
        [Header("Cube Prefab")]
        [SerializeField] private GameObject cubeAndArrowPrefab;

        public static CubeFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public GameObject CreateCube(CubeData data)
        {
            if (cubeAndArrowPrefab == null)
            {
                Debug.LogError("Cube prefab not assigned.");
                return null;
            }

            GameObject cubeObj = Instantiate(cubeAndArrowPrefab, data.gridPosition, Quaternion.identity);

            if (cubeObj.TryGetComponent(out Cube cube))
            {
                cube.Initialize(data);
            }
            else
            {
                Debug.LogWarning("Cube prefab does not contain a Cube script.");
            }

            return cubeObj;
        }
    }
}