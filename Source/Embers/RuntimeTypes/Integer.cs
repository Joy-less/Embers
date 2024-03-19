using System;
using System.Numerics;
using PeterO.Numbers;

namespace Embers {
    public sealed class Integer : IComparable<Integer> {
        public readonly EInteger Value;

        public Integer(EInteger from_einteger) {
            Value = from_einteger;
        }

        // Casts to Integer
        public static implicit operator Integer(byte FromByte)
            => new(FromByte);
        public static implicit operator Integer(sbyte FromSByte)
            => new(FromSByte);
        public static implicit operator Integer(short FromShort)
            => new(FromShort);
        public static implicit operator Integer(ushort FromUShort)
            => new(FromUShort);
        public static implicit operator Integer(int FromInt)
            => new(FromInt);
        public static implicit operator Integer(uint FromUInt)
            => new(FromUInt);
        public static implicit operator Integer(long FromLong)
            => new(FromLong);
        public static implicit operator Integer(ulong FromULong)
            => new(FromULong);
#if NET7_0_OR_GREATER
        public static implicit operator Integer(Int128 FromInt128)
            => new(EInteger.FromString(FromInt128.ToString()));
        public static implicit operator Integer(UInt128 FromUInt128)
            => new(EInteger.FromString(FromUInt128.ToString()));
#endif
        public static implicit operator Integer(EInteger FromEInteger)
            => new(FromEInteger);
        public static implicit operator Integer(BigInteger FromBigInteger)
            => new(EInteger.FromString(FromBigInteger.ToString()));
        public static explicit operator Integer(Float FromFloat)
            => new((EInteger)(EDecimal)FromFloat);

        // Casts from Integer
        public static explicit operator byte(Integer FromInteger)
            => FromInteger.Value.ToByteChecked();
        public static explicit operator sbyte(Integer FromInteger)
            => FromInteger.Value.ToSByteChecked();
        public static explicit operator short(Integer FromInteger)
            => FromInteger.Value.ToInt16Checked();
        public static explicit operator ushort(Integer FromInteger)
            => FromInteger.Value.ToUInt16Checked();
        public static explicit operator int(Integer FromInteger)
            => FromInteger.Value.ToInt32Checked();
        public static explicit operator uint(Integer FromInteger)
            => FromInteger.Value.ToUInt32Checked();
        public static explicit operator long(Integer FromInteger)
            => FromInteger.Value.ToInt64Checked();
        public static explicit operator ulong(Integer FromInteger)
            => FromInteger.Value.ToUInt64Checked();
#if NET7_0_OR_GREATER
        public static explicit operator Int128(Integer FromInteger)
            => Int128.Parse(FromInteger.Value.ToString());
        public static explicit operator UInt128(Integer FromInteger)
            => UInt128.Parse(FromInteger.Value.ToString());
#endif
        public static explicit operator Half(Integer FromInteger)
            => (Half)(Float)FromInteger.Value;
        public static explicit operator float(Integer FromInteger)
            => (float)(Float)FromInteger.Value;
        public static explicit operator double(Integer FromInteger)
            => (double)(Float)FromInteger.Value;
        public static explicit operator decimal(Integer FromInteger)
            => (decimal)(Float)FromInteger.Value;
        public static implicit operator BigInteger(Integer FromInteger)
            => BigInteger.Parse(FromInteger.Value.ToString());
        public static implicit operator EInteger(Integer FromInteger)
            => FromInteger.Value;

        // Operators
        public static Integer operator +(Integer First, Integer Second)
            => First.Value + Second.Value;
        public static Integer operator -(Integer First, Integer Second)
            => First.Value - Second.Value;
        public static Integer operator *(Integer First, Integer Second)
            => First.Value * Second.Value;
        public static Integer operator /(Integer First, Integer Second)
            => First.Value / Second.Value;
        public static Integer operator %(Integer First, Integer Second)
            => First.Value % Second.Value;
        public Integer Pow(Integer Exponent)
            => Value.Pow(Exponent);
        public Float Pow(Float Exponent) {
            if (((EDecimal)Exponent).IsInteger()) {
                return Value.Pow((EInteger)(EDecimal)Exponent);
            }
            else {
                return Math.Pow((double)(Float)this, (double)Exponent);
            }
        }
        public static Integer operator +(Integer First)
            => First;
        public static Integer operator -(Integer First)
            => -First.Value;
        public static Integer operator ++(Integer First)
            => First.Value + 1;
        public static Integer operator --(Integer First)
            => First.Value - 1;

        // Comparisons
        public static bool operator ==(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) == 0;
        public static bool operator !=(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) != 0;
        public static bool operator <(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) < 0;
        public static bool operator <=(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) <= 0;
        public static bool operator >=(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) >= 0;
        public static bool operator >(Integer First, Integer? Second)
            => First.Value.CompareTo(Second?.Value) > 0;
        public int CompareTo(Integer? Second)
            => Value.CompareTo(Second!);
        public override bool Equals(object? Second)
            => Second is Integer SecondInteger && this == SecondInteger;
        public override int GetHashCode()
            => Value.GetHashCode();

        // Instance Methods
        public override string ToString()
            => Value.ToString();
        public Integer Abs()
            => Value.Abs();

        // Static Methods
        public static Integer Parse(string Value) {
            Value = Value.Trim();
            int IndexOfE = Value.IndexOf('e', StringComparison.InvariantCultureIgnoreCase);
            // Multiply integer by 10**exponent
            if (IndexOfE != -1) {
                EInteger Integer = EInteger.FromString(Value[..IndexOfE]);
                EInteger Exponent = EInteger.FromString(Value[(IndexOfE + 1)..]);
                return Integer * EInteger.Ten.Pow(Exponent);
            }
            // Create integer
            else {
                return EInteger.FromString(Value);
            }
        }
    }
}
