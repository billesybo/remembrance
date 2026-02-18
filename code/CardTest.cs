using Godot;
using System;
using System.Net.Mime;
using Remembrance.code;

public partial class CardTest : Node
{
	[Export] private CardVisuals _testCard;
	[Export] private Texture2D _testMeh;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_testCard.Initialize("meh", _testMeh);
		_testCard.Flip(Callback);
	}

	void Callback()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
