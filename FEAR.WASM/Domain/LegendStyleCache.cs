using FEAR.Domain.Model.ColorMap;

namespace FEAR.WASM.Domain
{
    public class ValueStyleCache
    {
        private IEnumerable<ColorMap> _items;
        private Dictionary<string, ColorMap> _subjectValueMap = new Dictionary<string, ColorMap>();
        private Dictionary<string, ColorMap> _objectValueMap = new Dictionary<string, ColorMap>();
        public ValueStyleCache(IEnumerable<ColorMap> colorMaps)
        {
            _items = colorMaps;
            foreach (var item in _items)
            {
                foreach (var subjectValue in item.SubjectValues)
                {
                    if (!_subjectValueMap.ContainsKey(subjectValue))
                    {
                        _subjectValueMap.Add(subjectValue, item);
                    }
                }

                foreach (var objectValue in item.ObjectValues)
                {
                    if (!_objectValueMap.ContainsKey(objectValue))
                    {
                        _objectValueMap.Add(objectValue, item);
                    }
                }
            }
        }

        public ColorMap GetValueColor(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            
            if (_subjectValueMap.ContainsKey(value))
            {
                return _subjectValueMap[value];
            }
            else if (_objectValueMap.ContainsKey(value))
            {
                return _objectValueMap[value];
            }
            else
            {
                return null;
            }
        }
    }

    public class LegendStyleCache
    {
        private IEnumerable<ColorMap> _items;
        private Dictionary<string, Dictionary<string, ColorMap>> authorityDict = new Dictionary<string, Dictionary<string, ColorMap>>();
        public LegendStyleCache(IEnumerable<ColorMap> colorMaps)
        {
            _items = colorMaps;
            foreach (var item in _items)
            {
                if (!authorityDict.ContainsKey(item.Ontology.Authority))
                {
                    authorityDict[item.Ontology.Authority] = new Dictionary<string, ColorMap>();
                }

                if (!authorityDict[item.Ontology.Authority].ContainsKey(item.Ontology.ToString()))
                {
                    authorityDict[item.Ontology.Authority].Add(item.Ontology.ToString(), item);
                }

            }
        }

        public string ReduceUri(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            var s = uri.ToString();

            foreach (var authority in authorityDict)
            {
                if (uri.Authority == authority.Key)
                {
                    foreach (var item in authority.Value)
                    {
                        if (s.StartsWith(item.Key))
                        {
                            return $"{item.Value.Prefix}:{s.Substring(item.Key.Length)}";
                        }
                    }
                }
            }

            return s;
        }

        public static readonly ColorMap DefaultColorMap = new ColorMap()
        {
            Ontology = new Uri("http://default.colormap"),
            Styles = new ColorMapStyle
            {
                Edge = new EdgeStyle("#f96a1e"),
                Node = new NodeStyle("#f96a1e")
            }
        };
        public ColorMap GetColor(Uri uri)
        {
            var s = uri?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(s) || uri == null)
            {
                return null;
            }

            if (authorityDict?.ContainsKey(uri?.Authority) ?? false)
            { 
                foreach(var item in authorityDict[uri.Authority])
                {
                    if (s.StartsWith(item.Key))
                        return item.Value;
                }
            }

            return null;
        }
    }
}
