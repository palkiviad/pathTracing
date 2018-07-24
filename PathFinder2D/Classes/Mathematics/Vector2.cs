using System;
using System.ComponentModel;

namespace PathFinder.Mathematics {
    
    public struct Vector2 {
        
        public float x;

        public float y;

        private static readonly Vector2 zeroVector = new Vector2(0f, 0f);

        private static readonly Vector2 oneVector = new Vector2(1f, 1f);

        private static readonly Vector2 upVector = new Vector2(0f, 1f);

        private static readonly Vector2 downVector = new Vector2(0f, -1f);

        private static readonly Vector2 leftVector = new Vector2(-1f, 0f);

        private static readonly Vector2 rightVector = new Vector2(1f, 0f);

        private static readonly Vector2 positiveInfinityVector = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        private static readonly Vector2 negativeInfinityVector = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        public const float kEpsilon = 1E-05f;

        // Украдено
        // http://www.java2s.com/Code/CSharp/Development-Class/DistanceFromPointToLine.htm
        public static double DistanceFromPointToLine(Vector2 point, Vector2 lineA, Vector2 lineB)
        {
            // given a line based on two points, and a point away from the line,
            // find the perpendicular distance from the point to the line.
            // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // for explanation and defination.

            float dx = lineB.x - lineA.x;
            float dy = lineB.y - lineA.y;
            
            return Math.Abs(dx*(lineA.y - point.y) - dy*(lineA.x - point.x))/Math.Sqrt(dx*dx + dy*dy);
        }
        
        // Украдено 
        // https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        public static float DistToSegmentSquared(Vector2 p, Vector2 v, Vector2 w) {
            float l2 = SqrDistance(v, w);
            if (l2 == 0) return SqrDistance(v, w);
            var t = ((p.x - v.x) * (w.x - v.x) + (p.y - v.y) * (w.y - v.y)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            return SqrDistance(p, new Vector2(v.x + t * (w.x - v.x), v.y + t * (w.y - v.y)));
        }

        public static float DistToSegment(Vector2 p, Vector2 v, Vector2 w) {
            return (float) Math.Sqrt(DistToSegmentSquared(p, v, w));
        }
        
        // В отличие от метода SegmentToSegmentIntersection задетекит и пересечение граничных точек.
        // Т.е. сегменты [A, B] и [B, C] будут иметь пересечение.
        public static bool SegmentToSegmentIntersectionWithBounds(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 i) {
            float s10_x = p1.x - p0.x;
            float s10_y = p1.y - p0.y;
            float s32_x = p3.x - p2.x;
            float s32_y = p3.y - p2.y;

            var denom = s10_x * s32_y - s32_x * s10_y;
            if (denom == 0)
                return false; // Collinear
            bool denomPositive = denom > 0;

            float s02_x = p0.x - p2.x;
            float s02_y = p0.y - p2.y;
            var s_numer = s10_x * s02_y - s10_y * s02_x;
            if (s_numer < 0 == denomPositive)
                return false; // No collision

            var t_numer = s32_x * s02_y - s32_y * s02_x;
            if (t_numer < 0 == denomPositive)
                return false; // No collision

            if (s_numer >= denom == denomPositive || t_numer >= denom == denomPositive)
                return false; // No collision
            // Collision detected
            var t = t_numer / denom;
            
            i.x = p0.x + t * s10_x;
            i.y = p0.y + t * s10_y;

            return true;
        }
        
        public static bool SegmentToSegmentIntersectionWeak(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 i) {
            return SegmentToSegmentIntersection(p0, p1, p2, p3, ref i, 1e-3f);
        }
        

        // Украдено и чутка дополнено. не будет детектить пересечений конечных точек отрезков!!!
        // Сегменты [A, B] и [B, C] не будут иметь пересечения!
        // 
        // https://stackoverflow.com/questions/563198/whats-the-most-efficent-way-to-calculate-where-two-line-segments-intersect
        // хотя вроде бы где-то у меня был поприличнее алгоритм
        // (Вот бы он ещё работал правильно!)
        // вернёт true, если пересечение имеет место и false в противном случае
        // в i запишутся координаты точки перессечения
        public static bool SegmentToSegmentIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 i, float eps = kEpsilon) {
            float s10_x = p1.x - p0.x;
            float s10_y = p1.y - p0.y;
            float s32_x = p3.x - p2.x;
            float s32_y = p3.y - p2.y;

            var denom = s10_x * s32_y - s32_x * s10_y;
            if (denom == 0)
                return false; // Collinear
            bool denomPositive = denom > 0;

            float s02_x = p0.x - p2.x;
            float s02_y = p0.y - p2.y;
            var s_numer = s10_x * s02_y - s10_y * s02_x;
            if (s_numer < 0 == denomPositive)
                return false; // No collision

            var t_numer = s32_x * s02_y - s32_y * s02_x;
            if (t_numer < 0 == denomPositive)
                return false; // No collision

            if (s_numer > denom == denomPositive || t_numer > denom == denomPositive)
                return false; // No collision
            // Collision detected
            var t = t_numer / denom;

            if ( Math.Abs(t-1) < eps || Math.Abs(t) < eps)
                return false;
            
            i.x = p0.x + t * s10_x;
            i.y = p0.y + t * s10_y;

            return true;
        }
        
