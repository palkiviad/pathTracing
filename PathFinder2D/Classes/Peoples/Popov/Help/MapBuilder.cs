using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clusters;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public class MapBuilder {


        public static PolygonsContainer Build(Vector2[][] obstacles) {
            Vector2 bottomLeft = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 topRight = new Vector2(float.MinValue, float.MinValue);
            Polygon[] polygons = new Polygon[obstacles.Length];
            for (var i = 0; i < obstacles.Length; i++) {
                var obstacle = obstacles[i];
                var polygon = new Polygon(obstacle);
                if (polygon.MinX < bottomLeft.x) {
                    bottomLeft.x = polygon.MinX;
                }

                if (polygon.MinY < bottomLeft.y) {
                    bottomLeft.y = polygon.MinY;
                }

                if (polygon.MaxX > topRight.x) {
                    topRight.x = polygon.MaxX;
                }

                if (polygon.MaxY > topRight.y) {
                    topRight.y = polygon.MaxY;
                }

                polygons[i] = polygon;
            }

            var result = new PolygonsContainer(bottomLeft, topRight);
            result.InitializeChildren(polygons);
            return result;
        }
    }
}