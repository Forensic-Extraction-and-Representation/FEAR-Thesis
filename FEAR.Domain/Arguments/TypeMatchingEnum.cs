namespace FEAR.Domain.Arguments
{
    /// <summary>
    /// Defines thes method of matching incoming artifacts to scripts in the FEAR system.
    /// </summary>
    public enum TypeMatchingEnum
    {
        // PropertyFallback is used to match based on properties if a 'strict' (Type field) match fails.
        PropertyFallback,
        // Strict is used to only match based on the Type field.
        Strict
    }
}
