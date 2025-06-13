using UnityEngine;

namespace Majong.Tiles
{
	public interface ITileConfig
	{
		string Name { get; }
		Sprite Sprite { get; }
	}

	public class TileConfig : ITileConfig
	{
		private string _name;
		private Sprite _sprite;
		public string Name => _name;
		public Sprite Sprite => _sprite;

		public TileConfig(string name, Sprite sprite)
		{
			_name = name;
			_sprite = sprite;
		}
	}
}
