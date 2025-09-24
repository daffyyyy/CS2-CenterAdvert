using CounterStrikeSharp.API.Core;

namespace CS2_CenterAdvert;

public class AdvertConfig : IBasePluginConfig
{
    public int Version { get; set; } = 1;
    public bool RandomOrder { get; set; } = true;
    
    public List<Advert> Adverts { get; set; } =
    [
        new() { Message = "Welcome to the server! 🎮", Enabled = true },
        new() { Message = "Join our Discord: discord.gg/utopiafps", Enabled = true },
        new() { Message = "Support us with a donation ❤️", Enabled = false }
    ];
}

public class Advert
{
    public string Message { get; set; } = "";
    public bool Enabled { get; set; } = true;
}