using System.Collections.Generic;

namespace Ink_Canvas.Controls.Toolbar.FloatingToolbar
{
    public sealed class ToolbarSettingInfo
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ToolbarSettingType Type { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public List<string> OptionValues { get; set; } = new List<string>();
        public string DefaultValue { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? StepSize { get; set; }
    }

    public enum ToolbarSettingType
    {
        ComboBox,
        Slider,
        Toggle
    }
}
