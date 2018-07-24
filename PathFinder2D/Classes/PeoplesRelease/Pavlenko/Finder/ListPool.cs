using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {
    public class ListPool {

        private readonly Stack<List<Vector2>> pool = new Stack<List<Vector2>>();

        public ListPool() {
            for (int i = 0; i < 10; i++)
                pool.Push(new List<Vector2>(100));
        }

        public List<Vector2> Get() {
            return pool.Count == 0 ? new List<Vector2>(100) : pool.Pop();
        }

        public void Put(List<Vector2> list) {
            list.Clear();
            pool.Push(list);
        }
    }
}