using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathFinder.Mathematics;

namespace PathFinder2D.Classes.Peoples.Galkin.Map
{
    class Bound
    {
        private float _minX, _minY, _maxX, _maxY;

        public Bound(Vector2[] points)
        {
            for (int i = 0, count = points.Length; i < count; ++i)
            {
                Vector2 curPoint = points[i];
                if (curPoint.x < _minX) {
                    _minX = curPoint.x;
                }
                if (curPoint.x > _maxX)
                {
                    _maxX = curPoint.x;
                }
                if (curPoint.y < _minY)
                {
                    _minY = curPoint.y;
                }
                if (curPoint.y > _maxY)
                {
                    _maxY = curPoint.y;
                }
            }
        }

        /// <summary>
        /// проверка находится ли точка внутри баунда
        /// </summary>
        public bool Contains(Vector2 point) {
            return (point.x >= _minX && point.x <= _maxX && point.y >= _minY && point.y <= _maxY);
        }
    }
}
