using System;
using System.Numerics;
using PeterO.Numbers;

namespace Embers {
    public sealed class Float : IComparable<Float> {
        public readonly EDecimal Value;

        public static readonly Float Infinity = new(EDecimal.PositiveInfinity);
        public static readonly Float NaN = new(EDecimal.NaN);

        public Float(EDecimal from_edecimal) {
            Value = from_edecimal;
        }

        // Casts to Float
        public static implicit operator Float(byte FromByte)
            => new(FromByte);
        public static implicit operator Float(sbyte FromSByte)
            => new(FromSByte);
        public static implicit operator Float(short FromShort)
            => new(FromShort);
        public static implicit operator Float(ushort FromUShort)
            => new(FromUShort);
        public static implicit operator Float(int FromInt)
            => new(FromInt);
        public static implicit operator Float(uint FromUInt)
            => new(FromUInt);
        public static implicit operator Float(long FromLong)
            => new(FromLong);
        public static implicit operator Float(ulong FromULong)
            => new(FromULong);
#if NET7_0_OR_GREATER
        public static implicit operator Float(Int128 FromInt128)
            => new(EInteger.FromString(FromInt128.ToString()));
        public static implicit operator Float(UInt128 FromUInt128)
            => new(EInteger.FromString(FromUInt128.ToString()));
#endif
        public static implicit operator Float(Half FromHalf)
            => new(EInteger.FromString(FromHalf.ToString()));
        public static implicit operator Float(float FromFloat)
            => new(EDecimal.FromString(FromFloat.ToString()));
        public static implicit operator Float(double FromDouble)
            => new(EDecimal.FromString(FromDouble.ToString()));
        public static implicit operator Float(decimal FromDecimal)
            => new(FromDecimal);
        public static implicit operator Float(EInteger FromEInteger)
            => new(FromEInteger);
        public static implicit operator Float(EDecimal FromEDecimal)
            => new(FromEDecimal);
        public static explicit operator Float(EFloat FromEFloat)
            => new(FromEFloat.ToEDecimal());
        public static explicit operator Float(ERational FromERational)
            => new(FromERational.ToEDecimal());
        public static implicit operator Float(BigInteger FromBigInteger)
            => new(EDecimal.FromString(FromBigInteger.ToString()));
        public static implicit operator Float(Integer FromInteger)
            => new((EInteger)FromInteger);

        // Casts from Float
        public static explicit operator byte(Float FromFloat)
            => FromFloat.Value.ToByteChecked();
        public static explicit operator sbyte(Float FromFloat)
            => FromFloat.Value.ToSByteChecked();
        public static explicit operator short(Float FromFloat)
            => FromFloat.Value.ToInt16Checked();
        public static explicit operator ushort(Float FromFloat)
            => FromFloat.Value.ToUInt16Checked();
        public static explicit operator int(Float FromFloat)
            => FromFloat.Value.ToInt32Checked();
        public static explicit operator uint(Float FromFloat)
            => FromFloat.Value.ToUInt32Checked();
        public static explicit operator long(Float FromFloat)
            => FromFloat.Value.ToInt64Checked();
        public static explicit operator ulong(Float FromFloat)
            => FromFloat.Value.ToUInt64Checked();
#if NET7_0_OR_GREATER
        public static explicit operator Int128(Float FromFloat)
            => Int128.Parse(FromFloat.Value.ToString());
        public static explicit operator UInt128(Float FromFloat)
            => UInt128.Parse(FromFloat.Value.ToString());
#endif
        public static explicit operator Half(Float FromFloat)
            => (Half)(float)FromFloat.Value;
        public static explicit operator float(Float FromFloat)
            => FromFloat.Value.ToSingle();
        public static explicit operator double(Float FromFloat)
            => FromFloat.Value.ToDouble();
        public static explicit operator decimal(Float FromFloat)
            => FromFloat.Value.ToDecimal();
        public static explicit operator BigInteger(Float FromFloat)
            => BigInteger.Parse(FromFloat.Value.ToString());
        public static implicit operator EDecimal(Float FromFloat)
            => FromFloat.Value;

        // Operators
        public static Float operator +(Float First, Float Second)
            => First.Value + Second.Value;
        public static Float operator -(Float First, Float Second)
            => First.Value - Second.Value;
        public static Float operator *(Float First, Float Second)
            => First.Value * Second.Value;
        public static Float operator /(Float First, Float Second)
            => First.Value / Second.Value;
        public static Float operator %(Float First, Float Second)
            => First.Value % Second.Value;
        public Float Pow(Float Exponent) {
            Float Result = Value.Pow(Exponent);
            if (Result.Value.IsNaN()) {
                Result = Math.Pow((double)this, (double)Exponent);
            }
            return Result;
        }
        public static Float operator +(Float First)
            => First;
        public static Float operator -(Float First)
            => -First.Value;

        // Comparisons
        public static bool operator ==(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) == 0;
        public static bool operator !=(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) != 0;
        public static bool operator <(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) < 0;
        public static bool operator <=(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) <= 0;
        public static bool operator >=(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) >= 0;
        public static bool operator >(Float First, Float? Second)
            => First.Value.CompareTo(Second?.Value) > 0;
        public int CompareTo(Float? Second)
            => Value.CompareTo(Second!);
        public override bool Equals(object? Second)
            => Second is Float SecondFloat && this == SecondFloat;
        public override int GetHashCode()
            => Value.GetHashCode();

        // Instance Methods
        public override string ToString()
            => Value.ToString();
        public string ToPlainString()
            => Value.ToPlainString();
        public bool IsInteger()
            => Value.IsInteger();
        public bool IsNaN()
            => Value.IsNaN();
        public bool IsPositiveInfinity()
            => Value.IsPositiveInfinity();
        public bool IsNegativeInfinity()
            => Value.IsNegativeInfinity();
        public Float Abs()
            => Value.Abs();
        public Integer Floor()
            => (Integer)(Float)Value.RoundToExponent(EInteger.Zero, ERounding.Floor);
        public Integer Ceil()
            => (Integer)(Float)Value.RoundToExponent(EInteger.Zero, ERounding.Ceiling);
        public Integer Round()
            => (Integer)(Float)Value.RoundToExponent(EInteger.Zero, ERounding.HalfUp);
        public Integer Truncate()
            => (Integer)(Float)Value.RoundToExponent(EInteger.Zero, ERounding.Down);

        // Static Methods
        public static Float Parse(string Value) {
            Value = Value.Trim();
            int IndexOfE = Value.IndexOf('e', StringComparison.InvariantCultureIgnoreCase);
            // Multiply decimal by 10**exponent
            if (IndexOfE != -1) {
                EDecimal Integer = EDecimal.FromString(Value[..IndexOfE]);
                EDecimal Exponent = EDecimal.FromString(Value[(IndexOfE + 1)..]);
                return Integer * EDecimal.Ten.Pow(Exponent);
            }
            // Create decimal
            else {
                return EDecimal.FromString(Value);
            }
        }
    }
}
