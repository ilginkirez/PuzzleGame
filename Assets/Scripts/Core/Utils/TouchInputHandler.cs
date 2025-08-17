using UnityEngine;
using PuzzleGame.Core.Interfaces;

public class TouchInputHandler : IInputHandler
{
    public bool IsInputDown()
    {
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    public Vector3 GetInputPosition() => Input.GetTouch(0).position;
}