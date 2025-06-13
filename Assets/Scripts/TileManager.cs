using Cysharp.Threading.Tasks;
using Majong.Level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using Zenject;
using static UnityEngine.Rendering.DebugUI;


namespace Majong.Tiles
{
	public interface ITileManager
	{
		IReadOnlyReactiveProperty<int> Matchings { get; }
		int TilesNum { get; }
		List<AutolevelStep> AutoPlaySteps { get; }
		UniTask FillMap(IMapConfig config, CancellationToken token);
		void RemoveTiles(params Tile[] tiles);
		void RemoveTilesCollection(IEnumerable<Tile> tiles);
		UniTask Clear(CancellationToken token);
		bool TryGetInteractables(out Dictionary<Coordinates, Tile> tiles, bool useExclusions = false);
	}

	public class TileManager : MonoBehaviour, ITileManager
	{
		[Inject] private DiContainer _diContainer;
		[Inject] private ISpritesHolder _spritesHolder;
		[Inject] private ILevelManager _levelManager;

		[SerializeField] private Tile _tilePrefab;
		[SerializeField] private float _appointingTileDelay = 0.05f;
		private IMapConfig _currentMap;
		private ReactiveProperty<int> _matchingPairs = new();
		private CancellationTokenSource _tokenSource;

		private CommonPool<Tile> _tilePool;
		private readonly Dictionary<Coordinates, Tile> _tiles = new();

		public IReadOnlyReactiveProperty<int> Matchings => _matchingPairs;
		public int TilesNum => _tiles.Count;
		public List<AutolevelStep> AutoPlaySteps { get; private set; }

		private void Awake()
		{
			_levelManager.CurrentLevel.Subscribe(OnChangeLevel).AddTo(this);
		}

		private void OnChangeLevel(IMapConfig map)
		{
			if (map == null)
			{
				Debug.LogError("Map is null, cannot fill map.");
				return;
			}
			_currentMap = map;
			if (_tokenSource != null && _tokenSource.IsCancellationRequested)
			{
				_tokenSource.Cancel();
				_tokenSource.Dispose();
			}
			_tokenSource = new();

			FillMapAsync(map, _tokenSource.Token).Forget();
		}

		private async UniTask FillMapAsync(IMapConfig map, CancellationToken token)
		{
			try
			{
				await Clear(token);
				await UniTask.NextFrame(cancellationToken: token);
				await FillMap(map, token);
				await UniTask.NextFrame(cancellationToken: token);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error while filling map: {ex.Message}");
			}

			CheckPassability();
		}

		public async UniTask Clear(CancellationToken token)
		{
			if (_tiles.Count == 0) return;
			foreach (var tile in _tiles.Values)
			{
				tile.TrySetState(TileState.Hidden);
				_tilePool.Push(tile);
			}
			_tiles.Clear();
			try
			{
				await UniTask.NextFrame(cancellationToken: token);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error while waiting of clearing map: {ex.Message}");
			}
		}

		public async UniTask FillMap(IMapConfig config, CancellationToken token)
		{
			_tilePool ??= new CommonPool<Tile>(_tilePrefab, transform, _diContainer);
			var configPairs = _spritesHolder.GetRandomUniqueTiles(config.AvailablePairsCount).ToList();
			var dictionary = new Dictionary<ITileConfig, int>();
			var uniqueTilesNum = config.PairNum;

			if (uniqueTilesNum > 1)
			{
				configPairs = configPairs.Multiply(uniqueTilesNum);
				configPairs.Shuffle();
			}

			var allCoordinates = new List<Coordinates>();
			for (int layerIndex = 0; layerIndex < config.LayerConfigs.Length; layerIndex++)
			{
				var layer = config.LayerConfigs[layerIndex];
				int w = layer.Width;
				int h = layer.Height;

				for (int i = 0; i < h; i++)
				{
					for (int j = 0; j < w; j++)
					{
						if (!layer.IsPlacable(j, i)) continue;

						var coords = new Coordinates() { X = j, Y = i, Layer = layerIndex };
						if (_tiles.ContainsKey(coords)) continue;

						var position = coords.CoordsToVector(config);
						var tile = _tilePool.Pop();
						tile.TrySetState(TileState.Unassigned);
						tile.transform.localPosition = position;
						_tiles.Add(coords, tile);
					}
				}
			}
			try
			{
				await UniTask.NextFrame(cancellationToken: token);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error while waiting of filling map: {ex.Message}");
			}

			var dict = new Dictionary<Coordinates, Tile>(_tiles);
			int iteration = 0;

			AutoPlaySteps = new();

			// Тут неправильно строит
			while (TryGetInteractables(dict, out var tiles, true))
			{
				++iteration;
				Debug.Log($"{nameof(FillMap)} There are {tiles.Count} interactable tiles on iteration {iteration}.");
				if (tiles.Count() == 1)
				{
					var last = tiles.First();
					_tiles.Remove(last.Key);
					_tilePool.Push(last.Value);
					break;
				}


				var pair = tiles.GetRandomUniqueElements(2);

				var tileConfig = configPairs.GetRandomElement();

				if (tileConfig == null)
				{
					break;
				}
				Debug.Log($"Add pair of {tileConfig.Name}");
				foreach (var item in pair)
				{
					item.Value.SetConfig(tileConfig);
					dict.Remove(item.Key);
				}
				configPairs.Remove(tileConfig);
				AutoPlaySteps.Add(new (pair));

				try
				{
					await UniTask.Delay(TimeSpan.FromSeconds(_appointingTileDelay), cancellationToken: token);
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"Error while waiting of filling map: {ex.Message}");
				}
			}
		}

