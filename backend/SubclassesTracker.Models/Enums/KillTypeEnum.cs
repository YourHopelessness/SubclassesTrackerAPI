namespace SubclassesTracker.Models.Enums
{
    public enum KillType
    {
        /// <summary>
        /// Include trash and encounters.
        /// </summary>
        All,
        /// <summary>
        /// Only include encounters (kills and wipes).
        /// </summary>
        Encounters,
        /// <summary>
        /// Only include encounters that end in a kill.
        /// </summary>
        Kills,
        /// <summary>
        /// Only include trash.
        /// </summary>
        Trash,
        /// <summary>
        /// Only include encounters that end in a wipe.
        /// </summary>
        Wipes
    }
}
