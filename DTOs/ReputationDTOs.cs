// SWP_BE/DTOs/ReputationRuleDto.cs
public class ReputationRuleDto
{
    public int RuleID { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// SWP_BE/DTOs/AdminDTO/UpdateRuleDto.cs
public class UpdateRuleDto
{
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}