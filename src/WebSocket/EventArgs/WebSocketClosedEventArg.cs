namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct WebSocketClosedEventArg {
    /// <summary>
    /// 
    /// </summary>
    public ulong GuildId { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public int Code { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public string Reason { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public bool ByRemote { get; internal init; }
}