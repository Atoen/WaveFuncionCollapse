using ConsoleGUI;
using WaveFunctionCollapse;

var asked = false;
Input.ControlC += delegate
{
    if (!asked)
    {
        asked = true;
        Console.Title = "Confirm Exit?";
    }
    else Application.Exit();
};

Application.Start();

Input.TreatControlCAsInput = false;

MainMenu.Show();