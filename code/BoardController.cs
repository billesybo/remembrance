using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Remembrance.code;

public partial class BoardController : Control
{
	private List<CardVisuals> _allCards = new List<CardVisuals>();

	[Signal] public delegate void OnGameWonEventHandler();
	[Signal] public delegate void OnGameLostEventHandler();

	
	[Export] private Control[] _cardLocs;

	[Export] private Control _cardsRoot;
	[Export] private Control _discardPile;
	[Export] private DifficultyController _difficultyController;
	
	// Audio (should be elsewhere)
	[Export] private AudioStreamPlayer _cardFlipAudio;
	
	private bool _inputAllowed = true;
	
	private Tween _removeTween;

	private BoardState _state;
	
	private System.Collections.Generic.Dictionary<int, CardVisuals> _locationToCardMapping = new System.Collections.Generic.Dictionary<int, CardVisuals>();

	public int NumberOfMovesTaken
	{
		get;
		private set;
	}

	public int MaxMoves
	{
		get;
		private set;
	}

	public int MovesLeft => MaxMoves - NumberOfMovesTaken;

	public void Initialize(PackedScene cardPrefab, List<CardConfig> cards)
	{
		_state = BoardState.Initializing;
		Clear();
		MaxMoves = _difficultyController.GetMaxMoves();
		
		foreach (CardConfig config in cards)
		{
			CardVisuals card = cardPrefab.Instantiate<CardVisuals>();
			card.Initialize(config);
			card.OnCardClicked += HandleCardClicked;
			
			_cardsRoot.AddChild(card);
			
			_allCards.Add(card);
		}
		
		for (int i = 0; i < 9; i++)
		{
			// GD.Print(_cardLocs[i].GlobalPosition);
			// GD.Print(_cardLocs[i].Position);
			_locationToCardMapping.Add(i, _allCards[i]);
			_allCards[i].GlobalPosition = _cardLocs[i].GlobalPosition;
		}

		//_inputAllowed = true;
		_state = BoardState.WaitingForPlayerInput;
	}

	void Clear()
	{
		Array<Node> toClear = _cardsRoot.GetChildren();
		foreach (var entry in toClear)
		{
			entry.QueueFree();
		}
		_allCards.Clear();
		_locationToCardMapping.Clear();
		NumberOfMovesTaken = 0;
	}

	void HandleCardClicked(CardVisuals card)
	{
		GD.Print($"Card clicked in BoardController!! {card.Id} _state {_state}");

		if (!IsInputAllowed())
			return;

		if (card._showMode == CardVisuals.ShowMode.Front)
			return;

		//_inputAllowed = false;
		_state = BoardState.ShowingFeedback;
		card.Flip(FlipVisualsComplete);
		_cardFlipAudio.Play();
	}

	void FlipVisualsComplete()
	{
		//_inputAllowed = true;
		_state = BoardState.WaitingForPlayerInput;
		UpdateState();
	}

	void UpdateState()
	{
		GD.Print("Update state");
		List<CardVisuals> flippedCards = new List<CardVisuals>(2);
		
		foreach (CardVisuals card in _allCards)
		{
			if (card._showMode == CardVisuals.ShowMode.Front)
			{
				flippedCards.Add(card);
				if (flippedCards.Count > 1)
					break;
			}
		}

		if (flippedCards.Count > 1)
		{
			//_inputAllowed = false;
			_state = BoardState.ShowingFeedback;
			bool removed = CheckRemovePairedCards(flippedCards);
			if (!removed)
			{
				NumberOfMovesTaken++;
			}
		}
	}

	bool CheckRemovePairedCards(List<CardVisuals> flippedCards)
	{
		_removeTween = GetTree().CreateTween();

		if (flippedCards[0].Id == flippedCards[1].Id) // MATCH
		{
			for (int i = 0; i < 2; i++)
			{
				CardVisuals card = flippedCards[i];
				card.Flip(null); // flip back
				card.OnCardClicked -= HandleCardClicked;
				card.SetDiscarded();

				_removeTween.Parallel();
				//_removeTween.TweenInterval(0.1 * i);
				_removeTween.TweenProperty(card, "global_position", _discardPile.GlobalPosition, 1f).SetTrans(Tween.TransitionType.Circ);
			}

			_removeTween.TweenCallback(Callable.From( () => HandleRemoveDone(flippedCards[0], flippedCards[1])));
			return true;
		}
		else
		{
			flippedCards[0].Flip(null);
			flippedCards[1].Flip(null);
			_removeTween.TweenInterval(1);
			_removeTween.TweenCallback(Callable.From(HandleNoRemoveDone));
			return false;
		}
	}

