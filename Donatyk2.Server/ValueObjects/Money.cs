using Donatyk2.Server.Enums;

namespace Donatyk2.Server.ValueObjects
{
    public record Money(decimal Amount, Currency Currency)
    {
        private static void EnsureSameCurrency(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot operate on Money values with different currencies.");
        }

        // binary + and -
        public static Money operator +(Money a, Money b)
        {
            EnsureSameCurrency(a, b);
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            EnsureSameCurrency(a, b);
            return new Money(a.Amount - b.Amount, a.Currency);
        }

        // unary + (no-op) and unary - (negate)
        public static Money operator +(Money a) => a;

        public static Money operator -(Money a) => new Money(-a.Amount, a.Currency);

        // optional: ++ and -- to increment/decrement by one unit
        public static Money operator ++(Money a) => new Money(a.Amount + 1m, a.Currency);
        public static Money operator --(Money a) => new Money(a.Amount - 1m, a.Currency);

        // comparison operators (require same currency)
        public static bool operator <(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            EnsureSameCurrency(a, b);
            return a.Amount < b.Amount;
        }

        public static bool operator >(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            EnsureSameCurrency(a, b);
            return a.Amount > b.Amount;
        }

        public static bool operator <=(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            EnsureSameCurrency(a, b);
            return a.Amount <= b.Amount;
        }

        public static bool operator >=(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            EnsureSameCurrency(a, b);
            return a.Amount >= b.Amount;
        }
    }
}
