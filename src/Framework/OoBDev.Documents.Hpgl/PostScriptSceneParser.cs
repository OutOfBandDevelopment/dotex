using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace OoBDev.Documents.Hpgl;

/// <summary>
/// The PostScript <see cref="IVectorSceneParser"/>: a small interpreter for the path operators of a drawing
/// (<c>newpath moveto rmoveto lineto rlineto curveto arc closepath stroke fill rectstroke rectfill</c>), colour
/// (<c>setrgbcolor setgray</c>) and the matrix (<c>translate scale rotate gsave grestore</c>). Text, images, procedures,
/// dictionaries and loops are skipped, so the result is the line work only. Never throws on malformed input.
/// </summary>
public sealed class PostScriptSceneParser : IVectorSceneParser
{
    private const double Margin = 20;
    private const int CurveSteps = 12;
    private const double ArcStepDegrees = 10;

    /// <inheritdoc />
    public VectorScene Parse(ReadOnlySpan<byte> data)
    {
        var state = new Interpreter();
        foreach (var token in Tokenize(Encoding.Latin1.GetString(data)))
        {
            if (state.Done)
            {
                break;
            }

            state.Execute(token);
        }

        return state.ToScene();
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c) || c == '\0')
            {
                i++;
            }
            else if (c == '%')
            {
                while (i < text.Length && text[i] != '\n' && text[i] != '\r')
                {
                    i++;
                }
            }
            else if (c == '(')
            {
                i = SkipString(text, i);
                yield return "()";
            }
            else if (c is '{' or '}' or '[' or ']')
            {
                i++;
                yield return c.ToString();
            }
            else
            {
                var start = i++;
                while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] is not ('(' or '%' or '{' or '}' or '[' or ']' or '\0'))
                {
                    i++;
                }

                yield return text[start..i];
            }
        }
    }

    private static int SkipString(string text, int start)
    {
        var depth = 0;
        var i = start;
        while (i < text.Length)
        {
            switch (text[i])
            {
                case '\\': i++; break;
                case '(': depth++; break;
                case ')':
                    depth--;
                    if (depth == 0)
                    {
                        return i + 1;
                    }

                    break;
                default: break;
            }

            i++;
        }

        return text.Length;
    }

    private sealed class Interpreter
    {
        private readonly List<double> _stack = [];
        private readonly List<List<VectorPoint>> _path = [];
        private readonly List<VectorPolyline> _lines = [];
        private readonly Stack<(double A, double B, double C, double D, double E, double F, string Color)> _saved = new();
        private double _a = 1, _b, _c, _d = 1, _e, _f;
        private double _userX, _userY;
        private string _color = "#000000";
        private int _procedureDepth;

        public bool Done { get; private set; }

        [SuppressMessage("Maintainability", "MA0051:Method is too long", Justification = "A flat operator table; splitting it would scatter shared interpreter state.")]
        [SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "A flat operator table.")]
        public void Execute(string token)
        {
            if (token == "{")
            {
                _procedureDepth++;
                return;
            }

            if (token == "}")
            {
                _procedureDepth = Math.Max(0, _procedureDepth - 1);
                return;
            }

            if (_procedureDepth > 0)
            {
                return;
            }

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                _stack.Add(number);
                return;
            }

            switch (token)
            {
                case "newpath": _path.Clear(); break;
                case "moveto": MoveTo(Pop(2)); break;
                case "rmoveto": { var d = Pop(2); MoveTo([_userX + d[0], _userY + d[1]]); break; }
                case "lineto": LineTo(Pop(2)); break;
                case "rlineto": { var d = Pop(2); LineTo([_userX + d[0], _userY + d[1]]); break; }
                case "curveto": CurveTo(Pop(6)); break;
                case "rcurveto": { var d = Pop(6); CurveTo([_userX + d[0], _userY + d[1], _userX + d[2], _userY + d[3], _userX + d[4], _userY + d[5]]); break; }
                case "arc": Arc(Pop(5), clockwise: false); break;
                case "arcn": Arc(Pop(5), clockwise: true); break;
                case "closepath": ClosePath(); break;
                case "stroke":
                case "fill":
                case "eofill":
                    Paint();
                    break;
                case "rectstroke":
                case "rectfill":
                    Rectangle(Pop(4));
                    break;
                case "setrgbcolor": { var d = Pop(3); _color = Color(d[0], d[1], d[2]); break; }
                case "setgray": { var d = Pop(1); _color = Color(d[0], d[0], d[0]); break; }
                case "translate": { var d = Pop(2); _e += (_a * d[0]) + (_c * d[1]); _f += (_b * d[0]) + (_d * d[1]); break; }
                case "scale": { var d = Pop(2); _a *= d[0]; _b *= d[0]; _c *= d[1]; _d *= d[1]; break; }
                case "rotate": Rotate(Pop(1)[0]); break;
                case "gsave": _saved.Push((_a, _b, _c, _d, _e, _f, _color)); break;
                case "grestore":
                    if (_saved.TryPop(out var s))
                    {
                        (_a, _b, _c, _d, _e, _f, _color) = s;
                    }

                    break;
                case "showpage": Done = true; break;
                case "pop": Pop(1); break;
                default: _stack.Clear(); break;
            }
        }

        public VectorScene ToScene()
        {
            if (_lines.Count == 0)
            {
                return VectorScene.Empty;
            }

            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (var line in _lines)
            {
                foreach (var p in line.Points)
                {
                    minX = Math.Min(minX, p.X);
                    maxX = Math.Max(maxX, p.X);
                    minY = Math.Min(minY, p.Y);
                    maxY = Math.Max(maxY, p.Y);
                }
            }

            var lines = new List<VectorPolyline>(_lines.Count);
            foreach (var line in _lines)
            {
                var points = new List<VectorPoint>(line.Points.Count);
                foreach (var p in line.Points)
                {
                    points.Add(new VectorPoint(p.X - minX + Margin, maxY - p.Y + Margin));
                }

                lines.Add(line with { Points = points });
            }

            return new VectorScene(maxX - minX + (2 * Margin), maxY - minY + (2 * Margin), lines);
        }

        private static string Color(double r, double g, double b) =>
            "#" + Channel(r) + Channel(g) + Channel(b);

        private static string Channel(double value) =>
            ((int)Math.Round(Math.Clamp(value, 0, 1) * 255)).ToString("x2", CultureInfo.InvariantCulture);

        private double[] Pop(int count)
        {
            var values = new double[count];
            for (var i = count - 1; i >= 0; i--)
            {
                if (_stack.Count == 0)
                {
                    break;
                }

                values[i] = _stack[^1];
                _stack.RemoveAt(_stack.Count - 1);
            }

            return values;
        }

        private VectorPoint Device(double x, double y) => new((_a * x) + (_c * y) + _e, (_b * x) + (_d * y) + _f);

        private void MoveTo(double[] p)
        {
            (_userX, _userY) = (p[0], p[1]);
            _path.Add([Device(p[0], p[1])]);
        }

        private void LineTo(double[] p)
        {
            if (_path.Count == 0)
            {
                MoveTo(p);
                return;
            }

            (_userX, _userY) = (p[0], p[1]);
            _path[^1].Add(Device(p[0], p[1]));
        }

        private void CurveTo(double[] p)
        {
            var (x0, y0) = (_userX, _userY);
            for (var i = 1; i <= CurveSteps; i++)
            {
                var t = (double)i / CurveSteps;
                var u = 1 - t;
                LineTo([
                    (u * u * u * x0) + (3 * u * u * t * p[0]) + (3 * u * t * t * p[2]) + (t * t * t * p[4]),
                    (u * u * u * y0) + (3 * u * u * t * p[1]) + (3 * u * t * t * p[3]) + (t * t * t * p[5]),
                ]);
            }
        }

        private void Arc(double[] p, bool clockwise)
        {
            var (cx, cy, r, from, to) = (p[0], p[1], p[2], p[3], p[4]);
            if (clockwise)
            {
                while (to > from)
                {
                    to -= 360;
                }
            }
            else
            {
                while (to < from)
                {
                    to += 360;
                }
            }

            var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(to - from) / ArcStepDegrees));
            for (var i = 0; i <= steps; i++)
            {
                var angle = (from + ((to - from) * i / steps)) * Math.PI / 180;
                double[] point = [cx + (r * Math.Cos(angle)), cy + (r * Math.Sin(angle))];
                if (i == 0 && _path.Count == 0)
                {
                    MoveTo(point);
                }
                else
                {
                    LineTo(point);
                }
            }
        }

        private void ClosePath()
        {
            if (_path.Count > 0 && _path[^1].Count > 1)
            {
                _path[^1].Add(_path[^1][0]);
            }
        }

        private void Rectangle(double[] r)
        {
            var (x, y, w, h) = (r[0], r[1], r[2], r[3]);
            _lines.Add(new VectorPolyline(1, _color, [Device(x, y), Device(x + w, y), Device(x + w, y + h), Device(x, y + h), Device(x, y)]));
        }

        private void Paint()
        {
            foreach (var subpath in _path)
            {
                if (subpath.Count > 1)
                {
                    _lines.Add(new VectorPolyline(1, _color, [.. subpath]));
                }
            }

            _path.Clear();
        }

        private void Rotate(double degrees)
        {
            var (sin, cos) = Math.SinCos(degrees * Math.PI / 180);
            (_a, _b, _c, _d) = ((_a * cos) + (_c * sin), (_b * cos) + (_d * sin), (-_a * sin) + (_c * cos), (-_b * sin) + (_d * cos));
        }
    }
}
