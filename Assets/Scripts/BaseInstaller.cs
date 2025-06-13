using UnityEngine;
using Zenject;

namespace Majong.Installers
{
	public abstract class BaseInstaller : MonoInstaller
	{
		protected void Bind<T>(T instance) where T : class
		{
			var instanceType = typeof(T);

			if (instance == null && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
			{
				Debug.LogError($"Binding null instance of type {instanceType.Name}");

				if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)) &&
					FindFirstObjectByType(typeof(T)) is T existingInstance)
				{
					instance = existingInstance;
				}
			}

			Container.BindInterfacesAndSelfTo<T>().FromInstance(instance).AsSingle();
		}

		protected void Bind<T>(Transform parent = null)
		{

			if (!typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
			{
				Container.BindInterfacesAndSelfTo<T>()
						 .AsSingle()
						 .NonLazy();

				return;
			}

			Container.BindInterfacesAndSelfTo<T>()
					 .FromNewComponentOnNewGameObject()
					 .WithGameObjectName($"~{typeof(T).Name}")
					 .UnderTransform(parent)
					 .AsSingle()
					 .NonLazy();
		}

	}
}
