namespace Ink_Canvas.Models
{
    /// <summary>
    /// PPT 智慧模式的视频控件区域，保留 Shape 的原始磅值，由主窗口转换为屏幕坐标。
    /// </summary>
    public sealed class SmartRegion
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string ShapeName { get; set; }
        public int MediaType { get; set; }
    }
}
