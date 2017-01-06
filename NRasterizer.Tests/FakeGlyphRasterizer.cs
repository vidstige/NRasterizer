using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    public class FakeGlyphRasterizer : IGlyphRasterizer
    {
        public int BeginReadCallCount { get; private set; }
        public int CloseFigureCallCount { get; private set; }
        public int ContourCount { get; private set; }
        public int EndReadCallCount { get; private set; }
        public int FlushCallCount { get; private set; }
        public List<Point<double>> Points { get; private set; } = new List<Point<double>>();
        public int Resolution { get; set; } =  72;

        public void BeginRead(int countourCount)
        {
            BeginReadCallCount++;
            ContourCount = countourCount;
        }

        public void CloseFigure()
        {
            CloseFigureCallCount++;
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            Points.Add(new Point<double>(x, y));
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            Points.Add(new Point<double>(x, y));
        }

        public void EndRead()
        {
            EndReadCallCount++;
        }

        public void Flush()
        {
            FlushCallCount++;
        }

        public void LineTo(double x, double y)
        {
            Points.Add(new Point<double>(x, y));
        }

        public void MoveTo(double x, double y)
        {
            Points.Add(new Point<double>(x, y));
        }
    }
}

