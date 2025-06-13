using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public static class UIExtensions
{
	public static void SetListener(this Button button, Action action)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() =>
		{
			action?.Invoke();
		});
	}
}

