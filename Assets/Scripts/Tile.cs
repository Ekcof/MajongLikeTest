using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static UnityEngine.ParticleSystem;

namespace Majong.Tiles
{
	public interface ITile
	{
		Collider2D Collider { get; }
		string ID { get; }
		bool HasID { get; }
		void SetInteractable(bool isInteractable);
		void SetConfig(ITileConfig config);
		void Hide();
		void OnSelect();
		void OnUnselect();
		void OnCancel();
		void OnLock();

		void OnHint();
		void OnGet(Action onComplete);
	}

	public class Tile : MonoBehaviour, ITile
	{
		[Inject] private IInputManager _inputManager;
		[SerializeField] private Image _image;
		[SerializeField] private SpriteRenderer _renderer;
		[SerializeField] private Button _button;
		[SerializeField] private Collider2D _collider;
		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField] private ParticleSystem _particles;
		private ITileConfig _config;
		private Vector3 _nativeScale;
		private Tween _fadeTween;
		public string ID => _config?.Name;
		public bool HasID => !string.IsNullOrEmpty(ID);
		public float Width => _renderer.bounds.size.x;
		public Bounds Bounds => _renderer.bounds;
		public Collider2D Collider => _collider;

		private void Awake()
		{
			_nativeScale = transform.localScale;
			_button.SetListener(OnPress);
		}

		public void SetInteractable(bool isInteractable)
		{
			DOTween.Kill(_renderer);
			_button.interactable = isInteractable;
			var newColor = isInteractable ? Color.white : Color.gray;
			_renderer.Recolor(newColor);
		}

		public void Activate()
		{
			DOTween.Kill(_renderer);
			DOTween.Kill(_canvasGroup);
			gameObject.SetActive(true);
			_collider.enabled = true;
			_renderer.enabled = false;
			_particles.gameObject.SetActive(false);
			_canvasGroup.alpha = 0f;
		}

		public void SetConfig(ITileConfig config)
		{
			_config = config;
			_image.sprite = config.Sprite;
			DOTween.Kill(_renderer);
			DOTween.Kill(_canvasGroup);
			gameObject.SetActive(true);
			_canvasGroup.gameObject.SetActive(true);
			transform.localScale = _nativeScale;
			gameObject.SetActive(true);
			_collider.enabled = true;
			_renderer.enabled = true;
			_renderer.color = Color.white;
			_canvasGroup.alpha = 1f;
		}

		public void Hide()
		{
			_config = null;
			_collider.enabled = false;
			_renderer.enabled = false;
			_canvasGroup.gameObject.SetActive(false);
		}

		private void OnPress()
		{
			_inputManager.OnSelect(this);
		}

		#region Animations
		public void OnCancel()
		{
			DOTween.Kill(transform);
			transform.localScale = _nativeScale;
			_renderer.color = Color.red;

			float strength = Width * 0.2f;

			transform.DOPunchPosition(
					punch: new Vector3(0, strength, 0f),
					duration: 0.3f,
					vibrato: 10,
					elasticity: 0.5f
				)
				.SetEase(Ease.Linear).OnComplete(() => _renderer.color = Color.white);
		}

		public void OnLock()
		{
			DOTween.Kill(transform);
			DOTween.Kill(_renderer);
			transform.localScale = _nativeScale;
			_renderer.Recolor(Color.red);
			_button.interactable = false;
		}

		public void OnSelect()
		{
			DOTween.Kill(transform);
			transform.localScale = _nativeScale;
			_renderer.color = Color.green;
		}

		public void OnUnselect()
		{
			DOTween.Kill(_renderer);
			DOTween.Kill(transform);
			transform.localScale = _nativeScale;
			_renderer.color=Color.white;
		}

		public void OnGet(Action onComplete)
		{
			DOTween.Kill(_renderer);
			_particles.gameObject.SetActive(true);
			_particles.Play();
			_renderer.DOFade(0, 0.3f);
			_canvasGroup.DOFade(0, 0.7f).OnComplete(() => onComplete?.Invoke());
			_button.interactable = false;
		}

		public void OnHint()
		{
			float strength = Width * 0.2f;
			transform
				.DOPunchPosition(
					punch: new Vector3(strength, strength, 0f),
					duration: 0.3f,
					vibrato: 10,
					elasticity: 0.5f
				)
			.SetEase(Ease.Linear).OnComplete(() => _renderer.color = Color.white);
		}
		#endregion
		private void OnDestroy()
		{
			DOTween.Kill(transform);
			DOTween.Kill(_renderer);
		}
	}
}