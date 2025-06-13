using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Majong.Tiles;

public interface ISpritesHolder
{
	IEnumerable<ITileConfig> GetRandomUniqueTiles(int pairNumber);
}

[CreateAssetMenu(menuName = "ScriptableObjects/Sprites", fileName = "SpritesHolder")]
public class SpritesHolder : ScriptableObject, ISpritesHolder
{
	[SerializeField] private Sprite[] _sprites;

	public IEnumerable<ITileConfig> GetRandomUniqueTiles(int pairNumber)
	{
		var uniqueSprites = _sprites.GetRandomUniqueElements(pairNumber, true);

		foreach (var sprite in uniqueSprites)
		{
			yield return new TileConfig(sprite.name, sprite);
		}
	}
}