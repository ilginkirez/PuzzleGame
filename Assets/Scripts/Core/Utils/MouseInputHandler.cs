using UnityEngine;
using PuzzleGame.Core.Interfaces;

public class MouseInputHandler : IInputHandler
{
    public bool IsInputDown() => Input.GetMouseButtonDown(0);
    public Vector3 GetInputPosition() => Input.mousePosition;
}


