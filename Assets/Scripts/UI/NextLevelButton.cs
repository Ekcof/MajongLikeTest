using Majong.Level;
using Majong.Tiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using System;

public class NextLevelButton : MonoBehaviour
{
	[Inject] private ILevelManager _levelManager;
	[Inject] private ITileManager _tileManager;

	private Button _button;
	private void Awake()
	{
		
		_button = GetComponent<Button>();
		_button.SetListener(OnClick);
		_tileManager.Matchings.SkipLatestValueOnSubscribe().Subscribe(OnChangeMatchings).AddTo(this);
	}

	private void OnChangeMatchings(int obj)
	{
		Debug.Log($"OnChangeMatchings: {obj}, TilesNum: {_tileManager.TilesNum}");
		gameObject.SetActive(obj == 0 && _tileManager.TilesNum == 0);
	}

	private void OnClick()
	{
		_levelManager.StartNextLevel();
		gameObject.SetActive(false);
	}
}
