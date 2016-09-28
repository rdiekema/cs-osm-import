using ICities;

namespace Mapper
{
    public class OsmImportMod : IUserMod
    {
        public string Name
        {
            get
            {
                return "OSM Importer";
            }
        }
        public string Description
        {
            get
            {
                return "Import realworld map data from OpenStreetMap.";
            }
        }
    }
}
