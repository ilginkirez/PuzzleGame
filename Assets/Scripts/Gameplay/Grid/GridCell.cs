using UnityEngine;
using System.Collections.Generic;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Gameplay.Grid
{
    public class GridCell
    {
        public Vector3Int Position { get; private set; }
        public bool IsBlocked { get; set; }
        public CellType Type { get; set; }
        
        private List<IGridObject> objects;
        
        public GridCell(Vector3Int position)
        {
            Position = position;
            IsBlocked = false;
            Type = CellType.Normal;
            objects = new List<IGridObject>();
        }
        
        public void AddObject(IGridObject obj)
        {
            if (!objects.Contains(obj))
            {
                objects.Add(obj);
            }
        }
        
        public void RemoveObject(IGridObject obj)
        {
            objects.Remove(obj);
        }
        
        public List<IGridObject> GetObjects()
        {
            return new List<IGridObject>(objects);
        }
        
        public bool HasObjects()
        {
            return objects.Count > 0;
        }
        
        public T GetObject<T>() where T : class, IGridObject
        {
            foreach (var obj in objects)
            {
                if (obj is T)
                    return obj as T;
            }
            return null;
        }
    }
    
    
}