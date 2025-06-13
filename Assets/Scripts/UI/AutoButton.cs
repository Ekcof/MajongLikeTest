using Majong.Tiles;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AutoButton : MonoBehaviour
{
	[Inject] private IAutoLevelRunner _autoLevelRunner;
	[Inject] private ITileManager _tileManager;
	private Button _button;
	private void Awake()
	{
		_button = GetComponent<Button>();
		_button.SetListener(() => _autoLevelRunner.StartAutoLevel());
		_tileManager.Matchings.SkipLatestValueOnSubscribe().Subscribe(OnChangeMatchings).AddTo(this);
	}

	private void OnChangeMatchings(int pairs)
	{
		Debug.Log($"OnChangeMatchings: {pairs}, TilesNum: {_tileManager.TilesNum}, IsAutoleveling: {_autoLevelRunner.IsAutoleveling}");
		gameObject.SetActive(pairs > 0 && _tileManager.TilesNum > 0 && !_autoLevelRunner.IsAutoleveling);
	}
}
