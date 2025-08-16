using UnityEngine;
using PuzzleGame.Core.Enums;

using UnityEngine;
using PuzzleGame.Core.Enums;

[System.Serializable]
public class LevelData
{
    public int level;
    public int moves;
    public int[] gridSize; // [x,y,z]
    public CameraData camera; // 👈 kamera ayarları
    public CubeEntry[] cubes;
}

[System.Serializable]
public class CameraData
{
    public float height;
    public float offsetX;
    public float offsetZ;
    public int padding;
}

[System.Serializable]
public class CubeEntry
{
    public int x;
    public int z;                 // y sabit 0 kullanıyoruz
    public string direction;      // "Up","Down","Left","Right"
    public string color;          // "#RRGGBB" veya "#RRGGBBAA"
}
