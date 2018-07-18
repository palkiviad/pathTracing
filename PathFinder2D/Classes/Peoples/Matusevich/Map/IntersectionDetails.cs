namespace PathFinder.Matusevich
{
    public struct IntersectionDetails
    {
        public Obstacle Obstacle;
        public float SquaredDistanceFromStart;

        public static IntersectionDetails CreateEmpty()
        {
            IntersectionDetails result;
            result.Obstacle = null;
            result.SquaredDistanceFromStart = float.MaxValue;
            return result;
        }
    }
}