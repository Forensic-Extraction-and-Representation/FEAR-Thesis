using FEAR.Domain.Model.ColorMap;
using FEAR.WASM.Model;

namespace FEAR.WASM.Components.Graph
{
    public static class LegendHelpers
    {
        public static IEnumerable<LegendItem> GetLegendItems(this IEnumerable<ColorMapSet> cms)
        {
            return cms.SelectMany(set => set.ColorMaps.GetLegendItems());
        }

        public static IEnumerable<ColorMap> GetAllColorMaps(this IEnumerable<ColorMapSet> cms)
        {
            return cms.SelectMany(set => set.ColorMaps);
        }

        public static IEnumerable<LegendItem> GetLegendItems(this IEnumerable<ColorMap> e)
        {
            return e.Select(cm => new LegendItem()
            {
                Color = cm.Styles.Node.BackgroundColor,
                Name = cm.Name,
                Description = cm.Description,
                Ontology = cm.Ontology
            });
        }
    }
}