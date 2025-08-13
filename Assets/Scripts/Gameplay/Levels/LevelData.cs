using UnityEngine;
using PuzzleGame.Core.Enums;

[System.Serializable]
public class LevelData
{
    public int level;
    public int moves;
    public int[] gridSize; // [x,y,z]
    public CubeEntry[] cubes;
}

[System.Serializable]
public class CubeEntry
{
    public int x;
    public int z;                 // y sabit 0 kullanıyoruz
    public string direction;      // "Up","Down","Left","Right"
    public string color;          // "#RRGGBB" veya "#RRGGBBAA"
}