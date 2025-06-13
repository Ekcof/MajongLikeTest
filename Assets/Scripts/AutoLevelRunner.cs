using Cysharp.Threading.Tasks;
using Majong.Tiles;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

public interface IAutoLevelRunner
{
	bool IsAutoleveling { get; }
	void StartAutoLevel();
}

public class AutoLevelRunner : MonoBehaviour, IAutoLevelRunner
{
	private const float AUTO_LEVEL_DELAY = 0.5f;

	[Inject] private ITileManager _tileManager;

	private CancellationTokenSource _tokenSource;
	private bool _isAutoleveling;
	public bool IsAutoleveling => _isAutoleveling;

	public void StartAutoLevel()
	{
		if (_isAutoleveling)
		{
			Debug.LogWarning("Auto-leveling is running. Can't start twice.");
			return;
		}

		_tokenSource?.Cancel();
		_tokenSource?.Dispose();
		_tokenSource = new CancellationTokenSource();
		RunAutoLevelAsync(_tokenSource.Token).Forget();
	}

	private async UniTask RunAutoLevelAsync(CancellationToken token)
	{
		_isAutoleveling = true;

		var steps = _tileManager.AutoPlaySteps;
		foreach (var step in steps)
		{
			token.ThrowIfCancellationRequested();
			if (!_tileManager.TryGetInteractables(out var interactables))
			{
				Debug.LogWarning($"There are no interactables");
				break;
			}

			var matching = interactables
			   .Where(pair =>
				   !string.IsNullOrEmpty(pair.Value.ID) &&
				   pair.Value.ID.Equals(step.ID) &&
				   step.Coords.Contains(pair.Key))
			   .Select(pair => pair.Value)
			   .ToList();

			if (matching.Count < 2 && _tileManager.TilesNum > 0)
			{
				// запасной вариант: любая пара
				var fallbackGroup = interactables
					.GroupBy(pair => pair.Value.ID)
					.Where(g =>
						!string.IsNullOrEmpty(g.Key) &&
						g.Count() > 1
					)
					.FirstOrDefault();

				if (fallbackGroup != null)
				{
					matching = fallbackGroup
						.Take(2)
						.Select(pair => pair.Value)
						.ToList();
				}
				else
				{
					Debug.LogWarning($"No matching tiles found for autoleveling step {step.ID}. Skipping this step.");
					continue;
				}
			}

			_tileManager.RemoveTilesCollection(matching);
			try
			{
				await UniTask.Delay(
					TimeSpan.FromSeconds(AUTO_LEVEL_DELAY),
					cancellationToken: token
				);
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Auto-leveling was cancelled.");
				continue;
			}
		}
		Debug.Log("Finish autoleveling");
		_isAutoleveling = false;
	}


	private void OnDestroy()
	{
		_tokenSource?.Cancel();
		_tokenSource?.Dispose();
	}
}
