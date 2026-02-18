using Godot;
using System.Collections.Generic;
using System.Linq;
using Remembrance.code;

public partial class GameController : Node2D
{
	[Export] private PackedScene _card;

	[Export] private CardConfig[] _cardConfigs;

	[Export] private CardConfig _targetCardConfig;

	[Export] private BoardController _boardController;

	// configuration
	[Export] private VBoxContainer _configurationContainer;
	[Export] private Button _startButton;

	[Export] private HSlider _difficultySlider;
	[Export] private Label _difficultyLabel;
	
	// Move display
	[Export] private Label _moveLabel;
	
	// Win/Loss
	[Export] private Label _resultsLabel;
	[Export] private Control _resultsRoot;

	private List<CardConfig> _allCards = new List<CardConfig>(9);

	private GameState _gameState = GameState.None;

	private Timer _timer;
	
	public override void _Ready()
	{
		_difficultySlider.ValueChanged += HandleDifficultySliderChanged;
		_difficultyLabel.Text = DifficultyController.Difficulty.ToString();
		
		PrepareGame();
	}

	private void HandleDifficultySliderChanged(double value)
	{
		DifficultyController.Difficulty = (int)value;
		_difficultyLabel.Text = DifficultyController.Difficulty.ToString();
	}

	public override void _Process(double delta)
	{
		if(_boardController.MovesLeft >= 0)
			_moveLabel.Text = $"Moves remaining {_boardController.MovesLeft}"; // meh, use listener to update this
		else
			_moveLabel.Text = "No moves left";
	}

	void PrepareGame()
	{
		SetState(GameState.Preparing);
		
		_allCards.Clear();

		List<CardConfig> badCards = new List<CardConfig>(_cardConfigs);

		for (int i = 0; i < 4; i++)
		{
			int index = GD.RandRange(0, badCards.Count - 1);
			CardConfig toAdd = badCards[index];
			_allCards.Add(toAdd);
			_allCards.Add(toAdd); // two of each
			
			badCards.RemoveAt(index);
		}
		
		_allCards.Add(_targetCardConfig);
		_allCards = new List<CardConfig> (_allCards.OrderBy(x => GD.Randf()));

		foreach (var cardConfig in _allCards)
		{
			GD.Print(cardConfig.Id);
		}

		UpdateResults(Result.None);
	}

	void SetState(GameState state)
	{
		GD.Print($"Setting state {state} current: {_gameState}");
		
		if (_gameState == state)
			return;

		switch (state)
		{
			case GameState.None:
			case GameState.Done:
			case GameState.Preparing:
				_configurationContainer.Visible = true;
				_moveLabel.Visible = false;
				break;
			case GameState.Playing:
				_configurationContainer.Visible = false;
				_moveLabel.Visible = true;
				break;
			default: // shouldn't happen
				_configurationContainer.Visible = false;
				_moveLabel.Visible = true;
				break;
		}

		_gameState = state;
	}

	void StartGame() // Called by event
	{
		if (_gameState == GameState.Done) // this is crap
		{
			PrepareGame();
		}

		SetState(GameState.Playing);
		
		// Board controller will place, move cards etc.
		_boardController.Initialize(_card, _allCards);
	}

	void HandleGameFinishedWon()
	{
		SetState(GameState.Done);
		UpdateResults(Result.Won);
	}

	void HandleGameFinishedLost()
	{
		SetState(GameState.Done);
		UpdateResults(Result.Lost);
	}

	void UpdateResults(Result result)
	{
		_resultsRoot.Visible = _gameState == GameState.Done;

		if (_gameState != GameState.Done) // if not done, we only care about the visibility
			return;
		
		bool won = result == Result.Won;
		_resultsLabel.Text = won ? "SUCCESS" : "FAILURE";

		float greyTone = won ? 1f : 0.6f;
		Color targetColor = new Color(greyTone, greyTone, greyTone, 1f);
		float duration = won ? 0.7f : 1f;
		
		_resultsRoot.Modulate = new Color(1, 1, 1, 0);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(_resultsRoot, "modulate", targetColor, duration);
	}

	enum Result
	{
		None,
		Won,
		Lost,
	}

	public enum GameState
	{
		None,
		Preparing,
		Playing,
		Done,
	}

}
