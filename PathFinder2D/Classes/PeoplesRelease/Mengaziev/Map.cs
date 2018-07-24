using System.Collections.Generic;
using PathFinder.Mathematics;


namespace PathFinder.Release.Mengaziev_1
{
    public sealed class Map : IMap
    {
        Mengaziev.Map inplementation = new Mengaziev.Map(false, false);

        public void Init(Vector2[][] obstacles)
        {
            inplementation.Init(obstacles);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            return inplementation.GetPath(start, end);
        }

        public IEnumerable<Vector2> GetDebug()
        {
            return inplementation.GetDebug();
        }

    }
}

namespace PathFinder.Release.Mengaziev_2
{
    public sealed class Map : IMap
    {
        Mengaziev.Map inplementation = new Mengaziev.Map(true, false);

        public void Init(Vector2[][] obstacles)
        {
            inplementation.Init(obstacles);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            return inplementation.GetPath(start, end);
        }

        public IEnumerable<Vector2> GetDebug()
        {
            return inplementation.GetDebug();
        }
    }
}

namespace PathFinder.Release.Mengaziev_3
{
    public sealed class Map : IMap
    {
        Mengaziev.Map inplementation = new Mengaziev.Map(true, true);

        public void Init(Vector2[][] obstacles)
        {
            inplementation.Init(obstacles);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            return inplementation.GetPath(start, end);
        }

        public IEnumerable<Vector2> GetDebug()
        {
            return inplementation.GetDebug();
        }
    }
}
