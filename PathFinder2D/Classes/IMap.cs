using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder {
    
    public interface IMap {
        void Init(Vector2[][] obstacles);
        IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end);
    }
}