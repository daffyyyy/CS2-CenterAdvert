# 📢 CS2-CenterAdvert

A CounterStrikeSharp plugin for Counter-Strike 2 servers that displays customizable advertisements in the center HTML area, supporting both text messages and images.

## ✨ Features

- **Text & Image Ads**: Display both text messages and images in center HTML
- **Flexible Configuration**: Easy setup via config file
- **Random Order**: Option to randomize advertisement display order
- **Spectator Control**: Exclude spectators from seeing advertisements
- **Custom Timing**: Configurable display intervals
- **Enable/Disable**: Toggle individual advertisements on/off

## ⚙️ Configuration Example

```json
{
  "RandomOrder": true, // Randomnize adverts order
  "Time": 5, // Time to switch to next advert
  "ExcludeSpectators": true, // Exclude spectators from seeing ads
  "Adverts": [
    {
      "Message": "Welcome to the server! 🎮",
      "Enabled": true,
      "Image": false
    },
    {
      "Message": "Join our Discord: discord.gg/utopiafps",
      "Enabled": true,
      "Image": false
    },
    {
      "Message": "<img src='https://example.com/image.png'/>",
      "Enabled": true,
      "Image": true // Needed only if any advert is image, if you use only text then disable it in all adverts
    }
  ]
}