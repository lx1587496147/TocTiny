using System;
using TocTiny.View;

namespace TocTiny
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ViewManager viewManager = new ViewManager();
            viewManager.Run();
        }
    }
}
