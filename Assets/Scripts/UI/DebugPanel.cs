using Majong.Tiles;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class DebugPanel : MonoBehaviour
{
	[Inject] private ITileManager _tileManager;
	[SerializeField] private TMP_Text _info;
	private void Awake()
	{
		_tileManager.Matchings.Subscribe(OnChangeMatchingPairs).AddTo(this);
	}

	private void OnChangeMatchingPairs(int pairs)
	{
		_info.text = $"{pairs}";
	}
}
