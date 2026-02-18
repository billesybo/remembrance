using System;
using Godot;

namespace Remembrance.code;

public partial class CardConfig : Node2D
{
    [Export] private string _id;
    [Export] private Texture2D _texture2D;

    public string Id => _id;
    public Texture2D Texture => _texture2D;

}