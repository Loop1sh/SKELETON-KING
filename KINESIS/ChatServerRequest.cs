﻿namespace KINESIS;

public class ChatServerRequest
{
    public const short JoinChatChannel = 0x001E;
    public const short SendChatChannelMessage = 0x0003;
    public const short LeaveChatChannel = 0x0022;
    public const short TrackPlayerAction = 0x00B9;
    public const short Connect = 0x0C00;
    public const short CreateMatchmakingGroup = 0x0C0A;
    public const short LeaveMatchmakingGroup = 0x0C0C;
    public const short LeaveMatchmakingQueue = 0x0D02;
    public const short LoadingStatusChanged = 0x0D04;
    public const short EnterMatchmakingQueue = 0x0D05;
    public const short RefreshMatchmakingSettings = 0x0D07;
    public const short RefreshMatchmakingStats = 0x0F07;
}