		private void CheckPassability()
		{
			var interactable = new List<Tile>();
			foreach (var tile in _tiles)
			{
				var isInteractable = IsUnblocked(tile.Key);
				tile.Value.TrySetState(isInteractable ? TileState.Interactable : TileState.Blocked);
				if (isInteractable)
				{
					interactable.Add(tile.Value);
				}
			}

			_matchingPairs.Value = interactable.PairsInCollection();

			if (_matchingPairs.Value == 0 && _tiles.Count > 1)
			{
				foreach (var kvp in _tiles)
				{
					kvp.Value.TrySetState(TileState.Locked);
				}
			}
		}

		public bool TryGetInteractables(out Dictionary<Coordinates, Tile> tiles, bool useExclusions = false)
		{
			return TryGetInteractables(_tiles, out tiles, useExclusions);
		}

		private bool TryGetInteractables(Dictionary<Coordinates, Tile> dict, out Dictionary<Coordinates, Tile> tiles, bool useExclusions = false)
		{
			tiles = new Dictionary<Coordinates, Tile>();

			foreach (var tile in dict)
			{
				if (IsUnblocked(tile.Key, useExclusions))
					tiles.Add(tile.Key, tile.Value);
			}
			return tiles.Count > 0;
		}

		private bool IsUnblocked(Coordinates coords, bool useExclusions = false)
		{
			var isUnblockedBySide = IsNotBlocked(coords, useExclusions);
			var isUnblockedAbove = IsNotBlockedFromAbove(coords, useExclusions);

			return isUnblockedBySide && isUnblockedAbove;
		}

		private bool IsNotBlockedFromAbove(Coordinates coords, bool ignoreWithID = false)
		{
			foreach (var tile in _tiles)
			{
				if (tile.Key.Equals(coords))
					continue;

				var point = coords.CoordsToVector(_currentMap);
				var collider = tile.Value.Collider;
				if (collider != null && collider.enabled
					&& collider.OverlapPoint(point) && tile.Key.Layer > coords.Layer
					&& (!ignoreWithID || !tile.Value.HasID))
				{
					return false;
				}
			}
			return true;
		}

		private bool IsNotBlocked(Coordinates coords, bool ignoreWithID = false)
		{
			return !IsBlocked(coords, ignoreWithID);
		}

		private bool IsBlocked(Coordinates coords, bool ingoreWithID = false)
		{
			if (_tiles.Count == 0) return false;
			var leftTile = _tiles.FirstOrDefault(k => k.Key.X == coords.X - 1 && k.Key.Y == coords.Y && k.Key.Layer == coords.Layer);
			var rightTile = _tiles.FirstOrDefault(k => k.Key.X == coords.X + 1 && k.Key.Y == coords.Y && k.Key.Layer == coords.Layer);
			var leftBlock = _currentMap.IsPlacable(leftTile.Key) && leftTile.Value != null && (!ingoreWithID || !leftTile.Value.HasID);
			var rightBlock = _currentMap.IsPlacable(rightTile.Key) && rightTile.Value != null && (!ingoreWithID || !rightTile.Value.HasID);
			var isBlocked = leftBlock && rightBlock;

			return isBlocked;
		}

		private bool IsSuitableInRow(Coordinates coords, Coordinates prevCoords)
		{
			if (_tiles.Count == 0)
			{
				return true;
			}

			if (CountOfTilesInRow(coords) == 0)
			{
				return false;
			}

			var row = _tiles.Where(b => b.Key.Layer == coords.Layer && b.Key.Y == coords.Y).Select(b => b.Value);

			var hasPairInRow = row.PairsInCollection() > 0;

			return !hasPairInRow;
		}

		private int CountOfTilesInRow(Coordinates coords)
		{
			var y = coords.Y;
			var layer = coords.Layer;
			return _tiles.Where(t => t.Key.Y == y && t.Key.Layer == layer).Count();
		}

		public void RemoveTiles(params Tile[] tiles)
		{
			RemoveTilesCollection(tiles);
		}

		public void RemoveTilesCollection(IEnumerable<Tile> tiles)
		{
			Debug.Log($"___Try to remove {tiles.Count()} tiles from the map.");
			foreach (var tile in tiles)
			{
				var key = _tiles.FirstOrDefault(k => k.Value == tile).Key;
				tile.OnGet(() => _tilePool.Push(tile));
				_tiles.Remove(key);
			}
			CheckPassability();
		}

		private void OnDestroy()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
		}

	}
}
