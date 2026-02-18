using System;
using Godot;

namespace Remembrance.code
{
	public partial class CardVisuals : Control
	{
		public event Action<CardVisuals> OnCardClicked;

		public ShowMode _showMode { get; private set; } = ShowMode.Back;

			//[Signal] public delegate void OnCardClickedEventHandler(CardVisuals visuals);

		[Export] private TextureRect _cardBack;
		[Export] private TextureRect _cardFront;
		[Export] private CenterContainer _container;
		[Export] private float _flipTime = 0.5f;
		[Export] private Button _button;

		public string Id { get; private set; }
		private Tween _currentTween;

		private bool _discarded;

		public void Initialize(CardConfig config)
		{
			Initialize(config.Id, config.Texture);
		}

		public void Initialize(string id, Texture2D frontTexture)
		{
			Id = id;
			_cardFront.Texture = frontTexture;

			_showMode = ShowMode.Back;
			SwitchSprites();
		}

		public override void _Ready()
		{
			FadeIn();
		}

		public override void _Process(double delta)
		{
		}

		public void SetDiscarded()
		{
			//_discarded = true;
			_button.Visible = false;
		}

		public void Flip(Action onDoneCallback)
		{
			switch (_showMode)
			{
				case ShowMode.Back:
					TweenShowMode(ShowMode.Front, onDoneCallback);
					break;
				case ShowMode.Front:
					TweenShowMode(ShowMode.Back, onDoneCallback);
					break;
			}
		}

		void TweenShowMode(ShowMode showMode, Action callback)
		{
			KillTween();

			_showMode = showMode;

			_currentTween = GetTree().CreateTween();
			
			_currentTween.TweenProperty(_container, "scale", new Vector2(0f,1f), _flipTime).SetTrans(Tween.TransitionType.Expo);
			_currentTween.TweenCallback(Callable.From(SwitchSprites));
			_currentTween.TweenProperty(_container, "scale", new Vector2(1f,1f), _flipTime).SetTrans(Tween.TransitionType.Expo);
			_currentTween.TweenCallback(Callable.From(callback));
		}

		void FadeIn()
		{
			Tween tween = GetTree().CreateTween();
			_container.Modulate = new Color(1, 1, 1, 0);
			tween.TweenProperty(_container, "modulate", new Color(1, 1, 1, 1), 0.2f).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.In);
		}

		void SwitchSprites()
		{
			bool frontVisible = _showMode == ShowMode.Front;
			_cardBack.Visible = !frontVisible;
			_cardFront.Visible = frontVisible;
		}

		void KillTween()
		{
			if (_currentTween == null)
				return;
			
			_currentTween.Kill();
			_currentTween = null;
		}

		public void HandleCardClicked() // signal called
		{
			GD.Print("CARD GOT CLICK");
			
			OnCardClicked?.Invoke(this);
		}

		public enum ShowMode
		{
			Back,
			Front,
		}
	}
}
