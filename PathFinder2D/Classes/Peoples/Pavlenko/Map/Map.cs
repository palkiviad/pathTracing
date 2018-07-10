using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {

    public sealed class Map : IMap {

        public void Init(Vector2[][] obstacles) {

        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            return new List<Vector2>{start, end};
        }
    }
}