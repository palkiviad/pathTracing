using System;
using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {
    public struct Segment {
        public Vector2 Start;
        public Vector2 End;

        public static Segment Create(Vector2 start, Vector2 end) {
            Segment result;
            result.Start = start;
            result.End = end;
            return result;
        }

        internal Vector2 TransposeToZero() {
            return End - Start;
        }

        public static Segment Extend(Segment segment) {
            Segment result = segment;
            result.Start.x += float.Epsilon * Math.Sign(result.Start.x - result.End.x);
            result.Start.y += float.Epsilon * Math.Sign(result.Start.y - result.End.y);
            result.End.x += float.Epsilon * Math.Sign(result.End.x - result.Start.x);
            result.End.y += float.Epsilon * Math.Sign(result.End.y - result.Start.y);
            return result;
        }

        public static Segment Shrink(Segment segment) {
            Segment result = segment;
            result.Start.x -= float.Epsilon * Math.Sign(result.Start.x - result.End.x);
            result.Start.y -= float.Epsilon * Math.Sign(result.Start.y - result.End.y);
            result.End.x -= float.Epsilon * Math.Sign(result.End.x - result.Start.x);
            result.End.y -= float.Epsilon * Math.Sign(result.End.y - result.Start.y);
            return result;
        }


    }
}