namespace SFSSPlatform.Infrastructure.StudyContent;

public sealed record TrustedYouTubeChannelImportResult(
    string SourceFile,
    int ChannelsDiscovered,
    int ChannelsCreated,
    int ChannelsUpdated,
    int ChannelsSkipped);
