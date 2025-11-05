namespace SubclassesTracker.Models.Enums
{
    public enum WriteModeEnum
    {
        // Append new file(s) into a partition folder (facts)
        Append,
        // Replace the single target file atomically (dimensions/snapshots)
        ReplaceAll
    }
}
