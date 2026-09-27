namespace Jotwai.Domain.ValidateModel;

public enum TopicDeleteOutcome
{
    Deleted,
    NotFound,
    HasChildTopics,
}
