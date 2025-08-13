using UnityEngine;
using UnityEngine.InputSystem;            // YENİ: Keyboard.current
using PuzzleGame.Gameplay.Managers;      // MoveManager için
using PuzzleGame.Gameplay.Cubes;         // Cube tipi için
using PuzzleGame.Core.Enums;             // Direction için

public class MoveTest_NewInput : MonoBehaviour
{
    [SerializeField] private Cube testCube;

    void Update()
    {
        if (testCube == null || Keyboard.current == null) return;

        // wasPressedThisFrame = sadece bu frame’de basıldı mı
        if (Keyboard.current.wKey.wasPressedThisFrame)
            MoveManager.Instance.RequestMove(testCube, Direction.Up);

        if (Keyboard.current.sKey.wasPressedThisFrame)
            MoveManager.Instance.RequestMove(testCube, Direction.Down);

        if (Keyboard.current.aKey.wasPressedThisFrame)
            MoveManager.Instance.RequestMove(testCube, Direction.Left);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            MoveManager.Instance.RequestMove(testCube, Direction.Right);
    }
    
    
}