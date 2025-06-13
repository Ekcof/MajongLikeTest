using System.Collections.Generic;
using System.Linq;

namespace Majong.Tiles
{
	public class AutolevelStep
	{
		public AutolevelStep(IEnumerable<KeyValuePair<Coordinates,Tile>> kvps)
		{
			ID = kvps.First().Value.ID;
			Coords = kvps.Select(k => k.Key).ToList();
		}

		public List<Coordinates> Coords;
		public string ID;
	}
}