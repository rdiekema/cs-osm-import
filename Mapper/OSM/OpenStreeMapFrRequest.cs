using System;
using Mapper.OSM;

namespace Mapper
{
	public class OpenStreeMapFrRequest
	{
		private string sourceName = "OpenStreetMap";
		private string nodesRequestUrl = "http://api.openstreetmap.fr/xapi?node[bbox={0},{1},{2},{3}]";
		private string waysRequestUrl = "http://api.openstreetmap.fr/xapi?way[bbox={0},{1},{2},{3}]";

		private decimal left;
		private decimal bottom;
		private decimal right;
		private decimal top;

		public OpenStreeMapFrRequest(osmBounds osmBounds)
		{
			bottom = Math.Min(osmBounds.minlat, osmBounds.maxlat);
			top = Math.Max(osmBounds.minlat, osmBounds.maxlat);
			left = Math.Min(osmBounds.minlon, osmBounds.maxlon);
			right = Math.Max(osmBounds.minlon, osmBounds.maxlon);
		}

	    public decimal Top
	    {
	        get { return top; }
	    }

	    public decimal Left
	    {
	        get { return left; }
	    }

	    public decimal Bottom
	    {
	        get { return bottom; }
	    }

	    public decimal Right
	    {
	        get { return right; }
	    }

	    public string WaysRequestUrl
	    {
	        get { return string.Format(waysRequestUrl, left, bottom, right, top); }
	    }

	    public string NodeRequestUrl
	    {
	        get { return string.Format(nodesRequestUrl, left, bottom, right, top); }
	    }

	    public string SourceName
	    {
	        get { return sourceName; }
	    }
	}
}
