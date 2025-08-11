namespace PuzzleGame.Core.Enums
{
    public enum CubeType
    {
        Basic
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    public enum CellType
    {
        Normal,
        Blocked
    }
    
    public enum MoveResult
    {
        Success,
        Failed,
        Blocked,
        OutOfBounds,
        NoMovesLeft
    }
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        LevelComplete,
        LevelFailed
    }
}

