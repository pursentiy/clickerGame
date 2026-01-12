using UnityEngine;

namespace Common.Currency
{
    public readonly struct  HardCurrency : ICurrency
    {
        [SerializeField] private readonly int _value;

        public HardCurrency(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public static HardCurrency Zero => new HardCurrency(0);

        public static HardCurrency operator +(HardCurrency left, HardCurrency right)
        {
            var value = left.Value + right.Value;
            return new HardCurrency(value);
        }

        public static HardCurrency operator -(HardCurrency left, HardCurrency right)
        {
            var value = left.Value - right.Value;
            return new HardCurrency(value);
        }

        public static HardCurrency operator *(HardCurrency left, HardCurrency right)
        {
            var value = left.Value * right.Value;
            return new HardCurrency(value);
        }

        public static HardCurrency operator /(HardCurrency left, HardCurrency right)
        {
            var value = left.Value / right.Value;
            return new HardCurrency(value);
        }

        public static bool operator ==(HardCurrency left, HardCurrency right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(HardCurrency left, HardCurrency right)
        {
            return left.Value != right.Value;
        }

        public static bool operator >(HardCurrency left, HardCurrency right)
        {
            return left.Value > right.Value;
        }

        public static bool operator >=(HardCurrency left, HardCurrency right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(HardCurrency left, HardCurrency right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator <(HardCurrency left, HardCurrency right)
        {
            return left.Value < right.Value;
        }

        public bool Equals(HardCurrency other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is HardCurrency other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator HardCurrency(int spins)
        {
            return new HardCurrency(spins);
        }

        public static implicit operator int(HardCurrency spins)
        {
            return spins.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public long GetCount() => Value;
        
        public ICurrency Add(long value) => new HardCurrency(Value + (int) value);

        public ICurrency Multiply(long value) => new HardCurrency(Value * (int) value);
        
        public ICurrency Multiply(float value) => new HardCurrency((int) (Value * value));
        
        public ICurrency SetCount(long value) => new HardCurrency((int) value);
    }
}