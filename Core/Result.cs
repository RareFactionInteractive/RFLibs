namespace RFLibs.Core
{
    using System;

    namespace BEGiN.Core
    {
        public class Result<T, TE> : IEquatable<Result<T, TE>>
        {
            public bool IsOk { get; }
            public bool IsErr => !IsOk;

            public T Ok { get; }
            public TE Err { get; }

            public Result(T okValue, TE errValue, bool isOk)
            {
                Ok = okValue;
                Err = errValue;
                IsOk = isOk;
            }

            public static Result<T, TE> OK(T value) => new(value, default!, true);
            public static Result<T, TE> Error(TE error) => new(default!, error, false);

            public bool Equals(Result<T, TE> other)
            {
                if (ReferenceEquals(other, null)) return false;
                if (ReferenceEquals(this, other)) return true;

                if (IsOk && other.IsOk)
                    return Equals(Ok, other.Ok); // Safe null check
                if (IsErr && other.IsErr)
                    return Equals(Err, other.Err); // Safe null check

                return false;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                return obj is Result<T, TE> result && Equals(result);
            }

            public override int GetHashCode()
            {
                return IsOk
                    ? (Ok?.GetHashCode() ?? 0) ^ 397 // Safe null handling
                    : (Err?.GetHashCode() ?? 0);
            }

            public static bool operator ==(Result<T, TE> left, Result<T, TE> right)
            {
                if (ReferenceEquals(left, right)) return true;
                if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
                return left.Equals(right);
            }

            public static bool operator !=(Result<T, TE> left, Result<T, TE> right) => !(left == right);
        }
    }
}