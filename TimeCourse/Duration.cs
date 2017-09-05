using System;

namespace TimeCourse
{
    /// <summary>
    /// 時間幅を表すオブジェクト
    /// </summary>
    public class Duration
    {
        public TimeSpan value { get; set; }
        public long microseconds { get { return value.Ticks; } set { this.value = TimeSpan.FromTicks(value); } }
        public double milliseconds { get { return value.TotalMilliseconds; } set { this.value = TimeSpan.FromMilliseconds(value); } }
        public double seconds { get { return value.TotalSeconds; } set { this.value = TimeSpan.FromSeconds(value); } }

        public Duration(TimeSpan value)
        {
            this.value = value;
        }
        public Duration(long value) : this(TimeSpan.FromMilliseconds(value)) { }
        public Duration(double value) : this(TimeSpan.FromMilliseconds(value)) { }

        public static Duration operator +(Duration lhs, Duration rhs)
        {
            return new Duration(lhs.value + rhs.value);
        }
        public static Duration operator -(Duration lhs, Duration rhs)
        {
            return new Duration(lhs.value - rhs.value);
        }
        public static Duration operator *(Duration lhs, int rhs)
        {
            return new Duration(TimeSpan.FromTicks(lhs.value.Ticks * rhs));
        }
        public static Duration operator *(int lhs, Duration rhs)
        {
            return rhs * lhs;
        }
        public static Duration operator *(Duration lhs, double rhs)
        {
            return new Duration(TimeSpan.FromTicks((long)(lhs.value.Ticks * rhs)));
        }
        public static Duration operator *(double lhs, Duration rhs)
        {
            return rhs * lhs;
        }
        public static Duration operator /(Duration lhs, int rhs)
        {
            return new Duration(TimeSpan.FromTicks(lhs.value.Ticks / rhs));
        }
        public static Duration operator /(Duration lhs, double rhs)
        {
            return new Duration(TimeSpan.FromTicks((long)(lhs.value.Ticks / rhs)));
        }

        public static bool operator ==(Duration lhs, Duration rhs)
        {
            return lhs.value == rhs.value;
        }
        public static bool operator !=(Duration lhs, Duration rhs)
        {
            return !(lhs == rhs);
        }
        public static bool operator <=(Duration lhs, Duration rhs)
        {
            return lhs.value <= rhs.value;
        }
        public static bool operator >=(Duration lhs, Duration rhs)
        {
            return lhs.value >= rhs.value;
        }
        public static bool operator <(Duration lhs, Duration rhs)
        {
            return lhs.value < rhs.value;
        }
        public static bool operator >(Duration lhs, Duration rhs)
        {
            return lhs.value > rhs.value;
        }

        public override bool Equals(object rhs)
        {
            return base.Equals(rhs);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
