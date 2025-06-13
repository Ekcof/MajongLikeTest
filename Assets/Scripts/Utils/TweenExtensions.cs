using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TweenExtensions
{
	public static void Recolor(this Renderer renderer, Color color, float duration = 0.2f, Ease ease = Ease.Linear)
	{
		DOTween.Kill(renderer);
		renderer.material.DOColor(color, duration).SetEase(ease);
	}
}
