using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gtt.FastPass.Serializers;
using Terminal.Gui;

namespace Gtt.FastPass
{
    public class GuiRunner<T>
    {
        private readonly FastPassTestRunner<T> _runner;

        public GuiRunner(FastPassTestRunner<T> runner)
        {
            _runner = runner;
        }

        public void Run()
        {
            Application.Init();
            var top = Application.Top;
            FastPassResponse currentResult = null;

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

            var menu = new MenuBar(new[] {
                new MenuBarItem ("_Main Menu", new[] {
                    //new MenuItem ("_Close", "", () => Close ()),
                    new MenuItem ("_Quit", "", () =>
                    {
                        top.Running = false;
                    })
                })
            });
            top.Add(menu);

            var leftFrame = new FrameView()
            {
                Visible = true,
                Height = Dim.Fill(),
                Width = 30
            };

            var rightFrame = new FrameView()
            {
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                LayoutStyle = LayoutStyle.Computed,
                X = Pos.Right(leftFrame)
            };


            var resultsBtn = new Button(0, 0, "Results");
            var requestBtn = new Button(12, 0, "Request");
            var responseBtn = new Button(24, 0, "Response");

            rightFrame.Add(resultsBtn);
            rightFrame.Add(requestBtn);
            rightFrame.Add(responseBtn);

            var resultText = new TextView
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Y = 2
            };

            resultsBtn.Clicked += () => WriteResponse(currentResult, resultText, "");
            requestBtn.Clicked += () => WriteResponse(currentResult, resultText, "req");
            responseBtn.Clicked += () => WriteResponse(currentResult, resultText, "res");


            rightFrame.Add(resultText);

            win.Add(leftFrame);
            win.Add(rightFrame);


            var testsByController = _runner.GetTests().GroupBy(x => x.TestClass.Name).ToList();
            int currentRow = 0;
            foreach (var @group in testsByController)
            {
                leftFrame.Add(new Label(1, currentRow, @group.Key));
                var gl = @group.ToList();
                foreach (var test in gl)
                {
                    currentRow++;
                    var btn = new Button(1, currentRow, test.TestMethod.Name);
                    var test1 = test;
                    btn.Clicked += () =>
                    {
                        FastPassResponse result = test1.Execute();
                        if (result.AllTestsPassed && !btn.Text.EndsWith("PASS"))
                        {
                            btn.Text = btn.Text + " PASS";
                        }

                        currentResult = result;
                        WriteResponse(result, resultText);
                    };
                    leftFrame.Add(btn);
                }

                currentRow++;
                currentRow++;
            }

            Application.Run();
        }

        private static void WriteResponse(FastPassResponse result, TextView text, string action = "")
        {
            if (result == null) return;

            StringBuilder sb = new StringBuilder();

            switch (action)
            {
                case "":

                    foreach (var test in result.Results)
                    {
                        string expected = "";
                        string actual = "";
                        if (!string.IsNullOrWhiteSpace(test.Expected))
                            expected = $"Expected: {test.Expected}";

                        if (!string.IsNullOrWhiteSpace(test.Actual))
                            actual = $"Actual: {test.Actual}";

                        switch (test.Label)
                        {
                            case ResultLabel.Fail:
                                sb.Append("FAIL");
                                break;
                            case ResultLabel.Pass:
                                sb.Append("PASS");
                                break;
                            case ResultLabel.Skip:
                                sb.Append("SKIP");
                                break;
                            case ResultLabel.Warn:
                                sb.Append("WARN");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        sb.AppendLine($"  {test.Name} {expected} {actual}");
                    }

                    break;

                case "req":
                    sb.AppendLine($"{result.Request.Method} {result.Request.Endpoint.BuildUrl()}");
                    sb.AppendLine();
                    foreach (var header in result.Request.Headers)
                    {
                        sb.Append(header.Key + ": ");
                        sb.AppendLine(string.Join("; ", header.Value));
                    }

                    var prettyContent = new JsonObjectSerializer(true).Pretty(result.Request.Content);
                    sb.AppendLine();
                    sb.AppendLine(prettyContent);

                    break;

                case "res":
                    sb.AppendLine($"HTTP/{result.HttpVersion} {result.StatusCode} {(HttpStatusCode)result.StatusCode}");
                    sb.AppendLine();
                    foreach (var header in result.Headers)
                    {
                        sb.Append(header.Key + ": ");
                        sb.AppendLine(string.Join("; ", header.Value));
                    }

                    var prettyResponse = new JsonObjectSerializer(true).Pretty(result.Content);
                    sb.AppendLine();
                    sb.AppendLine(prettyResponse);

                    break;
            }


            text.Text = sb.Replace("\r", "").ToString();
        }
    }
}
