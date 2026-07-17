namespace Takayama.GuardCore
{
    /// <summary>
    /// Represents a type with only one value. 
    /// Used to provide a meaningful type for a monadic Result that returns no data.
    /// </summary>
    [Serializable]
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
    {
        private static readonly Unit DefaultValue = new Unit();
        public static ref readonly Unit Default => ref DefaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Unit other) => true;
        public override bool Equals(object? obj) => obj is Unit;
        public override int GetHashCode() => 0;
        public override string ToString() => "()";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Unit other) => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Unit first, Unit second) => true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Unit first, Unit second) => false;
    }

}
