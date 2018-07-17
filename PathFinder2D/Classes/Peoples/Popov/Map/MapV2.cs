using System.Collections.Generic;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clusters;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Popov {
    public class MapV2 : IMap {
        private PolygonsContainer _parentContainer;
        public void Init(Vector2[][] obstacles) {
            _parentContainer = MapBuilder.Build(obstacles);
            
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            
            var result = new List<Vector2>();
            result.Add(start);
            _parentContainer.FindPath(start, end, result);
            return result;
        }
    }
}