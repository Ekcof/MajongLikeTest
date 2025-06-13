using Majong.Tiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IInputManager
{
    void OnSelect(Tile tile);
}

public class InputManager : MonoBehaviour, IInputManager
{
	[Inject] private IAutoLevelRunner _autoLevelRunner;
    [Inject] private ITileManager _tileManager;
	private Tile _selectedTile;

	public void OnSelect(Tile tile)
	{
		if (_autoLevelRunner.IsAutoleveling)
		{
			return;
		}

		if (_selectedTile == null)
		{
			_selectedTile = tile;

			if (tile != null && tile.TrySetState(TileState.Selected))
			{
				return;
			}
		}
		else if (_selectedTile == tile)
		{
			_selectedTile.TrySetState(TileState.Interactable);
		}
		else if(tile != null && tile.HasID && tile.ID.Equals(_selectedTile.ID))
		{
			_tileManager.RemoveTiles(_selectedTile, tile);
		}
		else
		{
			_selectedTile.TrySetState(TileState.Interactable, true);
			tile.TrySetState(TileState.Interactable, true);
		}
		_selectedTile = null;
	}
}
