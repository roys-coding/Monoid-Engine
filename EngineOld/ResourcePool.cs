using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Numerics;
using MyDataTypes;
using System.Reflection.Metadata;

namespace MyMonoGameApp;

public static class ResourcePool
{
    public const string RESOURCE_NAMES_FILE_NAME = "resource_names.json";

    private static ResourcePoolInfo _contentInfo;
    private static Dictionary<string, Texture2D> _loadedTextures = new();
    private static Dictionary<string, Effect> _loadedEffects = new();
    private static Dictionary<string, SpriteSheetInfo> _loadedSpriteSheets = new();
    private static Dictionary<string, SpriteFont> _loadedFonts = new();

    public static void LoadContent(ContentManager contentManager)
    {
        _loadedTextures.Add("static", contentManager.Load<Texture2D>("textures/static"));
        _loadedTextures.Add("main_menu", contentManager.Load<Texture2D>("textures/main_menu"));
        _loadedTextures.Add("office", contentManager.Load<Texture2D>("textures/office/office"));
        _loadedTextures.Add("flash_left", contentManager.Load<Texture2D>("textures/office/flash_left"));
        _loadedTextures.Add("flash_right", contentManager.Load<Texture2D>("textures/office/flash_right"));
        _loadedTextures.Add("fan", contentManager.Load<Texture2D>("textures/office/fan"));
        _loadedTextures.Add("door", contentManager.Load<Texture2D>("textures/office/door"));
        _loadedTextures.Add("buttons", contentManager.Load<Texture2D>("textures/office/buttons"));
        _loadedTextures.Add("bonnie_leave", contentManager.Load<Texture2D>("textures/office/bonnie_leave"));
        _loadedTextures.Add("bonnie_leave_flash", contentManager.Load<Texture2D>("textures/office/bonnie_leave_flash"));
        _loadedTextures.Add("bonnie_office", contentManager.Load<Texture2D>("textures/office/bonnie_office"));
        _loadedTextures.Add("bonnie_flash", contentManager.Load<Texture2D>("textures/office/bonnie_flash"));
        _loadedEffects.Add("perspective", contentManager.Load<Effect>("effects/perspective"));

        _loadedSpriteSheets.Add("main_menu", new() { ID = "main_menu", FrameWidth = 1280, FrameHeight = 720, TextureID = "main_menu" });
        _loadedSpriteSheets.Add("office", new() { ID = "office", FrameWidth = 1600, FrameHeight = 720, TextureID = "office" });
        _loadedSpriteSheets.Add("flash_left", new() { ID = "flash_left", FrameWidth = 451, FrameHeight = 720, TextureID = "flash_left" });
        _loadedSpriteSheets.Add("flash_right", new() { ID = "flash_right", FrameWidth = 432, FrameHeight = 720, TextureID = "flash_right" });
        _loadedSpriteSheets.Add("static", new() { ID = "static", FrameWidth = 1280, FrameHeight = 720, TextureID = "static" });
        _loadedSpriteSheets.Add("fan", new() { ID = "fan", FrameWidth = 122, FrameHeight = 156, TextureID = "fan" });
        _loadedSpriteSheets.Add("door", new() { ID = "door", FrameWidth = 229, FrameHeight = 720, TextureID = "door" });
        _loadedSpriteSheets.Add("buttons", new() { ID = "buttons", FrameWidth = 62, FrameHeight = 174, TextureID = "buttons" });
        _loadedSpriteSheets.Add("bonnie_leave", new() { ID = "bonnie_leave", FrameWidth = 430, FrameHeight = 720, TextureID = "bonnie_leave" });
        _loadedSpriteSheets.Add("bonnie_flash", new() { ID = "bonnie_flash", FrameWidth = 164, FrameHeight = 676, TextureID = "bonnie_flash" });
        _loadedSpriteSheets.Add("bonnie_leave_flash", new() { ID = "bonnie_leave_flash", FrameWidth = 164, FrameHeight = 720, TextureID = "bonnie_leave_flash" });
        _loadedSpriteSheets.Add("bonnie_office", new() { ID = "bonnie_office", FrameWidth = 389, FrameHeight = 720, TextureID = "bonnie_office" });
    }

    public static Texture2D GetTexture2D(string ID)
    {
        if (!_loadedTextures.ContainsKey(ID))
        {
            throw new InvalidOperationException($"Texture '{ID}' not found.");
        }

        return _loadedTextures[ID];
    }

    public static Effect GetEffect(string ID)
    {
        if (!_loadedEffects.ContainsKey(ID))
        {
            throw new InvalidOperationException($"Effect '{ID}' not found.");
        }

        return _loadedEffects[ID];
    }

    public static SpriteSheetInfo GetSpriteSheetInfo(string ID)
    {
        if (!_loadedSpriteSheets.ContainsKey(ID))
        {
            throw new InvalidOperationException($"SpriteSheetInfo '{ID}' not found.");
        }

        return _loadedSpriteSheets[ID];
    }

    public static SpriteFont GetFont(string ID)
    {
        if (!_loadedFonts.ContainsKey(ID))
        {
            throw new InvalidOperationException($"SpriteSheetInfo '{ID}' not found.");
        }

        return _loadedFonts[ID];
    }

    public static Sprite CreateSprite(string textureID, Transform2D transform)
    {
        Texture2D texture = GetTexture2D(textureID);
        return new(texture, transform);
    }

    public static SpriteSheet CreateSpriteSheet(string spriteSheetID, Transform2D transform)
    {
        SpriteSheetInfo info = GetSpriteSheetInfo(spriteSheetID);
        Texture2D texture = GetTexture2D(info.TextureID);
        return new(texture, info.FrameWidth, info.FrameHeight, transform);
    }
}
