using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;

namespace CS2_CenterAdvert;

public sealed class CS2_CenterAdvert : BasePlugin, IPluginConfig<AdvertConfig>
{
    public AdvertConfig Config { get; set; }
    private readonly List<CCSPlayerController> _players = [];
    private List<int> _shuffledIndexes = [];
    private int _currentAdvertIndex = -1;
    
    public override string ModuleName => "CS2-CenterAdvert";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "daffyy";

    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            _players.Clear();
            _shuffledIndexes.Clear();
            _currentAdvertIndex = -1;
            
            var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
            foreach (var player in players)
            {
                    _players.Add(player);
            }
        }
            
        AddTimer(0.1f, Advert, TimerFlags.REPEAT);
        AddTimer(5.0f, RotateAdvert, TimerFlags.REPEAT);
    }

    private void RotateAdvert()
    {
        if (Config?.Adverts == null)
            return;

        var enabledAdverts = Config.Adverts
            .Select((ad, i) => new { ad, i })
            .Where(x => x.ad.Enabled && !string.IsNullOrWhiteSpace(x.ad.Message))
            .ToList();

        if (enabledAdverts.Count == 0)
        {
            _currentAdvertIndex = -1;
            return;
        }

        if (Config.RandomOrder)
        {
            if (_shuffledIndexes.Count == 0)
            {
                _shuffledIndexes = enabledAdverts
                    .Select(x => x.i)
                    .OrderBy(_ => Random.Shared.Next())
                    .ToList();
            }

            _currentAdvertIndex = _shuffledIndexes[0];
            _shuffledIndexes.RemoveAt(0);
        }
        else
        {
            var orderedIndexes = enabledAdverts.Select(x => x.i).ToList();

            if (_currentAdvertIndex == -1)
            {
                _currentAdvertIndex = orderedIndexes[0];
            }
            else
            {
                var current = orderedIndexes.IndexOf(_currentAdvertIndex);
                var next= (current + 1) % orderedIndexes.Count;
                _currentAdvertIndex = orderedIndexes[next];
            }
        }
    }

    private void Advert()
    {
        if (Config?.Adverts == null || Config.Adverts.Count == 0)
            return;

        if (_currentAdvertIndex < 0 || _currentAdvertIndex >= Config.Adverts.Count)
            return;

        var advert = Config.Adverts[_currentAdvertIndex];
        if (!advert.Enabled) return;

        foreach (var player in _players)
        {
            if (player?.IsValid != true) continue;
            if (MenuManager.GetActiveMenu(player) != null) continue;

            if (player.PlayerPawn.Value?.LifeState == (int)LifeState_t.LIFE_DEAD)
                player.PrintToCenterHtml(advert.Message);
        }
    }

    public void OnConfigParsed(AdvertConfig config)
    {
        Config = config;
        RotateAdvert();
        Logger.LogInformation("Loaded {0} adverts from config", config.Adverts?.Count ?? 0);
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
    
}