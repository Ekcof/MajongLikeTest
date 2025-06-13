using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UniRx;

public class CommonPool<T> where T : Component // Pool for copies from component prefabs
{
	private readonly DiContainer _diContainer;
	private readonly T _prefab;
	private readonly Transform _parent;
	private readonly Stack<T> _pool = new();

	public CommonPool(T prefab, Transform parent = null, DiContainer diContainer = null)
	{
		_diContainer = diContainer;
		_prefab = prefab;
		_prefab.gameObject.SetActive(false);
		_parent = parent ?? prefab.transform.parent;
	}

	public CommonPool(IEnumerable<T> items, Transform parent = null, DiContainer diContainer = null)
	{
		_diContainer = diContainer;
		foreach (var item in items)
		{
			item.gameObject.SetActive(false);
			if (_prefab == null)
			{
				_prefab = item;
			}
			else
			{
				_pool.Push(item);
			}
		}

		if (_prefab == null)
		{
			Debug.LogError($"No prefab for {typeof(T)}");
			return;
		}

		_prefab.gameObject.SetActive(true);

		_prefab.transform.SetAsFirstSibling();
		_parent = parent ?? _prefab.transform.parent;
	}

	public CommonPool(Transform parent, int startSize = 0, DiContainer diContainer = null)
	{
		_diContainer = diContainer;

		foreach (var child in parent.GetComponentsInChildren<T>(true))
		{
			child.transform.SetParent(parent);
			_pool.Push(child);
		}

		_prefab = _pool.Pop();
		_prefab.transform.SetAsFirstSibling();
		_parent = parent;

		while (startSize-- > 0) SpawnOneCopy();
	}

	public T Pop()
	{
		if (_pool.Count == 0)
		{
			SpawnOneCopy();
		}

		return _pool.Pop();
	}

	public void Push(T item)
	{
		_pool.Push(item);
		item.gameObject.SetActive(false);
	}

	public void Push(List<T> items)
	{
		if (items == null)
		{
			Debug.LogError($"No items to push");
			return;
		}

		foreach (var item in items)
		{
			Push(item);
			item.gameObject.SetActive(false);
		}

		items.Clear();
	}

	private void SpawnOneCopy()
	{
		var copy = _prefab.Copy(_parent);
		_diContainer?.InjectGameObject(copy.gameObject);
		_pool.Push(copy);
	}
}
