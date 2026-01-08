namespace Common.Currency
{
    public interface ICurrency
    {
        long GetCount();
        ICurrency Add(long value);
        ICurrency Multiply(long value);
        ICurrency Multiply(float value);
        ICurrency SetCount(long value);
    }
}