using Majong.Tiles;
using System;
using System.Linq;
using UnityEngine;

namespace Majong.Level
{
	public interface IMapConfig
	{
		string ID { get; }
		public int PairNum { get; }
		public int SlotsCount { get; }
		int AvailablePairsCount { get; }
		LayerConfig[] LayerConfigs { get; }
		float TileOffsetX { get; }
		float TileOffsetY { get; }
		bool IsPlacable(int layerIndex, int x, int y);
		bool IsPlacable(Coordinates coords);
		Vector3 GetOffset(int layerIndex);
	}

	[CreateAssetMenu(menuName = "ScriptableObjects/MapConfig", fileName = "MapConfig")]
	public class MapConfig : ScriptableObject, IMapConfig
	{
		[SerializeField] private string _id;
		[SerializeField] private LayerConfig[] _layerConfigs;
		[SerializeField, Min(1)] private int _pairNum;

		[SerializeField] private float _layerOffset = 0.05f;
		[SerializeField] private float _tileOffsetX = 0.8f;
		[SerializeField] private float _tileOffsetY = 1.0f;

		public string ID => _id;
		public LayerConfig[] LayerConfigs => _layerConfigs;
		public int PairNum => _pairNum;
		public int SlotsCount => _layerConfigs.Sum(l => l.PlacableSlotsCount);
		public int AvailablePairsCount => SlotsCount / (2 * Math.Clamp(_pairNum,1,int.MaxValue));

		public float TileOffsetX => _tileOffsetX;
		public float TileOffsetY => _tileOffsetY;

		public bool IsPlacable(int layerIndex, int x, int y)
		{
			if (layerIndex < 0 || layerIndex >= _layerConfigs.Length)
				return false;
			return _layerConfigs[layerIndex].IsPlacable(x, y);
		}

		public bool IsPlacable(Coordinates coords)
		{
			return IsPlacable(coords.Layer, coords.X, coords.Y);
		}

		public Vector3 GetOffset(int layerIndex)
		{
			var layer = _layerConfigs[layerIndex];

			int w = layer.Width;
			int h = layer.Height;

			float totalWidth = (w - 1) * _tileOffsetX;
			float totalHeight = (h - 1) * _tileOffsetY;

			float x0 = -totalWidth / 2f - layerIndex * _layerOffset;
			float y0 = totalHeight / 2f + layerIndex * _layerOffset;
			return new Vector3(x0, y0, -layerIndex);
		}
	}

	[Serializable]
	public class LayerConfig
	{
		[SerializeField, Min(1)] private int _width = 6;
		[SerializeField, Min(1)] private int _height = 7;


		[SerializeField] private bool[] _slots;

		public Vector3 Offset;
		public int Width => _width;
		public int Height => _height;
		public int PlacableSlotsCount => _slots.Count(slot => slot);

		public bool IsPlacable(int x, int y)
		{
			if (x < 0 || x >= _width || y < 0 || y >= _height)
				return false;
			return _slots[y * _width + x];
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			int size = _width * _height;
			if (_slots == null || _slots.Length != size)
			{
				Array.Resize(ref _slots, size);
			}
		}
#endif
	}
}