        // Возвращает НОВЫЙ вектор, повёрнутый на заданный угол в ГРАДУСАХ
        // Украдено 
        // https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
        // Авось пригодиться
        public Vector2 Rotate(float degrees) {
            float radians = degrees * 0.01745329251994329576923690768489f; //(Math.PI * 2) / 360.;
            float sin = (float)Math.Sin(radians);
            float cos = (float) Math.Cos(radians);
            return new Vector2(cos * x - sin * y, sin * x + cos * y);
        }

        public float this[int index] {
            get {
                float result;
                if (index != 0) {
                    if (index != 1) {
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                    }

                    result = y;
                } else {
                    result = x;
                }

                return result;
            }
            set {
                if (index != 0) {
                    if (index != 1) {
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                    }

                    y = value;
                } else {
                    x = value;
                }
            }
        }

        public Vector2 normalized {
            get {
                Vector2 result = new Vector2(x, y);
                result.Normalize();
                return result;
            }
        }

        public float magnitude { get{ return (float) Math.Sqrt(x * x + y * y);}}

        public float sqrMagnitude {
            get { return x * x + y * y; }
        }

        public static Vector2 zero {
            get { return zeroVector; }
        }

        public static Vector2 one {
            get { return oneVector; }
        }

        public static Vector2 up {
            get { return upVector; }
        }

        public static Vector2 down {
            get { return downVector; }
        }

        public static Vector2 left {
            get { return leftVector; }
        }

        public static Vector2 right {
            get { return rightVector; }
        }

        public static Vector2 positiveInfinity {
            get { return positiveInfinityVector; }
        }

        public static Vector2 negativeInfinity {
            get { return negativeInfinityVector; }
        }

        public Vector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public void Set(float newX, float newY) {
            x = newX;
            y = newY;
        }

        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T> {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) {
            t = Clamp(t, 0, 1);
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t) {
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta) {
            Vector2 a = target - current;
            float magnitude = a.magnitude;
            Vector2 result;
            if (magnitude <= maxDistanceDelta || magnitude == 0f) {
                result = target;
            } else {
                result = current + a / magnitude * maxDistanceDelta;
            }

            return result;
        }

        public static Vector2 Scale(Vector2 a, Vector2 b) {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public void Scale(Vector2 scale) {
            x *= scale.x;
            y *= scale.y;
        }

        public void Normalize() {
            float magnitude = this.magnitude;
            if (magnitude > 1E-05f) {
                this /= magnitude;
            } else {
                this = zero;
            }
        }

        public override string ToString() {
            return x + " " + y;
        }

        public override int GetHashCode() {
            return x.GetHashCode() ^ y.GetHashCode() << 2;
        }

        public override bool Equals(object other) {
            bool result;
            if (!(other is Vector2)) {
                result = false;
            } else {
                Vector2 vector = (Vector2) other;
                result = x.Equals(vector.x) && y.Equals(vector.y);
            }

            return result;
        }

        public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal) {
            return -2f * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector2 Perpendicular(Vector2 inDirection) {
            return new Vector2(-inDirection.y, inDirection.x);
        }

        public static float Dot(Vector2 lhs, Vector2 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public static float Angle(Vector2 from, Vector2 to) {
            return (float) Math.Acos(Clamp(Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
        }

        public static float SignedAngle(Vector2 from, Vector2 to) {
            Vector2 normalized = from.normalized;
            Vector2 normalized2 = to.normalized;
            float num = (float) Math.Acos(Clamp(Dot(normalized, normalized2), -1f, 1f)) * 57.29578f;
            float num2 = Math.Sign(normalized.x * normalized2.y - normalized.y * normalized2.x);
            return num * num2;
        }

        public static float Distance(Vector2 a, Vector2 b) {
            return (a - b).magnitude;
        }

        public static float SqrDistance(Vector2 a, Vector2 b) {
            return (a - b).sqrMagnitude;
        }

        public static Vector2 ClampMagnitude(Vector2 vector, float maxLength) {
            Vector2 result;
            if (vector.sqrMagnitude > maxLength * maxLength) {
                result = vector.normalized * maxLength;
            } else {
                result = vector;
            }

            return result;
        }

        public static float SqrMagnitude(Vector2 a) {
            return a.x * a.x + a.y * a.y;
        }

        public float SqrMagnitude() {
            return x * x + y * y;
        }

        public static Vector2 Min(Vector2 lhs, Vector2 rhs) {
            return new Vector2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
        }

        public static Vector2 Max(Vector2 lhs, Vector2 rhs) {
            return new Vector2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
        }

        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime,
            [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime) {
            smoothTime = Math.Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float d = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            Vector2 vector = current - target;
            Vector2 vector2 = target;
            float maxLength = maxSpeed * smoothTime;
            vector = ClampMagnitude(vector, maxLength);
            target = current - vector;
            Vector2 vector3 = (currentVelocity + num * vector) * deltaTime;
            currentVelocity = (currentVelocity - num * vector3) * d;
            Vector2 vector4 = target + (vector + vector3) * d;
            if (Dot(vector2 - current, vector4 - vector2) > 0f) {
                vector4 = vector2;
                currentVelocity = (vector4 - vector2) / deltaTime;
            }

            return vector4;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b) {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b) {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b) {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        public static Vector2 operator -(Vector2 a) {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator *(Vector2 a, float d) {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator *(float d, Vector2 a) {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator /(Vector2 a, float d) {
            return new Vector2(a.x / d, a.y / d);
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs) {
            return (lhs - rhs).sqrMagnitude < 9.99999944E-11f;
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs) {
            return !(lhs == rhs);
        }
    }
}