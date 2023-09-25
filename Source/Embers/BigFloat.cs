using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

#nullable enable

namespace Embers
{
    /*
    Copyright (C) 2014 Patrick Demian

    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    of the Software, and to permit persons to whom the Software is furnished to do
    so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
    THE SOFTWARE.

    Significant modifications made by Joyless.
    */

    [Serializable]
    public struct BigFloat : IComparable, IComparable<BigFloat>, IEquatable<BigFloat>
    {
        private BigInteger Numerator;
        private BigInteger Denominator;

        public static readonly BigFloat One = new(1);
        public static readonly BigFloat Zero = new(0);
        public static readonly BigFloat MinusOne = new(-1);
        public static readonly BigFloat OneHalf = new(1, 2);

        public readonly int Sign => Numerator.Sign + Denominator.Sign switch {
            2 or -2 => 1,
            0 => -1,
            _ => 0,
        };

        // Constructors
        public BigFloat() {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        public BigFloat(string value) {
            BigFloat BigFloat = Parse(value);
            Numerator = BigFloat.Numerator;
            Denominator = BigFloat.Denominator;
        }
        public BigFloat(BigInteger numerator, BigInteger denominator) {
            Numerator = numerator;
            if (denominator == 0)
                throw new ArgumentException("Denominator equals 0");
            Denominator = BigInteger.Abs(denominator);
        }
        public BigFloat(BigInteger value) {
            Numerator = value;
            Denominator = BigInteger.One;
        }
        public BigFloat(BigFloat? value) {
            Numerator = value.HasValue ? value.Value.Numerator : BigInteger.Zero;
            Denominator = value.HasValue ? value.Value.Denominator : BigInteger.One;
        }
        public BigFloat(ulong value) {
            Numerator = new BigInteger(value);
            Denominator = BigInteger.One;
        }
        public BigFloat(long value) {
            Numerator = new BigInteger(value);
            Denominator = BigInteger.One;
        }
        public BigFloat(uint value) {
            Numerator = new BigInteger(value);
            Denominator = BigInteger.One;
        }
        public BigFloat(int value) {
            Numerator = new BigInteger(value);
            Denominator = BigInteger.One;
        }
        public BigFloat(float value) : this(value.ToString("N99")) { }
        public BigFloat(double value) : this(value.ToString("N99")) { }
        public BigFloat(decimal value) : this(value.ToString("N99")) { }

        // Non-static methods
        public BigFloat Add(BigFloat? other) {
            if (!other.HasValue)
                throw new ArgumentNullException(nameof(other));
            Numerator = Numerator * other.Value.Denominator + other.Value.Numerator * Denominator;
            Denominator *= other.Value.Denominator;
            return this;
        }
        public BigFloat Subtract(BigFloat? other) {
            if (!other.HasValue)
                throw new ArgumentNullException(nameof(other));
            Numerator = Numerator * other.Value.Denominator - other.Value.Numerator * Denominator;
            Denominator *= other.Value.Denominator;
            return this;
        }
        public BigFloat Multiply(BigFloat? other) {
            if (!other.HasValue)
                throw new ArgumentNullException(nameof(other));
            Numerator *= other.Value.Numerator;
            Denominator *= other.Value.Denominator;
            return this;
        }
        public BigFloat Divide(BigFloat? other) {
            if (!other.HasValue)
                throw new ArgumentNullException(nameof(other));
            if (other.Value.Numerator == 0)
                throw new DivideByZeroException();
            Numerator *= other.Value.Denominator;
            Denominator *= other.Value.Numerator;
            return this;
        }
        public BigFloat Remainder(BigFloat? other) {
            if (!other.HasValue)
                throw new ArgumentNullException(nameof(other));
            // b = a % n
            // remainder = a - floor(a/n) * n
            BigFloat Result = this - Floor(this / other.Value) * other.Value;
            Numerator = Result.Numerator;
            Denominator = Result.Denominator;
            return this;
        }
        public BigFloat DivideRemainder(BigFloat Other, out BigFloat Rem) {
            Divide(Other);
            Rem = Remainder(this, Other);
            return this;
        }
        public BigFloat Pow(int exponent) {
            if (Numerator.IsZero) {
                // Nothing to do
            }
            else if (exponent < 0) {
                BigInteger SavedNumerator = Numerator;
                Numerator = BigInteger.Pow(Denominator, -exponent);
                Denominator = BigInteger.Pow(SavedNumerator, -exponent);
            }
            else {
                Numerator = BigInteger.Pow(Numerator, exponent);
                Denominator = BigInteger.Pow(Denominator, exponent);
            }
            return this;
        }
        public BigFloat Abs() {
            Numerator = BigInteger.Abs(Numerator);
            return this;
        }
        public BigFloat Negate() {
            Numerator = BigInteger.Negate(Numerator);
            return this;
        }
        public BigFloat Inverse() {
            (Numerator, Denominator) = (Denominator, Numerator);
            return this;
        }
        public BigFloat Increment() {
            Numerator += Denominator;
            return this;
        }
        public BigFloat Decrement() {
            Numerator -= Denominator;
            return this;
        }
        public BigFloat Ceil() {
            if (Numerator < 0)
                Numerator -= BigInteger.Remainder(Numerator, Denominator);
            else
                Numerator += Denominator - BigInteger.Remainder(Numerator, Denominator);

            Factor();
            return this;
        }
        public BigFloat Floor() {
            if (Numerator < 0)
                Numerator += Denominator - BigInteger.Remainder(Numerator, Denominator);
            else
                Numerator -= BigInteger.Remainder(Numerator, Denominator);

            Factor();
            return this;
        }
        public BigFloat Round() {
            // Get remainder. Ceil if greater than 0.5, floor otherwise.
            BigFloat Value = Decimals(this);
            if (Value.CompareTo(OneHalf) >= 0)
                Ceil();
            else
                Floor();

            return this;
        }
        public BigFloat Truncate() {
            Numerator -= BigInteger.Remainder(Numerator, Denominator);
            Factor();
            return this;
        }
        public readonly BigFloat Decimals() {
            BigInteger result = BigInteger.Remainder(Numerator, Denominator);
            return new BigFloat(result, Denominator);
        }
        public BigFloat ShiftDecimalLeft(int Shift) {
            if (Shift < 0)
                return ShiftDecimalRight(-Shift);
            Numerator *= BigInteger.Pow(10, Shift);
            return this;
        }
        public BigFloat ShiftDecimalRight(int Shift) {
            if (Shift < 0)
                return ShiftDecimalLeft(-Shift);
            Denominator *= BigInteger.Pow(10, Shift);
            return this;
        }
        public readonly double Sqrt() {
            return Math.Pow(10, BigInteger.Log10(Numerator) / 2) / Math.Pow(10, BigInteger.Log10(Denominator) / 2);
        }
        public readonly double Log10() {
            return BigInteger.Log10(Numerator) - BigInteger.Log10(Denominator);
        }
        public readonly double Log(double BaseValue) {
            return BigInteger.Log(Numerator, BaseValue) - BigInteger.Log(Numerator, BaseValue);
        }
        public override string ToString() {
            // Default precision is 100
            return ToString(100);
        }
        public string ToString(int Precision, bool TrailingZeros = false) {
            Factor();

            BigInteger Result = BigInteger.DivRem(Numerator, Denominator, out BigInteger Remainder);

            if (Remainder == 0 && TrailingZeros)
                return Result + ".0";
            else if (Remainder == 0)
                return Result.ToString();


            BigInteger Decimals = (Numerator * BigInteger.Pow(10, Precision)) / Denominator;

            if (Decimals == 0 && TrailingZeros)
                return Result + ".0";
            else if (Decimals == 0)
                return Result.ToString();

            StringBuilder DecimalPlacesString = new();

            while (Precision-- > 0 && Decimals > 0) {
                DecimalPlacesString.Append(Decimals % 10);
                Decimals /= 10;
            }

            if (TrailingZeros)
                return Result + "." + new string(DecimalPlacesString.ToString().Reverse().ToArray());
            else
                return Result + "." + new string(DecimalPlacesString.ToString().Reverse().ToArray()).TrimEnd('0');
        }
        public string ToMixString() {
            Factor();

            BigInteger Result = BigInteger.DivRem(Numerator, Denominator, out BigInteger Remainder);

            if (Remainder == 0)
                return Result.ToString();
            else
                return Result + ", " + Remainder + "/" + Denominator;
        }

        public string ToRationalString() {
            Factor();
            return Numerator + " / " + Denominator;
        }
        public readonly int CompareTo(BigFloat Other) {
            // Make copies
            BigInteger One = Numerator;
            BigInteger Two = Other.Numerator;
            // Cross multiply
            One *= Other.Denominator;
            Two *= Denominator;
            // Test
            return BigInteger.Compare(One, Two);
        }
        public readonly int CompareTo(object? Other) {
            if (Other == null)
                throw new ArgumentNullException(nameof(Other));
            else if (Other is not BigFloat)
                throw new ArgumentException("Other is not a BigFloat", nameof(Other));
            return CompareTo((BigFloat)Other);
        }
        public override readonly bool Equals(object? Other) {
            if (Other == null || GetType() != Other.GetType()) {
                return false;
            }
            return Numerator == ((BigFloat)Other).Numerator && Denominator == ((BigFloat)Other).Denominator;
        }
        public readonly bool Equals(BigFloat Other) {
            return Other.Numerator == Numerator && Other.Denominator == Denominator;
        }

        // Static methods
        public static new bool Equals(object? Left, object? Right) {
            if (Left is BigInteger LeftInteger && Right is BigInteger RightInteger) {
                return LeftInteger.Equals(RightInteger);
            }
            else if (Left == null && Right == null) {
                return true;
            }
            return false;
        }
        public static string ToString(BigFloat value) {
            return value.ToString();
        }
        public override readonly int GetHashCode() {
            return base.GetHashCode();
        }

        public static BigFloat Inverse(BigFloat value) {
            return new BigFloat(value).Inverse();
        }
        public static BigFloat Decrement(BigFloat value) {
            return new BigFloat(value).Decrement();
        }
        public static BigFloat Negate(BigFloat value) {
            return new BigFloat(value).Negate();
        }
        public static BigFloat Increment(BigFloat value) {
            return new BigFloat(value).Increment();
        }
        public static BigFloat Abs(BigFloat value) {
            return new BigFloat(value).Abs();
        }
        public static BigFloat Add(BigFloat left, BigFloat right) {
            return new BigFloat(left).Add(right);
        }
        public static BigFloat Subtract(BigFloat left, BigFloat right) {
            return new BigFloat(left).Subtract(right);
        }
        public static BigFloat Multiply(BigFloat left, BigFloat right) {
            return new BigFloat(left).Multiply(right);
        }
        public static BigFloat Divide(BigFloat left, BigFloat right) {
            return new BigFloat(left).Divide(right);
        }
        public static BigFloat Pow(BigFloat value, int exponent) {
            return (new BigFloat(value)).Pow(exponent);
        }
        public static BigFloat Remainder(BigFloat left, BigFloat right) {
            return new BigFloat(left).Remainder(right);
        }
        public static BigFloat DivideRemainder(BigFloat left, BigFloat right, out BigFloat remainder) {
            return new BigFloat(left).DivideRemainder(right, out remainder);
        }
        public static BigFloat Decimals(BigFloat value) {
            return value.Decimals();
        }
        public static BigFloat Truncate(BigFloat value) {
            return new BigFloat(value).Truncate();
        }
        public static BigFloat Ceil(BigFloat value) {
            return new BigFloat(value).Ceil();
        }
        public static BigFloat Floor(BigFloat value) {
            return new BigFloat(value).Floor();
        }
        public static BigFloat Round(BigFloat value) {
            return new BigFloat(value).Round();
        }
        public static BigFloat Parse(string value) {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            value = value.Trim();
            value = value.Replace(",", "");
            int pos = value.IndexOf('.');
            value = value.Replace(".", "");

            if (pos < 0) {
                // No decimal point
                BigInteger Numerator = BigInteger.Parse(value);
                return new BigFloat(Numerator).Factor();
            }
            else {
                // Decimal point (length - pos - 1)
                BigInteger Numerator = BigInteger.Parse(value);
                BigInteger Denominator = BigInteger.Pow(10, value.Length - pos);

                return new BigFloat(Numerator, Denominator).Factor();
            }
        }
        public static BigFloat ShiftDecimalLeft(BigFloat value, int shift) {
            return new BigFloat(value).ShiftDecimalLeft(shift);
        }
        public static BigFloat ShiftDecimalRight(BigFloat value, int shift) {
            return new BigFloat(value).ShiftDecimalRight(shift);
        }
        public static bool TryParse(string value, out BigFloat? result) {
            try {
                result = Parse(value);
                return true;
            }
            catch (ArgumentNullException) {
                result = null;
                return false;
            }
            catch (FormatException) {
                result = null;
                return false;
            }
        }
        public static int Compare(BigFloat left, BigFloat right) {
            if (Equals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (Equals(right, null))
                throw new ArgumentNullException(nameof(right));

            return new BigFloat(left).CompareTo(right);
        }
        public static double Log10(BigFloat value) {
            return new BigFloat(value).Log10();
        }
        public static double Log(BigFloat value, double baseValue) {
            return new BigFloat(value).Log(baseValue);
        }
        public static double Sqrt(BigFloat value) {
            return new BigFloat(value).Sqrt();
        }

        public static BigFloat operator -(BigFloat value) {
            return new BigFloat(value).Negate();
        }
        public static BigFloat operator -(BigFloat left, BigFloat right) {
            return new BigFloat(left).Subtract(right);
        }
        public static BigFloat operator --(BigFloat value) {
            return value.Decrement();
        }
        public static BigFloat operator +(BigFloat left, BigFloat right) {
            return new BigFloat(left).Add(right);
        }
        public static BigFloat operator +(BigFloat value) {
            return new BigFloat(value).Abs();
        }
        public static BigFloat operator ++(BigFloat value) {
            return value.Increment();
        }
        public static BigFloat operator %(BigFloat left, BigFloat right) {
            return new BigFloat(left).Remainder(right);
        }
        public static BigFloat operator *(BigFloat left, BigFloat right) {
            return new BigFloat(left).Multiply(right);
        }
        public static BigFloat operator /(BigFloat left, BigFloat right) {
            return new BigFloat(left).Divide(right);
        }
        public static BigFloat operator >>(BigFloat value, int shift) {
            return new BigFloat(value).ShiftDecimalRight(shift);
        }
        public static BigFloat operator <<(BigFloat value, int shift) {
            return new BigFloat(value).ShiftDecimalLeft(shift);
        }
        public static BigFloat operator ^(BigFloat left, int right) {
            return new BigFloat(left).Pow(right);
        }
        public static BigFloat operator ~(BigFloat value) {
            return new BigFloat(value).Inverse();
        }

        public static bool operator !=(BigFloat left, BigFloat right) {
            return Compare(left, right) != 0;
        }
        public static bool operator ==(BigFloat left, BigFloat right) {
            return Compare(left, right) == 0;
        }
        public static bool operator <(BigFloat left, BigFloat right) {
            return Compare(left, right) < 0;
        }
        public static bool operator <=(BigFloat left, BigFloat right) {
            return Compare(left, right) <= 0;
        }
        public static bool operator >(BigFloat left, BigFloat right) {
            return Compare(left, right) > 0;
        }
        public static bool operator >=(BigFloat left, BigFloat right) {
            return Compare(left, right) >= 0;
        }

        public static bool operator true(BigFloat value) {
            return value != 0;
        }
        public static bool operator false(BigFloat value) {
            return value == 0;
        }

        public static explicit operator decimal(BigFloat value) {
            if (decimal.MinValue > value) throw new System.OverflowException("value is less than System.decimal.MinValue.");
            if (decimal.MaxValue < value) throw new System.OverflowException("value is greater than System.decimal.MaxValue.");

            return (decimal)value.Numerator / (decimal)value.Denominator;
        }
        public static explicit operator double(BigFloat value) {
            if (double.MinValue > value) throw new System.OverflowException("value is less than System.double.MinValue.");
            if (double.MaxValue < value) throw new System.OverflowException("value is greater than System.double.MaxValue.");

            return (double)value.Numerator / (double)value.Denominator;
        }
        public static explicit operator float(BigFloat value) {
            if (float.MinValue > value) throw new System.OverflowException("value is less than System.float.MinValue.");
            if (float.MaxValue < value) throw new System.OverflowException("value is greater than System.float.MaxValue.");

            return (float)value.Numerator / (float)value.Denominator;
        }

        public static implicit operator BigFloat(byte value) {
            return new BigFloat((uint)value);
        }
        public static implicit operator BigFloat(sbyte value) {
            return new BigFloat((int)value);
        }
        public static implicit operator BigFloat(short value) {
            return new BigFloat((int)value);
        }
        public static implicit operator BigFloat(ushort value) {
            return new BigFloat((uint)value);
        }
        public static implicit operator BigFloat(int value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(long value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(uint value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(ulong value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(decimal value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(double value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(float value) {
            return new BigFloat(value);
        }
        public static implicit operator BigFloat(BigInteger value) {
            return new BigFloat(value);
        }
        public static explicit operator BigFloat(string value) {
            return new BigFloat(value);
        }

        public static explicit operator BigInteger(BigFloat value) {
            return value.Factor().Numerator;
        }

        private BigFloat Factor() {
            // Factoring can be very slow. So use only when necessary (ToString and comparisons)

            if (Denominator == 1)
                return this;

            // Factor Numerator and Denominator
            BigInteger factor = BigInteger.GreatestCommonDivisor(Numerator, Denominator);

            Numerator /= factor;
            Denominator /= factor;

            return this;
        }
    }
}
