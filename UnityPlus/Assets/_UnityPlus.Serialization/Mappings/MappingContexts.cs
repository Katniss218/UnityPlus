
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Also used for any sort of key-value collection.
    /// </summary>
    public static class KeyValueContext
    {
        public const int ValueToValue = 0b00; // default
        public const int ValueToRef = 0b10;

        public const int RefToValue = 0b01;
        public const int RefToRef = 0b11;

        public const int ValueToAsset = 0b1100;
        public const int RefToAsset = 0b1101;
    }

    /// <summary>
    /// Also used for any sort of "flat" collection.
    /// </summary>
    public static class ArrayContext
    {
        public const int Values = 0; // Default
        public const int Refs = 1;
        public const int Assets = 2;
    }
}