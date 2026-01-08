namespace Common.Currency
{
    public readonly struct Stars : ICurrency
    {
        public Stars(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static Stars Zero => new Stars(0);

        public static Stars operator +(Stars left, Stars right)
        {
            var value = left.Value + right.Value;
            return new Stars(value);
        }

        public static Stars operator -(Stars left, Stars right)
        {
            var value = left.Value - right.Value;
            return new Stars(value);
        }

        public static Stars operator *(Stars left, Stars right)
        {
            var value = left.Value * right.Value;
            return new Stars(value);
        }

        public static Stars operator /(Stars left, Stars right)
        {
            var value = left.Value / right.Value;
            return new Stars(value);
        }

        public static bool operator ==(Stars left, Stars right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(Stars left, Stars right)
        {
            return left.Value != right.Value;
        }

        public static bool operator >(Stars left, Stars right)
        {
            return left.Value > right.Value;
        }

        public static bool operator >=(Stars left, Stars right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(Stars left, Stars right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator <(Stars left, Stars right)
        {
            return left.Value < right.Value;
        }

        public bool Equals(Stars other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Stars other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator Stars(int spins)
        {
            return new Stars(spins);
        }

        public static implicit operator int(Stars spins)
        {
            return spins.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public long GetCount() => Value;
        
        public ICurrency Add(long value) => new Stars(Value + (int) value);

        public ICurrency Multiply(long value) => new Stars(Value * (int) value);
        
        public ICurrency Multiply(float value) => new Stars((int) (Value * value));
        
        public ICurrency SetCount(long value) => new Stars((int) value);
    }
}