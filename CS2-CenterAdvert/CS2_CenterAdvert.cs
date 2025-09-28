using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2_CenterAdvert;

public sealed class CS2_CenterAdvert : BasePlugin, IPluginConfig<AdvertConfig>
{
    public AdvertConfig Config { get; set; }
    private readonly HashSet<CCSPlayerController> _players = [];
    
    private List<Advert> _enabledAds = [];
    private List<Advert> _shuffledAds = [];
    private Advert? _currentAdvert;

    private Timer? _timer;
    
    public override string ModuleName => "CS2-CenterAdvert";
    public override string ModuleVersion => "1.0.3";
    public override string ModuleAuthor => "daffyy";

    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            _players.Clear();
            _shuffledAds.Clear();
            
            var players = Utilities.GetPlayers()
                .Where(p => p.IsValid && !p.IsBot)
                .Where(p => !Config.ExcludeSpectators || p.TeamNum > 1)
                .ToList();
            
            foreach (var player in players)
            {
                _players.Add(player);
            }
        }
        
        if (Config.Adverts.Any(a => a.Image))
            RegisterListener<Listeners.OnTick>(Advert);
        else
            AddTimer(0.1f, Advert, TimerFlags.REPEAT);

        _timer = StartRotateTimer();
    }

    private void RotateAdvert()
    {
        var next = GetNextAdvert();
        if (next != null)
            _currentAdvert = next;
    }

    private void Advert()
    {
        if (_currentAdvert == null)
            return;

        foreach (var player in _players)
        {
            if (player?.IsValid != true) continue;
            if (MenuManager.GetActiveMenu(player) != null) continue;

            if (Config.ExcludeAlive && player.PlayerPawn.Value?.LifeState != (int)LifeState_t.LIFE_DEAD)
                continue;
            
            player.PrintToCenterHtml(_currentAdvert.Message);
        }
    }

    public void OnConfigParsed(AdvertConfig config)
    {
        Config = config;
        _enabledAds = GetEnabledAdverts();
        _currentAdvert = null;
        
        RotateAdvert();
        Logger.LogInformation("Loaded {0} adverts from config", config.Adverts?.Count ?? 0);
    }

    [ConsoleCommand("css_centeradvert")]
    [CommandHelper(minArgs: 1, usage: "[start] / [force advert_index]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/cheats")]
    public void OnCenterAdvertCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var command = commandInfo.GetArg(1);
        switch (command)
        {
            case "force":
                if (int.TryParse(commandInfo.GetArg(2), out var index))
                {
                    if (index >= 0 && index < _enabledAds.Count)
                    {
                        _timer?.Kill();
                        _timer = null;
                        commandInfo.ReplyToCommand($"You forced an advertisement with content: {_enabledAds[index].Message}");
                        _currentAdvert = _enabledAds[index];
                    }
                }
                break;
            case "start":
            {
                _timer ??= StartRotateTimer();
                commandInfo.ReplyToCommand($"You have enabled changing advertisements");
            }
            break;
        }
    }

    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;
        
        _players.Add(player);
        
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;
        
        _players.Remove(player);
        
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult EventPlayerTeam(EventPlayerTeam @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;
        
        if (Config.ExcludeSpectators && @event.Team <= 1)
            _players.Remove(player);
        else
            _players.Add(player);
        
        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        RemoveListener<Listeners.OnTick>(Advert);
    }

    private List<Advert> GetEnabledAdverts()
    {
        return Config.Adverts
            .Where(ad => ad.Enabled && !string.IsNullOrWhiteSpace(ad.Message))
            .ToList();
    }
    
    private Advert? GetNextAdvert()
    {
        if (_enabledAds.Count == 0)
            return null;

        if (Config.RandomOrder)
        {
            if (_shuffledAds.Count == 0)
            {
                _shuffledAds = _enabledAds
                    .OrderBy(_ => Random.Shared.Next())
                    .ToList();
            }

            var advert = _shuffledAds[0];
            _shuffledAds.RemoveAt(0);
            return advert;
        }

        if (_currentAdvert == null)
            _currentAdvert = _enabledAds[0];
        else
        {
            var current = _enabledAds.IndexOf(_currentAdvert);
            var next = (current + 1) % _enabledAds.Count;
            _currentAdvert = _enabledAds[next];
        }

        return _currentAdvert;
    }

    private Timer StartRotateTimer()
    {
        return AddTimer(Config.Time, RotateAdvert, TimerFlags.REPEAT);
    }
}