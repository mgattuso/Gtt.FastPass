using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.FastPass.Serializers;
using Terminal.Gui;

namespace Gtt.FastPass
{
    public class GuiRunner<T>
    {
        private readonly FastPassEndpoint _endpoint;
        FastPassResponse _currentResult;
        private Dictionary<Label, TestDefinition> _labels = new Dictionary<Label, TestDefinition>();

        internal GuiRunner(FastPassEndpoint endpoint)
        {
            _endpoint = endpoint.Clone();
            _endpoint.Options.WarnOnResponseTimeFailures = true;
        }

        public void Run()
        {
            Application.Init();
            Paint();
        }

        public void Paint()
        {
            var top = Application.Top;
            top.RemoveAll();

            var win = new Window("Fast Pass API Test Runner")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            top.Add(win);

            var menu = new MenuBar(new[] {
                new MenuBarItem ("_Main Menu", new[] {
                    new MenuItem ("_Restart", "", Paint),
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
                Width = 40
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
            //var repeatBtn = new Button(36, 0, "Repeat");

            rightFrame.Add(resultsBtn);
            rightFrame.Add(requestBtn);
            rightFrame.Add(responseBtn);
            //rightFrame.Add(repeatBtn);

            var resultText = new TextView
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Y = 2
            };

            var runner = new FastPassTestRunner<T>(_endpoint.Clone());

            resultsBtn.Clicked += () => WriteResponse(_currentResult, resultText, "");
            requestBtn.Clicked += () => WriteResponse(_currentResult, resultText, "req");
            responseBtn.Clicked += () => WriteResponse(_currentResult, resultText, "res");
            //repeatBtn.Clicked += () =>
            //{
            //    Paint();
            //    //TODO: TRIGGER THE TEST AGAIN
            //};


            rightFrame.Add(resultText);

            win.Add(leftFrame);
            win.Add(rightFrame);



            var testsByController = runner.GetTests().GroupBy(x => x.TestClass).ToList();
            int currentRow = 0;
            foreach (var @group in testsByController)
            {
                var attr = @group.Key.GetCustomAttribute<ApiTestSuiteAttribute>();
                string name = attr?.Name ?? @group.Key.Name;

                leftFrame.Add(new Label(1, currentRow, name));
                var gl = @group.ToList();
                foreach (var test in gl)
                {
                    currentRow++;
                    var lbl = new Label(1, currentRow, "");
                    var btn = new Button(3, currentRow, test.TestMethod.Name);
                    var test1 = test;
                    btn.Clicked += () =>
                    {
                        lbl.Text = "/";
                        int running = 1;
                        FastPassResponse result = null;
                        Task.Run(() =>
                        {
                            result = test1.Execute();
                            Interlocked.Decrement(ref running);
                        });
                        int iterations = 0;
                        string sprite = "/-\\|";
                        while (running > 0)
                        {
                            lbl.Text = sprite[iterations % 4].ToString();
                            Application.Refresh();
                            Task.Delay(200).Wait();
                            iterations++;
                        }

                        //lbl.Text = result.AllTestsPassed ? "P" : "X";

                        foreach (var label in _labels)
                        {
                            if (label.Value.TestHasBeenRun)
                            {
                                label.Key.Text = label.Value.TestResult.AllTestsPassed ? "P" : "X";
                            }
                        }

                        _currentResult = result;
                        WriteResponse(result, resultText);
                    };
                    _labels[lbl] = test;
                    leftFrame.Add(lbl);
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
