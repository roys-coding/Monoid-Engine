using FontStashSharp;
using FontStashSharp.RichText;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine
{
    public class FontTestScene : Scene
    {
        FontSystem _fontSystem = new();
        string _text = "Hello world!";
        float _fontSize = 12;
        float _fontSizeBuffer = 12;
        ImTextureID _atlasTextureID;
        Texture2D _atlasTexture;

        public override void Initialize()
        {
            _fontSystem.AddFont(File.ReadAllBytes(@"Content\fonts\consola.ttf"));

            base.Initialize();

            DImGui.OnDraw += (s, a) =>
            {
                ImGui.Begin("Text Test");

                ImGui.InputText("Redered text", ref _text, 512);
                ImGui.InputFloat("Font size", ref _fontSizeBuffer);
                bool applyPressed = ImGui.Button("Apply");
                if (applyPressed) _fontSize = _fontSizeBuffer;
                bool resetPressed = ImGui.Button("Reset atlas");
                if (resetPressed) _fontSystem.CurrentAtlas.Reset(1024, 1024);

                ImGui.End();

                if (!_atlasTextureID.IsNull)
                {
                    ImGui.Begin("Font atlas");
                    ImGui.Text($"{_atlasTexture.Width} x {_atlasTexture.Height}");
                    ImGui.Image(_atlasTextureID, new(1024f, 1024f));

                    var imageTL = ImGui.GetItemRectMin();
                    var imageBR = ImGui.GetItemRectMax();
                    var drawList = ImGui.GetWindowDrawList();
                    drawList.AddRect(imageTL, imageBR, 0xFFFF0000);

                    ImGui.End();
                }
            };
        }

        public override void Terminate()
        {
            base.Terminate();
        }

        public override void BuildScene()
        {
            base.BuildScene();
        }

        public override void Update()
        {
            base.Update();

        }

        public override void Draw()
        {
            Graphics.Begin(null);

            if (_fontSystem.Atlases.Count > 0 && _atlasTexture == null)
            {
                _atlasTexture = _fontSystem.Atlases[0].Texture;
                _atlasTextureID = DImGui.Renderer.BindTexture(_atlasTexture);
            }

            DynamicSpriteFont font = _fontSystem.GetFont(_fontSize);
            RichTextLayout rich = new()
            {
                Font = font,
                Text = _text
            };
            rich.Draw(Graphics.SpriteBatch, Vector2.Zero, Color.White);

            Graphics.End();

            base.Draw();
        }
    }
}
