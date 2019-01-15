using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    public class DrawingLayer
    {
        /// <summary>
        /// Create drawing layer.
        /// </summary>
        /// <param name="name">name of layer</param>
        public DrawingLayer(string name, bool front)
        {
            LayerName = name;
            Front = front;
        }
        /// <summary>
        /// Layer Name
        /// </summary>
        public readonly string LayerName;
        public readonly bool Front;
        /// <summary>
        /// Class representing one line on a drawing layer
        /// </summary>
        public class Line
        {
            public Line(PointF start, PointF end, double width = 0.15)
            {
                this.start = start;
                this.end = end;
                this.width = width;
            }
            /// <summary>
            /// Start point of line
            /// </summary>
            public PointF start;
            /// <summary>
            /// End point of line
            /// </summary>
            public PointF end;
            /// <summary>
            /// Width of line
            /// </summary>
            public double width;

            /// <summary>
            /// Length of the line
            /// </summary>
            public float Length {
                get
                {
                    double dx = start.X - end.X;
                    double dy = start.Y - end.Y;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
            }

            /// <summary>
            /// Angle of line in degrees
            /// </summary>
            public double Angle
            {
                get
                {
                    return Math.Atan2(-end.Y + start.Y, end.X - start.X) * 180 / Math.PI;
                }
            }

            /// <summary>
            /// Center point of the line
            /// </summary>
            public PointF Center
            {
                get
                {
                    return new PointF((start.X + end.X) / 2, (start.Y + end.Y) / 2);
                }
            }

            public override string ToString()
            {
                return start.ToString() + " => " + end.ToString();
            }
        }

        /// <summary>
        /// Circle on drawing layer
        /// </summary>
        public class Circle
        {
            public Circle(PointF center, double radius, double width)
            {
                this.center = center;
                this.radius = radius;
                this.width = width;
            }
            public PointF center;
            public double radius;
            public double width;
        }


        /// <summary>
        /// Arc, as described in KiCad.
        /// </summary>
        public class Arc
        {
            /// <summary>
            /// Ctor for Arc
            /// </summary>
            /// <param name="center">Center of the circle on which the arc is drawn</param>
            /// <param name="start">Start point of the arc</param>
            /// <param name="angle">angle of the arc</param>
            /// <param name="width">Line width of the arc</param>
            public Arc(PointF center, PointF start, double angle, double width)
            {
                this.center = center;
                this.start = start;
                this.angle = angle;
                this.width = width;
            }

            /// <summary>
            /// End point of the arc
            /// </summary>
            public PointF End
            {
                get
                {

                    PointF delta = new PointF(start.X - center.X, start.Y - center.Y);
                    // Rotate relatives location around owners location.
                    float sin = (float)Math.Sin(angle * 2 * Math.PI / 360);
                    float cos = (float)Math.Cos(angle * 2 * Math.PI / 360);

                    PointF rotated = new PointF(
                            +delta.X * cos - delta.Y * sin,
                            delta.X * sin + delta.Y * cos
                        );

                    return new PointF(center.X + rotated.X, center.Y + rotated.Y);
                }
            }

            /// <summary>
            /// Radius of the arc
            /// </summary>
            public double Radius {
                get
                {
                    double dx = start.X - center.X;
                    double dy = start.Y - center.Y;
                    return Math.Sqrt(dx * dx + dy * dy);
                }
            }

            public PointF center;
            public PointF start;
            public double angle;
            public double width;


        }

        /// <summary>
        /// Text on drawing layer
        /// </summary>
        public class Text_
        {

            public string Text;
            public double Width;
            public double Height;
            public double Angle;
            public double Thickness;
            public PointF Location;
        }

        internal List<Line> lines = new List<Line>();
        internal List<Arc> arcs = new List<Arc>();
        internal List<Circle> circles = new List<Circle>();
        internal List<Text_> texts = new List<Text_>();

        /// <summary>
        /// Add text to layer
        /// </summary>
        /// <returns>The Text object</returns>
        public Text_ AddText(string text, PointF location, double angle = 0, double width = 1.5, double height = 1.5, double thickness = 0.3)
        {
            Text_ text_ = new Text_();
            text_.Text = text;
            text_.Location = location;
            text_.Angle = angle;
            text_.Width = width;
            text_.Height = height;
            text_.Thickness = thickness;
            texts.Add(text_);
            return text_;
        }

        /// <summary>
        /// Add a line to a layer
        /// </summary>
        /// <returns>Line object</returns>
        public Line AddLine(double x1, double y1, double x2, double y2, double width= 0.15)
        {
            return AddLine(new PointF((float)x1, (float)y1), new PointF((float)x2, (float)y2), width);
        }

        /// <summary>
        /// Add a line to a layer
        /// </summary>
        /// <returns>Line object</returns>
        public Line AddLine(PointF start, PointF end, double width = 0.15)
        {
            var line = new Line(start, end, width);
            lines.Add(line);
            LastPoint = end;
            LastAngle = line.Angle;
            return line;

        }

        /// <summary>
        /// Calculates next point at at angle and distance from the current last point.
        /// </summary>
        /// <returns>the next point</returns>
        private PointF NextPoint(float length, double angle)
        {
            double dX = Math.Cos(angle / 180 * Math.PI);
            double dY = Math.Sin(angle / 180 * Math.PI);
            return new PointF(
                    LastPoint.X + (float)dX * length,
                    LastPoint.Y - (float)dY * length
                    );
        }

        /// <summary>
        /// Advances the location of the last point, without drawinf a line
        /// </summary>
        /// <returns>The advanced point</returns>
        public PointF AdvanceLine(float length, double angle)
        {
            var next_point = NextPoint(length, angle);
            LastAngle = Utils.AngleOf(LastPoint, next_point);
            LastPoint = next_point;
            return LastPoint;
        }

        /// <summary>
        /// Continue line at last angle;
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Line ContinueLine(float length)
        {
            return ContinueLine(length, LastAngle);
        }

        /// <summary>
        /// Draw a line at an angle and a distance from the last point
        /// </summary>
        /// <returns>the line object</returns>
        public Line ContinueLine(float length, double angle)
        {

            return ContinueLine(
                NextPoint(length, angle)
                );
        }

        public Arc ContinueArc(double radius, double angle)
        {
            if (angle == 0) throw new Exception("Arc must have an angle");
            PointF center = Utils.PointOnCircle(LastPoint, LastAngle + 90 * Math.Sign(angle) , radius);
            return AddArc(center, LastPoint, -angle);
        }

        /// <summary>
        /// Draw a line to a point
        /// </summary>
        /// <returns>the line object</returns>
        public Line ContinueLine(PointF end)
        {
            return AddLine(LastPoint, end);
        }

        /// <summary>
        /// Adds an arc to the layer
        /// </summary>
        /// <returns>the arc object</returns>
        public Arc AddArc(PointF center, PointF start, double angle, double width = 0.15)
        {
            var arc = new Arc(center, start, angle, width);
            arcs.Add(arc);
            LastPoint = arc.End;
            LastAngle = Utils.AngleOf(center, start) + 90 + angle;
            return arc;
        }

        /// <summary>
        /// Adds a circle to a layer
        /// </summary>
        /// <returns>The circle object</returns>
        public Circle AddCircle(PointF center, double radius, double width = 0.15)
        {
            var res = new Circle(center, radius, width);
            circles.Add(res);
            return res;
        }

        /// <summary>
        /// The last point when drawing a continous line
        /// </summary>
        public PointF LastPoint { get; set; }

        public double LastAngle { get; set; }

        /// <summary>
        /// Create all of the KiCad file data.
        /// </summary>
        /// <returns></returns>
        public string GetKiCadData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var cut in lines)
            {
                sb.AppendFormat("   (gr_line (start {0} {1}) (end {2} {3}) (angle 90) (layer {4}) (width {5}))\n", cut.start.X, cut.start.Y, cut.end.X, cut.end.Y, LayerName, cut.width);
            }
            foreach (var arc in arcs)
            {
                sb.AppendFormat("    (gr_arc (start {0} {1}) (end {2} {3}) (angle {4}) (layer {6}) (width {5}))\n",
                                arc.center.X, arc.center.Y, arc.start.X, arc.start.Y, arc.angle, arc.width, LayerName);

            }
            foreach (var circle in circles)
            {
                sb.AppendFormat("    (gr_circle (center {0} {1}) (end {2} {1}) (layer Edge.Cuts) (width {3}))\n", circle.center.X, circle.center.Y, circle.center.X + circle.radius, circle.width);
            }

            foreach(var text in texts)
            {
                sb.AppendFormat(@"(gr_text ""{0}"" (at {1} {2} {3}) (layer {4})
    (effects(font(size {5} {6})(thickness {7})){8})
  )", text.Text.Replace("\n", "\\n"), text.Location.X, text.Location.Y, text.Angle, LayerName, text.Width, text.Height, text.Thickness,
  Front ? "" :" (justify mirror)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remove all lines and arcs part of paths that go through an area, ensuring there are no holes in it.
        /// </summary>
        /// <param name="rect">Area that will be made sure to be solid</param>
        public void EnsureSolid(RectangleF rect)
        {
            List<PointF> problemPoints = new List<PointF>();
            foreach (var in_area in lines.Where(line => { return rect.Contains(line.start) || rect.Contains(line.end); }))
            {
                problemPoints.Add(in_area.start);
                problemPoints.Add(in_area.end);
            }

            foreach (var in_area in arcs.Where(line => { return rect.Contains(line.start) || rect.Contains(line.End); }))
            {
                problemPoints.Add(in_area.start);
                problemPoints.Add(in_area.End);
            }

            while (problemPoints.Count != 0)
            {
                var toRemove = lines.Where(line => { return problemPoints.Contains(line.start) || problemPoints.Contains(line.end); }).ToList();
                var toRemove2 = arcs.Where(line => { return problemPoints.Contains(line.start) || problemPoints.Contains(line.End); }).ToList();
                problemPoints.Clear();
                foreach (var item in toRemove)
                {
                    problemPoints.Add(item.start);
                    problemPoints.Add(item.end);
                }
                lines.RemoveAll(x => toRemove.Contains(x));

                foreach (var item in toRemove2)
                {
                    problemPoints.Add(item.start);
                    problemPoints.Add(item.End);
                }
                arcs.RemoveAll(x => toRemove2.Contains(x));

            }

        }
         
        /// <summary>
        /// Calculate a rectangle bounding all elements in the layer.
        /// </summary>
        public RectangleF BoundingRect {  get
            {
                float minX = lines[0].start.X;
                float maxX = lines[0].start.X;
                float minY = lines[0].start.Y;
                float maxY = lines[0].start.Y;
                foreach (var line in lines)
                {
                    if (line.start.X < minX) minX = line.start.X;
                    if (line.end.X < minX) minX = line.end.X;
                    if (line.start.X > maxX) maxX = line.start.X;
                    if (line.end.X > maxX) maxX = line.end.X;
                    if (line.start.Y < minY) minY = line.start.Y;
                    if (line.end.Y < minY) minY = line.end.Y;
                    if (line.start.Y > maxY) maxY = line.start.Y;
                    if (line.end.Y > maxY) maxY = line.end.Y;
                }
                foreach (var arc in arcs)
                {
                    if (arc.start.X < minX) minX = arc.start.X;
                    if (arc.End.X < minX) minX = arc.End.X;
                    if (arc.start.X > maxX) maxX = arc.start.X;
                    if (arc.End.X > maxX) maxX = arc.End.X;
                    if (arc.start.Y < minY) minY = arc.start.Y;
                    if (arc.End.Y < minY) minY = arc.End.Y;
                    if (arc.start.Y > maxY) maxY = arc.start.Y;
                    if (arc.End.Y > maxY) maxY = arc.End.Y;
                }
                foreach (var circle in circles)
                {
                    minX = Math.Min(minX, circle.center.X - (float)circle.radius);
                    maxX = Math.Max(maxX, circle.center.X + (float)circle.radius);
                    minY = Math.Min(minY, circle.center.Y - (float)circle.radius);
                    maxY = Math.Max(maxY, circle.center.Y + (float)circle.radius);
                }
                return RectangleF.FromLTRB(minX, minY, maxX, maxY);
            }
        }

        /// <summary>
        /// Create an OpenScad polygon with layer. for now wothout text and circles
        /// </summary>
        /// <param name="thickness">Thickness of the polygon created</param>
        /// <param name="offset_r">Hom much to expand the polygon by. Recommended 0.3 for openscad</param>
        /// <param name="external_only">Only create the external polygon or include inner holes</param>
        /// <returns>OpenScad code</returns>
        public string GetOpenScadCode(double thickness, double offset_r = 0, bool external_only = false)
        {
            var min_x = lines.Min(x => x.start.X);
            var first_line = lines.First(line => (line.start.X == min_x));

            StringBuilder sb = new StringBuilder();

            List<Line> all_as_lines = new List<Line>();
            all_as_lines.AddRange(lines);

            foreach (var arc in arcs)
            {
                Arc other = new Arc(arc.center, arc.start, 0, arc.width);
                PointF last = arc.start;
                // We want to turn ever 0.1mm to a line. That's ever degree at radius of 5.729mm; 
                var dx = arc.start.X - arc.center.X;
                var dy = arc.start.Y - arc.center.Y;

                double radius = Math.Sqrt(dx * dx + dy * dy);
                double step = (36 / (2 * Math.PI)) / radius;
                for (double i = step; i <= arc.angle; i += step)
                {
                    other.angle = i;
                    all_as_lines.Add(new Line(last, other.End));
                    last = other.End;
                }
                all_as_lines.Add(new Line(last, arc.End));
            }

            StringBuilder sb2 = new StringBuilder();
            foreach (var l in all_as_lines)
            {
                sb2.AppendFormat("[{0}, {1}] => [{2}, {3}]\r\n", l.start.X, l.start.Y, l.end.X, l.end.Y);
            }
            if (thickness != 0) sb.AppendFormat("linear_extrude({0}) offset(r = {1}) ", thickness, offset_r);
            sb.AppendFormat("difference() {{\n    polygon(points = [[{0}, {1}]", first_line.start.X, first_line.start.Y);
            sb.AppendFormat(", [{0}, {1}]", first_line.end.X, first_line.end.Y);
            all_as_lines.Remove(first_line);
            var cur = first_line.end;
            int counter = 1;
            do
            {
                var next_line = all_as_lines.FirstOrDefault(x => (Math.Abs(x.start.X - cur.X) < 0.001 && Math.Abs(x.start.Y - cur.Y) < 0.001));
                if (next_line != null)
                {
                    sb.AppendFormat(", [{0}, {1}]", next_line.end.X, next_line.end.Y);
                    cur = next_line.end;
                }
                else
                {
                    next_line = all_as_lines.FirstOrDefault(x => (Math.Abs(x.end.X - cur.X) < 0.001 && Math.Abs(x.end.Y - cur.Y) < 0.001));
                    if (next_line != null)
                    {
                        sb.AppendFormat(", [{0}, {1}]", next_line.start.X, next_line.start.Y);
                        cur = next_line.start;
                    }
                }
                if (counter++ % 4 == 0) sb.Append("\r\n");

                all_as_lines.Remove(next_line);
            } while (Math.Abs(first_line.start.X - cur.X) > 0.001 || Math.Abs(first_line.start.Y - cur.Y) > 0.001);

            sb.AppendLine("]);");



            while (!external_only && all_as_lines.Count > 0)
            {

                first_line = all_as_lines[0];
                sb.Append("polygon(points = [");
                sb.AppendFormat("[{0}, {1}]", first_line.start.X, first_line.start.Y);
                sb.AppendFormat(", [{0}, {1}]", first_line.end.X, first_line.end.Y);
                cur = first_line.end;
                all_as_lines.Remove(first_line);
                do
                {
                    var next_line = all_as_lines.FirstOrDefault(x => (Math.Abs(x.start.X - cur.X) < 0.001 && Math.Abs(x.start.Y - cur.Y) < 0.001));
                    if (next_line != null)
                    {
                        sb.AppendFormat(", [{0}, {1}]", next_line.end.X, next_line.end.Y);
                        cur = next_line.end;
                    }
                    else
                    {
                        next_line = all_as_lines.FirstOrDefault(x => (Math.Abs(x.end.X - cur.X) < 0.001 && Math.Abs(x.end.Y - cur.Y) < 0.001));
                        sb.AppendFormat(", [{0}, {1}]", next_line.start.X, next_line.start.Y);
                        cur = next_line.end;
                    }

                    all_as_lines.Remove(next_line);
                } while (all_as_lines.Count != 0 && Math.Abs(first_line.start.X - cur.X) > 0.001 || Math.Abs(first_line.start.Y - cur.Y) > 0.001);
                sb.AppendLine("]);");

            }

            sb.AppendLine("}");
            return sb.ToString();

        }
    }
}