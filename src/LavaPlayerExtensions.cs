using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Victoria.Rest.Filters;
using Victoria.Rest.Payloads;

namespace Victoria;

/// <summary>
/// 
/// </summary>
public static class LavaPlayerExtensions {
    internal static ConcurrentDictionary<ulong, LavaQueue<LavaTrack>> Queue { get; } = new();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static LavaQueue<LavaTrack> GetQueue(this LavaPlayer<LavaTrack> player) {
        if (!Queue.ContainsKey(player.GuildId)) {
            Queue.TryAdd(player.GuildId, new LavaQueue<LavaTrack>());
        }
        
        return Queue[player.GuildId];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="lavaTrack"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PlayAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     bool noReplace = true,
                                                                     int volume = default,
                                                                     bool shouldPause = false)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                Volume: volume,
                IsPaused: shouldPause));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="lavaTrack"></param>
    /// <param name="startTime"></param>
    /// <param name="stopTime"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PlayAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     TimeSpan startTime,
                                                                     TimeSpan stopTime,
                                                                     bool noReplace = true,
                                                                     int volume = default,
                                                                     bool shouldPause = false)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                Volume: volume,
                IsPaused: shouldPause,
                Position: startTime.Milliseconds,
                EndTime: stopTime.Milliseconds));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask StopAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     bool noReplace = false,
                                                                     bool shouldPause = true)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: lavaTrack?.Hash,
                IsPaused: shouldPause));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PauseAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                      LavaNode<TLavaPlayer, TLavaTrack> lavaNode)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(
                IsPaused: true));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="lavaTrack"></param>
    /// <returns></returns>
    public static async ValueTask ResumeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                       LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                       TLavaTrack lavaTrack)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                IsPaused: false));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaNode"></param>
    /// <param name="skipAfter"></param>
    /// <param name="lavaPlayer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask<(TLavaTrack Skipped, TLavaTrack Current)> SkipAsync<TLavaPlayer, TLavaTrack>(
        this LavaPlayer<TLavaTrack> lavaPlayer,
        LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
        TimeSpan? skipAfter = default)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        if (!Queue.TryGetValue(lavaPlayer.GuildId, out var queue)) {
            return default;
        }
        
        if (!queue.TryDequeue(out var lavaTrack)) {
            throw new InvalidOperationException("There aren't any more tracks in the Vueue.");
        }
        
        var skippedTrack = lavaPlayer.Track as TLavaTrack;
        await Task.Delay(skipAfter ?? TimeSpan.Zero);
        await PlayAsync(lavaPlayer, lavaNode, (TLavaTrack)lavaTrack);
        
        return (skippedTrack, (TLavaTrack)lavaTrack);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="seekPosition"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask SeekAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TimeSpan seekPosition)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Position: seekPosition.TotalMilliseconds));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="volume"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask SetVolumeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                          LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                          int volume)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Volume: volume));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="equalizerBands"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask EqualizeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                         LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                         params EqualizerBand[] equalizerBands)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Filters: new Filters {
                Bands = equalizerBands
            }));
    }
}