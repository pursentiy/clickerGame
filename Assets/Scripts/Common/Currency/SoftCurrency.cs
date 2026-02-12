using UnityEngine;

namespace Common.Currency
{
    public readonly struct SoftCurrency : ICurrency
    {
        [SerializeField] private readonly int _value;

        public SoftCurrency(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public static SoftCurrency Zero => new SoftCurrency(0);

        public static SoftCurrency operator +(SoftCurrency left, SoftCurrency right)
        {
            var value = left.Value + right.Value;
            return new SoftCurrency(value);
        }

        public static SoftCurrency operator -(SoftCurrency left, SoftCurrency right)
        {
            var value = left.Value - right.Value;
            return new SoftCurrency(value);
        }

        public static SoftCurrency operator *(SoftCurrency left, SoftCurrency right)
        {
            var value = left.Value * right.Value;
            return new SoftCurrency(value);
        }

        public static SoftCurrency operator /(SoftCurrency left, SoftCurrency right)
        {
            var value = left.Value / right.Value;
            return new SoftCurrency(value);
        }

        public static bool operator ==(SoftCurrency left, SoftCurrency right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(SoftCurrency left, SoftCurrency right)
        {
            return left.Value != right.Value;
        }

        public static bool operator >(SoftCurrency left, SoftCurrency right)
        {
            return left.Value > right.Value;
        }

        public static bool operator >=(SoftCurrency left, SoftCurrency right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(SoftCurrency left, SoftCurrency right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator <(SoftCurrency left, SoftCurrency right)
        {
            return left.Value < right.Value;
        }

        public bool Equals(SoftCurrency other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SoftCurrency other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator SoftCurrency(int spins)
        {
            return new SoftCurrency(spins);
        }

        public static implicit operator int(SoftCurrency spins)
        {
            return spins.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public long GetCount() => Value;
        
        public ICurrency Add(long value) => new SoftCurrency(Value + (int) value);

        public ICurrency Multiply(long value) => new SoftCurrency(Value * (int) value);
        
        public ICurrency Multiply(float value) => new SoftCurrency((int) (Value * value));
        
        public ICurrency SetCount(long value) => new SoftCurrency((int) value);
    }
}