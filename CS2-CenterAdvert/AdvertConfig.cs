using CounterStrikeSharp.API.Core;

namespace CS2_CenterAdvert;

public class AdvertConfig : IBasePluginConfig
{
    public int Version { get; set; } = 2;
    public bool RandomOrder { get; set; } = true;
    public int Time { get; set; } = 5;
    public bool ExcludeSpectators { get; set; } = true;
    
    public List<Advert> Adverts { get; set; } =
    [
        new() { Message = "Welcome to the server! 🎮", Enabled = true },
        new() { Message = "Join our Discord: discord.gg/utopiafps", Enabled = true },
        new() { Message = "<img src='https://github.com/daffyyyy/CS2-UtopiaFPS_Assets/blob/main/images/deadImages/5.png?raw=true'/>", Enabled = true, Image = true}
    ];
}

public class Advert
{
    public string Message { get; set; } = "";
    public bool Image { get; set; } = false;
    public bool Enabled { get; set; } = true;
}