using Godot;
using System;
using System.Collections.Generic;
using Remembrance.code;

public partial class DifficultyController : Node // TODO do we want this as a node??
{
	private const int MaxDifficulty = 4;
	private static int _difficulty = 1;
	private int _confusionAmountLeft;

	public static int Difficulty
	{
		get
		{
			return _difficulty;
		}
		set
		{
			_difficulty = Math.Clamp(value, 1, MaxDifficulty);
		}
	}

	public int GetMaxMoves() // TODO some difficulty logic here
	{
		return 7;
	}

	public void PrepareConfusion() // sets up a new confusion set for a move
	{
		// _confusionAmountLeft += GD.RandRange(0, Difficulty * 3);
		_confusionAmountLeft += GD.RandRange(Difficulty - 1, Difficulty * 3);
	}

	public bool TryGetNextEntry(List<int> currentCardEntries, out ConfusionEntry entry)
	{
		GD.Print($"TRY GET NEXT CONFUSION ENTRY {_confusionAmountLeft}");
		
		if (_confusionAmountLeft <= 0)
		{
			entry = null;
			return false;
		}

		ConfusionEntry potentialEntry = GetRandomEntry(currentCardEntries);
		
		if(potentialEntry == null)
		{
			entry = null;
			return false;
		}

		if (potentialEntry.Cost > _confusionAmountLeft)
		{
			entry = null;
			return false;
		}

		_confusionAmountLeft -= potentialEntry.Cost;
		entry = potentialEntry;
		return true;
	}

	private ConfusionEntry GetRandomEntry(List<int> currentCardEntries)
	{
		// TOTAL PLACEHOLDER ... put in some neat, configurable data structure or node whatnot
		int index = GD.RandRange(0, 3);
		switch (index)
		{
			case 0:
				return new SwitchEntry(_difficulty, currentCardEntries);
			case 1:
				return new PushPatternEntry(_difficulty, currentCardEntries);
			case 2:
				return new SPatternEntry(_difficulty, currentCardEntries);
			case 3:
				return new ReverseSPatternEntry(_difficulty, currentCardEntries);
			default:
				return null;
		}
	}
}