	void HandleRemoveDone(CardVisuals card1, CardVisuals card2)
	{
		GD.Print("Handle remove done");
		RemoveCard(card1);
		RemoveCard(card2);
		
		// _inputAllowed = true;
		bool finished = CheckEndConditions();
		
		if(!finished)
			StartConfusion();
	}


	void HandleNoRemoveDone()
	{
		GD.Print("Handle NO remove done");
		_state = BoardState.WaitingForPlayerInput;
		// _inputAllowed = true;
		bool finished = CheckEndConditions();
		if(!finished)
			StartConfusion();
	}
	
	void RemoveCard(CardVisuals card)
	{
		_allCards.Remove(card);
		foreach (var kvp in _locationToCardMapping)
		{
			if (kvp.Value == card)
			{
				_locationToCardMapping.Remove(kvp.Key);
				return;
			}
		}
	}
	
	bool CheckEndConditions()
	{
		if (_allCards.Count == 1)
		{
			_state = BoardState.DoneSuccess;
			// _inputAllowed = false;
			_allCards[0].SetDiscarded();
			_allCards[0].Flip(DoFinalMoveSuccess);
			return true;
		}

		if (MovesLeft < 0)
		{
			_state = BoardState.DoneFailure;
			FinishFailure();
			return true;
		}

		return false;
	}

	void DoFinalMoveSuccess()
	{
		if (_locationToCardMapping.ContainsKey(4)) // Last card's already centered
		{
			ShowVictory();
		}
		else
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(_allCards[0], "global_position", _cardLocs[4].GlobalPosition, 0.6).SetTrans(Tween.TransitionType.Circ);
			tween.TweenCallback(Callable.From(ShowVictory));
		}
	}

	void FinishFailure()
	{
		GD.Print("LOST! BOOO!");
		EmitSignal("OnGameLost");
	}

	void StartConfusion()
	{
		_state = BoardState.ApplyingConfusion;
		_difficultyController.PrepareConfusion();

		ApplyConfusion();
	}

	void ApplyConfusion()
	{
		if (_difficultyController.TryGetNextEntry(_locationToCardMapping.Keys.ToList(), out ConfusionEntry entry))
		{
			ApplyConfusionEntry(entry);
		}
		else
		{
			_state = BoardState.WaitingForPlayerInput;
		}
	}

	void ApplyConfusionEntry(ConfusionEntry entry)
	{
		GD.Print("MEEEEEH");
		
		var moveElements = entry.GetMoveElements();
		Tween tween = GetTree().CreateTween();
		
		for (int i = 0; i < moveElements.Count; i++)
		{
			MoveElement moveElement = moveElements[i];
			if (_locationToCardMapping.TryGetValue(moveElement.CardPosition, out CardVisuals card))
			{
				//CardVisuals card = _locationToCardMapping[moveElement.CardPosition];
				tween.TweenProperty(card, "global_position", _cardLocs[moveElement.Destination].GlobalPosition,
				moveElement.MoveTime).SetTrans(Tween.TransitionType.Circ);
			}
		}

		tween.TweenCallback(Callable.From(() => FinishConfusionEntry(moveElements)));
	}

	void FinishConfusionEntry(List<MoveElement> moveElements)
	{
		System.Collections.Generic.Dictionary<int, CardVisuals> toAdd = new System.Collections.Generic.Dictionary<int, CardVisuals>();
		
		foreach (MoveElement moveElement in moveElements) // remove from current positions 
		{
			GD.Print($"Removing item at index {moveElement.CardPosition}, will add it to {moveElement.Destination}");
			if (_locationToCardMapping.ContainsKey(moveElement.CardPosition))
			{
				toAdd.Add(moveElement.Destination, _locationToCardMapping[moveElement.CardPosition]);
				_locationToCardMapping.Remove(moveElement.CardPosition);
			}
		}

		foreach (var meh in toAdd)
		{
			_locationToCardMapping[meh.Key] = meh.Value;
		}

		//_state = BoardState.WaitingForPlayerInput;
		ApplyConfusion();
	}

	void ShowVictory()
	{
		GD.Print("VICTORY, ASSHAT!!");
		// raise event, game controller should set state blahblah
		EmitSignal("OnGameWon");
	}

	bool IsInputAllowed()
	{
		return _state == BoardState.WaitingForPlayerInput;
	}

	enum BoardState
	{
		None,
		Initializing,
		WaitingForPlayerInput,
		ShowingFeedback,
		ApplyingConfusion,
		DoneSuccess,
		DoneFailure,
	}

}
