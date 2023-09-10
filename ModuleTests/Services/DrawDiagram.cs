using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using Bygdrift.Tools.DataLakeTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ModuleTests.Services
{
    [TestClass]
    public class DrawDigram
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        private readonly Plot plt;
        private readonly string fileName;
        private int lineNumber = 1;

        public DrawDigram(int width, int height, string filename)
        {
            fileName = filename;
            plt = new(width, height);
        }

        public void DrawTimeStack(List<Span> spans, bool withGroups)
        {
            if (!Debugger.IsAttached)
                return;

            foreach (var spanGroup in spans.GroupBy(o => o.Group))
            {
                for (int s = spanGroup.Count() - 1; s >= 0; s--)
                {
                    var span = spanGroup.ElementAt(s);

                    if (span.SpanRows.Any())
                    {
                        for (int r = span.SpanRows.Count - 1; r >= 0; r--)
                            DrawVector(new Vector(span.SpanRows[r].From, span.SpanRows[r].To, Color.MistyRose, 2, $"S{s + 1}, R{r + 1}"));

                        for (int r = span.TimesInSpan.Count - 1; r >= 0; r--)
                            DrawVector(new Vector(span.TimesInSpan[r].From, span.TimesInSpan[r].To, Color.DodgerBlue, 4, $"S{s + 1}, R{r + 1}"));
                    }
                    DrawVector(new Vector(span.From, span.To, Color.Red, 6, $"S{s + 1} {(spanGroup.Key != null ? ", g" + spanGroup.Key : " ")}"));
                }
            }
            Execute();
        }

        private void DrawVector(Vector vector)
        {
            DrawVector(vector, lineNumber);
            lineNumber++;
        }

        private void DrawVector(Vector vector, int lineNumber)
        {
            if (vector == null)
                return;

            if (!string.IsNullOrEmpty(vector.Text))
            {
                var text = plt.AddText(vector.Text, vector.From, lineNumber - 0.2);
                text.Color = vector.Color;
            }

            var line = plt.AddArrow(vector.To, lineNumber, vector.From, lineNumber);
            line.Color = vector.Color;
            line.MarkerSize = vector.Weight * 2f;
            line.LineWidth = vector.Weight;
        }

        private void Execute()
        {
            plt.YAxis.ManualTickSpacing(1);
            plt.YAxis.MinimumTickSpacing(1);
            plt.YAxis.MajorGrid(false);
            plt.YAxis.MinorGrid(false);
            plt.XAxis.DateTimeFormat(true);
            plt.Style(Style.Black);
            var path = Path.Combine(BasePath, "Files", "Out");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            plt.SaveFig(Path.Combine(path, fileName));
        }
    }

    public class Vector
    {
        public Vector(DateTime from, DateTime to, Color color, int weight = 1, string text = "")
        {
            From = from.ToOADate();
            To = to.ToOADate();
            Color = color;
            Weight = weight;
            Text = text;
        }

        public double From { get; set; }
        public double To { get; set; }
        public Color Color { get; set; }
        public int Weight { get; }
        public string Text { get; set; }
    }
}
