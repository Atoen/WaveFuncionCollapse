using ConsoleGUI;
using WaveFunctionCollapse.UI;

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

Input.TreatControlCAsInput = true;

MainMenu.Show();