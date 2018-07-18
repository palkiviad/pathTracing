using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Arkhipov 
{
    public sealed class Map : IMap
    {
        private readonly IMap _impl;

        public Map()
        {
            _impl = new DumbRayPathfinder();
        }

        public void Init(Vector2[][] obstacles) 
        {
            _impl.Init(obstacles);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) 
        {
            return _impl.GetPath(start, end);
        }
    }
}