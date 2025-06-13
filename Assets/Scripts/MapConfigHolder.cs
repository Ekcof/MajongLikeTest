using System.Linq;
using UnityEngine;

namespace Majong.Level
{
	public interface IMapConfigHolder
	{
		IMapConfig GetMapConfig(string id);
		IMapConfig GetNextMapConfig(string id);
	}

	[CreateAssetMenu(fileName = "MapConfigHolder", menuName = "ScriptableObjects/MapConfigHolder", order = 1)]
	public class MapConfigHolder : ScriptableObject, IMapConfigHolder
	{
		[SerializeField] private MapConfig[] _configs;

		public IMapConfig GetMapConfig(string id)
		{
			return _configs.FirstOrDefault(config => config.ID.Equals(id));
		}

		public IMapConfig GetNextMapConfig(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				for (int i = 0; i < _configs.Length; i++)
				{
					if (_configs[i].ID.Equals(id))
					{
						return _configs[(i + 1) % _configs.Length];
					}
				}
			}
			return _configs.FirstOrDefault(); 
		}
	}
}
