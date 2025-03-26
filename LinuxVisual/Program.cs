using Gtk;
using LinuxVisual;
using System;

class Program
{
    [STAThread]
    static void Main()
    {
        // Initialize GTK
        Application.Init();

        // Create and show the main window
        var mainWindow = new SMPSOGuiGtk();
        mainWindow.ShowAll();

        // Start the GTK event loop
        Application.Run();
    }
}