using Majong.Tiles;
using Majong.Level;
using UnityEngine;

namespace Majong.Installers
{
	public class MainSceneInstaller : BaseInstaller
	{
		[SerializeField] private Camera _camera;
		[SerializeField] private TileManager _tileManager;
		[SerializeField] private InputManager _inputManager;
		[SerializeField] private NextLevelButton _nextLevelButton;

		public override void InstallBindings()
		{
			Container.BindInstance(_camera).WithId("mainCam");

			Bind(_tileManager);
			Bind(_inputManager);
			Bind(_nextLevelButton);
			Bind<LevelManager>();
			Bind<AutoLevelRunner>();
		}
	}
}