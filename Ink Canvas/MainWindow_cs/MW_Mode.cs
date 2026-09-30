namespace Ink_Canvas
{
    public partial class MainWindow
    {
        private int _currentMode;

        internal int currentMode
        {
            get => _currentMode;
            set => _currentMode = value;
        }

        internal bool IsWhiteboardMode => currentMode == 1;
    }
}
