using System;
using Terminal.Gui;

namespace Gtt.FastPass.Gui
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            var win = new Window("Fast Pass API Test Runner")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            top.Add(win);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_Main Menu", new MenuItem [] {
                    new MenuItem ("_Restart","", Restart),
                    //new MenuItem ("_Close", "", () => Close ()),
                    new MenuItem ("_Quit", "", () =>
                    {
                        top.Running = false;
                    })
                })
            });
            top.Add(menu);

            //var login = new Label("Login: ") { X = 3, Y = 2 };
            //var password = new Label("Password: ")
            //{
            //    X = Pos.Left(login),
            //    Y = Pos.Top(login) + 1
            //};
            //var loginText = new TextField("")
            //{
            //    X = Pos.Right(password),
            //    Y = Pos.Top(login),
            //    Width = 40
            //};
            //var passText = new TextField("")
            //{
            //    Secret = true,
            //    X = Pos.Left(loginText),
            //    Y = Pos.Top(password),
            //    Width = Dim.Width(loginText)
            //};

            win.Add(
                RunAllTests(),
                new CheckBox(1, 2, "Run all Tests")
            );

            // Add some controls, 
            //win.Add(
            //    // The ones with my favorite layout system, Computed
            //    login, password, loginText, passText,

            //    // The ones laid out like an australopithecus, with Absolute positions:
            //    new CheckBox(3, 6, "Remember me"),
            //    new Button(3, 14, "Ok"),
            //    new Button(10, 14, "Cancel"),
            //    new Label(3, 18, "Press F9 or ESC plus 9 to activate the menubar")
            //);

            Application.Run();
        }

        private static void Restart()
        {
            Console.WriteLine("Restarting");
        }

        private static Button RunAllTests()
        {
            var btn = new Button(1, 1, "Run all tests", is_default: true);
            btn.Clicked += () =>
            {
                new FastPassTestRunner().RunWarmUps(new FastPassEndpoint("http://deckofcardsapi.com:80"));
            };
            return btn;
        }
    }
}
