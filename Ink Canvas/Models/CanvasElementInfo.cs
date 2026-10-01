namespace Ink_Canvas
{
    /// <summary>Canvas element metadata persisted in .elements.json.</summary>
    public class CanvasElementInfo
    {
        public string Type { get; set; }
        public string SourcePath { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Stretch { get; set; } = "Fill";
        public string MediaKind { get; set; }
        public string MediaDisplayName { get; set; }
        public double? MediaPositionSeconds { get; set; }
        public double? MediaSpeedRatio { get; set; }
        public double? MediaVolume { get; set; }
        public int? PdfCurrentPage { get; set; }
        public int? PdfPageCount { get; set; }
    }
}
