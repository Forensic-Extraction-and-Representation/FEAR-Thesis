using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Domain.Model.ColorMap
{
    public enum ColorMapEntityDiscriminator
    {
        None = 0,
        Investigation = 1,
        User = 2,
    }

    public class ColorMapSet
    {
        public virtual Guid ColorMapSetId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ColorMapEntityDiscriminator EntityDiscriminator { get; set; }
        public string EntityId { get; set; }

        public IList<ColorMap> ColorMaps { get; set; }
    }

    public class ColorMap
    {
        public int Layer { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> SubjectValues { get; set; } = new();
        public List<string> ObjectValues { get; set; } = new();
        public string Prefix { get; set; }
        public Uri Ontology { get; set; }
        public ColorMapStyle Styles { get; set; } = new();
    }

    public class EdgeStyle
    {
        public EdgeStyle(string lineColor)
        {
            LineColor = lineColor;
            TargetArrowColor = lineColor;
        }

        public string Width { get; set; } = "4px";
        public string LineColor { get; set; }
        public string TargetArrowColor { get; set; }
        public string CurveStyle { get; set; } = "bezier";
        public string FontSize { get; set; } = "12px";
    }

    public class NodeStyle
    {
        public NodeStyle(string backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public string BackgroundColor { get; set; }
        public string Shape { get; set; } = "round-octagon";
        public string TextValign { get; set; } = "center";
        public string TextHalign { get; set; } = "right";
        public string TextWrap { get; set; } = "wrap";
        public string TextOverflowWrap { get; set; } = "anywhere";
        public string TextMaxWidth { get; set; } = "140px";
        public string FontSize { get; set; } = "12px";
    }

    public class ColorMapStyle
    {
        public EdgeStyle Edge { get; set; }
        public NodeStyle Node { get; set; }
    }

}
