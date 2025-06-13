using Majong.Level;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Majong.Installers
{
	[CreateAssetMenu(fileName = "Configs Installer", menuName = "Installers/Configs Installer")]
	public class ScriptableInstaller : ScriptableObjectInstaller
	{
		[SerializeField] private SpritesHolder _sprites;
		[SerializeField] private MapConfigHolder _mapConfigHolder;
		public override void InstallBindings()
		{
			Bind(_mapConfigHolder);
			Bind(_sprites);
		}

		private void Bind<T>(T instance) where T : ScriptableObject
		{
			if (typeof(T).GetInterfaces().Any())
			{
				Container.BindInterfacesAndSelfTo<T>().FromInstance(instance);
				return;
			}

			Container.BindInstance(instance);
		}
	}
}