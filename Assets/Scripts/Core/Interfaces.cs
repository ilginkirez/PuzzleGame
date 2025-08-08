using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Core.Interfaces
{
    public interface IGridObject
    {
        Vector3Int GridPosition { get; set; }
        void SetGridPosition(Vector3Int position);
        Vector3 WorldPosition { get; }
    }
}