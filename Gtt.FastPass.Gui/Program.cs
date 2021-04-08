using System;
using System.Linq;
using Gtt.FastPass.Sample.Models;
using Gtt.FastPass.Serializers;
using NStack;
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

            var runner = new FastPassTestRunner<TestModel>(new FastPassEndpoint("http://deckofcardsapi.com:80"));
            var testsByController = runner.GetTests().GroupBy(x => x.TestClass.Name).ToList();

            for (int i = 0; i < testsByController.Count; i++)
            {
                var group = testsByController[i];
                win.Add(new Label(1, 1, group.Key));
                var gl = group.ToList();
                for (int j = 0; j < gl.Count; j++)
                {
                    var test = gl[j];
                    var btn = new Button(i, j + 2, test.TestMethod.Name);
                    btn.Clicked += () =>
                    {
                        FastPassResponse result = test.Execute();
                        if (result.AllTestsPassed && !btn.Text.EndsWith("PASS"))
                        {
                            btn.Text = btn.Text + " PASS";
                        }
                        WriteResponse(result, win);
                    };
                    win.Add(btn);
                }

            }

            //win.Add(
            //    RunAllTests()
            //);

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

        private static void WriteResponse(FastPassResponse result, Window win)
        {
            var f = new FrameView(new Rect(25, 1, 300, 400), result.Request.Endpoint.Name);
            f.Visible = true;
            f.Add(new Label(0, 0, $"{result.Request.Method} {result.Request.Endpoint.BuildUrl()}"));
            f.Add(new Label(0, 1, $"{new JsonObjectSerializer(true).Pretty(result.Request.Content)}"));
            f.Add(new Label(0, 2, $"{result.StatusCode}"));
            f.Add(new Label(0, 3, $"{new JsonObjectSerializer(true).Pretty(result.Content)}"));
            win.Add(f);
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
                new FastPassTestRunner<TestModel>(new FastPassEndpoint("http://deckofcardsapi.com:80")).RunWarmUps();
            };
            return btn;
        }
    }
}
