namespace Jotwai.Domain.RequestModels;

public sealed class AssignNoteTopicRequest
{
    /// <summary>The Topic to move the Note into, or null to clear it. Must already exist (ticket 036).</summary>
    public Guid? TopicId { get; set; }
}
