using Majong.Tiles;
using UniRx;
using UnityEngine;
using Zenject;
namespace Majong.Level
{
	public interface ILevelManager
	{
		IReadOnlyReactiveProperty<IMapConfig> CurrentLevel { get; }
		void StartNextLevel();
		void Restart();
	}

	public class LevelManager : ILevelManager, IInitializable
	{
		[Inject] private IMapConfigHolder _mapConfigHolder;

		private ReactiveProperty<IMapConfig> _currentLevel = new();
		public IReadOnlyReactiveProperty<IMapConfig> CurrentLevel => _currentLevel;

		public void Initialize()
		{
			if (PlayerPrefs.HasKey("Level"))
			{
				string levelId = PlayerPrefs.GetString("Level");
				_currentLevel.Value = _mapConfigHolder.GetMapConfig(levelId);
			}
			else
			{
				_currentLevel.Value = _mapConfigHolder.GetNextMapConfig("");
			}
		}

		public void StartNextLevel()
		{
			_currentLevel.Value = _mapConfigHolder.GetNextMapConfig(_currentLevel.Value.ID);
			PlayerPrefs.SetString("Level", _currentLevel.Value.ID);
		}

		public void Restart()
		{
			if(_currentLevel.Value == null)
				return;

			_currentLevel.SetValueAndForceNotify(_currentLevel.Value);
		}
	}
}