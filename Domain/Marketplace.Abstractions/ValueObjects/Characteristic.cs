namespace Marketplace
{
    public class Characteristic
    {
        public string Key { get; }
        public string Value { get; }

        public Characteristic(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

            Key = key;
            Value = value;
        }
    }
}