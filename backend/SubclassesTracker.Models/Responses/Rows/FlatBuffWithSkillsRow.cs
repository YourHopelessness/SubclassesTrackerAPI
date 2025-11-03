namespace SubclassesTracker.Models.Responses.Rows
{
    /// <summary>
    /// Flattened representation of a buff with associated skills for a player in a report.
    /// </summary>
    public sealed record FlatBuffWithSkillsRow(
         string ReportCode,
         int PlayerId,
         string PlayerName,
         string PlayerEsoId,
         string PlayerRole,
         string Spec,
         int BuffId,
         string BuffName,
         int? SkillId,
         string? SkillName
    );
}
