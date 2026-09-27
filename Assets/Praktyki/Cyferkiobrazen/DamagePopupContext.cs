namespace DamageNumbers
{
    /// <summary>
    /// Statyczny kontekst pozwalający opcjonalnie przekazać typ obrażeń
    /// (np. Trucizna, Krwawienie, Krytyk) tuż przed wywołaniem IDamageable.TakeDamage.
    /// </summary>
    public static class DamagePopupContext
    {
        public static DamageType? NextDamageType { get; private set; }
        public static bool? NextIsCrit { get; private set; }

        public static void SetNext(DamageType damageType, bool? isCrit = null)
        {
            NextDamageType = damageType;
            NextIsCrit = isCrit;
        }

        public static (DamageType damageType, bool isCrit) Consume(int damage, int critThreshold = 25)
        {
            DamageType resolvedType;
            bool resolvedCrit;

            if (NextDamageType.HasValue)
            {
                resolvedType = NextDamageType.Value;
                resolvedCrit = NextIsCrit.HasValue 
                    ? NextIsCrit.Value 
                    : (resolvedType == DamageType.Critical);
            }
            else
            {
                resolvedCrit = damage >= critThreshold;
                resolvedType = resolvedCrit ? DamageType.Critical : DamageType.Normal;
            }

            NextDamageType = null;
            NextIsCrit = null;

            return (resolvedType, resolvedCrit);
        }
    }
}
