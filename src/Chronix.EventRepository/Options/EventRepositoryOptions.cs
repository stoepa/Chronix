using System.ComponentModel;

namespace Chronix.EventRepository.Options;

public class EventRepositoryOptions
{
    public int AutoRevisionAfterNthEvent { get; set; } = -1;
    public VersioningStrategy VersioningStrategy { get; set; } = VersioningStrategy.ReadBackwards;
}

public enum VersioningStrategy
{
    [Description("Will get the first stream, read it backwards to find a revision event, if found will go to the next stream, will do this until latest stream is found")]
    ReadBackwards,
    [Description("A separate index for revisions are kept, will get the latest entry and rebuild from there")]
    SeparateIndex
}
