using System.Collections.Generic;
using Godot;

namespace Remembrance.code
{
    public abstract class ConfusionEntry
    {
        public virtual int Cost => 1;

        private int _difficulty;

        protected virtual float GetMoveTime()
        {
            switch (_difficulty)
            {
                case 1:
                    return 0.6f;
                case 2:
                    return 0.5f;
                case 3:
                    return 0.4f;
                case 4:
                    return 0.3f;
                default:
                    return 0.5f;
            }
        }

        public ConfusionEntry(int difficulty, List<int> currentCardEntries) // TODO kill difficulty??
        {
            _difficulty = difficulty;
        }

        public virtual List<MoveElement> GetMoveElements()
        {
            return new List<MoveElement>();
        }
        
        protected MoveElement GetMoveElement(int position, int destination)
        {
            return new MoveElement()
            {
                CardPosition = position % 9,
                Destination = destination % 9,
                MoveTime = GetMoveTime()
            };
        }
    }

    public class SwitchEntry : ConfusionEntry
    {
        private List<MoveElement> _moveElements = new List<MoveElement>();
        
        public SwitchEntry(int difficulty, List<int> currentCardEntries) : base(difficulty, currentCardEntries)
        {
            if (currentCardEntries.Count < 2)
                return;

            List<int> activeCardEntries = new List<int>(currentCardEntries);

            int index = GD.RandRange(0, activeCardEntries.Count - 1);
            int first = activeCardEntries[index];
            activeCardEntries.RemoveAt(index);
            
            index = GD.RandRange(0, activeCardEntries.Count - 1);
            int second = activeCardEntries[index];
            float moveTime = GetMoveTime();

            MoveElement firstMove = new MoveElement()
            {
                CardPosition = first,
                Destination = second,
                MoveTime = moveTime,
            };
            _moveElements.Add(firstMove);

            MoveElement secondMove = new MoveElement()
            {
                CardPosition = second,
                Destination = first,
                MoveTime = moveTime,
            };
            _moveElements.Add(secondMove);
        }

        public override List<MoveElement> GetMoveElements()
        {
            return _moveElements;
        }
    }

    public class PushPatternEntry : ConfusionEntry
    {
        public override int Cost => 3;

        private List<MoveElement> _moveElements = new List<MoveElement>();

        public PushPatternEntry(int difficulty, List<int> currentCardEntries) : base(difficulty, currentCardEntries)
        {
            float moveTime = GetMoveTime();

            for (int i = 0; i < 9; i++)
            {
                MoveElement firstMove = new MoveElement()
                {
                    CardPosition = i % 9,
                    Destination = (i + 1) % 9,
                    MoveTime = moveTime,
                };
                _moveElements.Add(firstMove);
            }
        }
        
        public override List<MoveElement> GetMoveElements()
        {
            return _moveElements;
        }
    }

    public class SPatternEntry : ConfusionEntry
    {
        public override int Cost => 4;

        private List<MoveElement> _moveElements = new List<MoveElement>();

        public SPatternEntry(int difficulty, List<int> currentCardEntries) : base(difficulty, currentCardEntries)
        {
            _moveElements.Add(GetMoveElement(0, 1));
            _moveElements.Add(GetMoveElement(1, 2));
            _moveElements.Add(GetMoveElement(2, 5));
            _moveElements.Add(GetMoveElement(5, 4));
            _moveElements.Add(GetMoveElement(4, 3));
            _moveElements.Add(GetMoveElement(3, 6));
            _moveElements.Add(GetMoveElement(6, 7));
            _moveElements.Add(GetMoveElement(7, 8));
            _moveElements.Add(GetMoveElement(8, 0));
        }

        public override List<MoveElement> GetMoveElements()
        {
            return _moveElements;
        }
    }

    public class ReverseSPatternEntry : ConfusionEntry
    {
        public override int Cost => 4;

        private List<MoveElement> _moveElements = new List<MoveElement>();

        public ReverseSPatternEntry(int difficulty, List<int> currentCardEntries) : base(difficulty, currentCardEntries)
        {
            _moveElements.Add(GetMoveElement(0, 8));
            _moveElements.Add(GetMoveElement(8, 7));
            _moveElements.Add(GetMoveElement(7, 6));
            _moveElements.Add(GetMoveElement(6, 3));
            _moveElements.Add(GetMoveElement(3, 4));
            _moveElements.Add(GetMoveElement(4, 5));
            _moveElements.Add(GetMoveElement(5, 2));
            _moveElements.Add(GetMoveElement(2, 1));
            _moveElements.Add(GetMoveElement(1, 0));
        }

        public override List<MoveElement> GetMoveElements()
        {
            return _moveElements;
        }
    }
    
    


    public class MoveElement
    {
        public int CardPosition;
        public int Destination;

        public float MoveTime;
    }
}