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
    
    public interface IMovable
    {
        bool CanMove(Direction direction);
        void Move(Direction direction, System.Action<MoveResult> onComplete = null);
        bool IsMoving { get; }
        Vector3Int GridPosition { get; set; }
    }

    public interface IClickable
    {
        void OnClick();
        bool IsClickable { get; }
    }
}