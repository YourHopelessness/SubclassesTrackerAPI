namespace SubclassesTracker.Models.Dto
{
    public sealed record SkillInfo(
        string SkillName,
        string SkillLine,
        string SkillType,
        string? UrlIcon,
        string? ClassName);
}
