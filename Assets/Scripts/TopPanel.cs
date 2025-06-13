using Majong.Level;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class TopPanel : MonoBehaviour
{
	[Inject] private ILevelManager _levelManager;
	[SerializeField] private TMP_Text _levelText;

	private void Awake()
	{
		_levelManager.CurrentLevel
			.Subscribe(OnChangeLevel)
			.AddTo(this);
	}

	private void OnChangeLevel(IMapConfig map)
	{
		if (map == null)
		{
			Debug.LogWarning("MapConfig is null, cannot update level text.");
			return;
		}
		_levelText.text = $"Level: {map.ID}";
	}
}
