using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using WGP.AZURUI;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace Executer
{
    internal class Program
    {
        #region Private Methods

        private static void Main(string[] args)
        {
            Engine.Initialize();
            var app = new RenderWindow(new VideoMode(900, 600), "test");
            app.Closed += (sender, e) => app.Close();

            app.SetVerticalSyncEnabled(true);

            var button = new Button();
            button.HalfSize = new Vector2f(120, 40);
            button.Position = new Vector2f(150, 150);
            button.Text = "Jouer";
            button.Hue = ((HSVColor)Color.Green).H;
            button.Clicked = () => Console.WriteLine("clic !");
            var checkbox = new Checkbox();
            checkbox.Text = "check";
            checkbox.Hue = ((HSVColor)Color.Red).H;
            checkbox.Position = new Vector2f(150, 350);
            checkbox.StateChanged = () => Console.WriteLine(checkbox.Checked);

            while (app.IsOpen)
            {
                app.DispatchEvents();

                button.Update(app);
                checkbox.Update(app);

                app.Clear(Engine.DefaultBackgroundColor);

                button.DrawOn(app);
                checkbox.DrawOn(app);

                app.Display();
            }
        }

        #endregion Private Methods
    }
}