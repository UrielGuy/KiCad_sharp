using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    /// <summary>
    /// Class to manage traces on a PCB
    /// </summary>
    public class Traces
    {
        /// <summary>
        /// Describes a piece of trace on a board
        /// </summary>
        public class Trace
        {
            public PointF From;
            public PointF To;
            public bool front;
            public double Width;
            public Net net;
        }

        /// <summary>
        /// Describes a via on a board
        /// </summary>
        public class Via
        {
            public PointF Location;
            public Net Net;
            public double Size;
            public double DrillSize;
        }

        /// <summary>
        /// All traces on a board
        /// </summary>
        public List<Trace> Traces_ = new List<Trace>();
        /// <summary>
        /// All vias on a board
        /// </summary>
        public List<Via> Vias = new List<Via>();

        /// <summary>
        /// The last point a trace was drwan to
        /// </summary>
        PointF lastPoint;
        /// <summary>
        /// indicator for layer of last trace drawn
        /// </summary>
        bool lastFront;
        /// <summary>
        /// Net of currentyl drawen trace
        /// </summary>
        Net lastNet;
        /// <summary>
        /// Width of current trace
        /// </summary>
        double lastWidth;

        /// <summary>
        /// The last point a trace was drwan to
        /// </summary>
        public PointF LastPoint { get { return lastPoint; } }
        /// <summary>
        /// Net of currentyl drawen trace
        /// </summary>
        public Net LastNet { get { return lastNet; } }

        /// <summary>
        /// Continue trace to usng deltas
        /// </summary>
        /// <param name="dX">delta X</param>
        /// <param name="dY">delta Y</param>
        /// <returns>Next point</returns>
        public PointF ContinueTrace(double dX, double dY)
        {
            return ContinueTrace(new SizeF((float)dX, (float)dY));
        }
        /// <summary>
        /// Continue trace with angle and distance
        /// </summary>
        /// <param name="angle">absolute angle to continue trace with</param>
        /// <param name="distance">distance of the trace to be drwn</param>
        /// <returns>Next point</returns>
        public PointF ContinueTraceAngle(double angle, double distance)
        {
            double dx = distance * Math.Cos(angle * Math.PI / 180);
            double dy = distance * Math.Sin(angle * Math.PI / 180);
            return ContinueTrace(dx, -dy);
        }

        /// <summary>
        /// Continue trace to usng delta
        /// </summary>
        /// <returns>Next point</returns>
        public PointF ContinueTrace(SizeF size)
        {
            var dest = lastPoint + size;
            ContinueTrace(dest);
            return dest;
        }

        /// <summary>
        /// Draw trace to an absolute point
        /// </summary>
        /// <param name="to">point to draw to</param>
        /// <returns>The next point</returns>
        public PointF ContinueTrace(PointF to)
        {
            return DrawTrace(lastPoint, to, lastNet, lastFront, lastWidth);
        }

        /// <summary>
        /// Continue trace to a pad. This will adjust the net on the pad
        /// </summary>
        /// <param name="to">Pad to advance to</param>
        /// <returns>Next point</returns>
        public PointF ContinueTrace(Components.Pad to)
        {
            to.Net = lastNet;
            return DrawTrace(lastPoint, to.Location, lastNet, lastFront, lastWidth);
        }

        /// <summary>
        /// Start new trace from pad
        /// </summary>
        /// <returns></returns>
        public PointF SetTraceStart(Components.Pad pad, double width = 0.15)
        {
            return SetTraceStart(pad.Location, pad.Net, pad.Owner.Front, width);
        }

        /// <summary>
        /// Start new trace from via
        /// </summary>
        public PointF SetTraceStart(Traces.Via via, bool front, double width = 0.15)
        {
            return SetTraceStart(via.Location, via.Net, front, width);
        }

        /// <summary>
        /// Start new trace at point
        /// </summary>
        public PointF SetTraceStart(PointF from, Net net, bool front = false, double width = 0.15)
        {
            lastFront = front;
            lastWidth = width;
            lastPoint = from;
            lastNet = net;
            return from;
        }

        /// <summary>
        /// Drawe a trace from pad with delta
        /// </summary>
        public PointF DrawTrace(Components.Pad from, double dX, double dY, double width = 0.15)
        {
            return DrawTrace(from, new SizeF((float)dX, (float)(dY)), width);
        }

        /// <summary>
        /// Draw a trace from a point with a delta
        /// </summary>
        public PointF DrawTrace(PointF from, double dX, double dY, Net net, bool front = false, double width = 0.15)
        {
            return DrawTrace(from, new SizeF((float)dX, (float)dY), net, front, width);
        }

        /// <summary>
        /// Drawe a trace from pad with delta
        /// </summary>
        public PointF DrawTrace(Components.Pad from, SizeF size, double width = 0.15)
        {
            var dest = from.Location + size;
            return DrawTrace(from, dest, width);
        }

        /// <summary>
        /// Draw a trace from a point with a delta
        /// </summary>
        public PointF DrawTrace(PointF from, SizeF size, Net net, bool front = false, double width = 0.15)
        { 
            var dest = from + size;
            return DrawTrace(from, dest, net, front, width);
        }

        /// <summary>
        /// Draw trace from pad to point
        /// </summary>
        public PointF DrawTrace(Components.Pad pad, PointF to, double width = 0.15)
        {
            return DrawTrace(pad.Location, to, pad.Net, pad.Owner.Front, width);
        }

        /// <summary>
        /// Draw trace from pad to pad
        /// </summary>
        public PointF DrawTrace(Components.Pad pad, Components.Pad to, double width = 0.15)
        {
            return DrawTrace(pad.Location, to.Location, pad.Net, pad.Owner.Front, width);
        }

        /// <summary>
        /// Draw trace from point to point
        /// </summary>
        public PointF DrawTrace(PointF from, PointF to, Net net, bool front = false, double width = 0.15)
        {
            lastPoint = to;
            lastNet = net;
            lastFront = front;
            lastWidth = width;
            Trace newTrace = new Trace();
            newTrace.From = from;
            newTrace.To = to;
            newTrace.net = net;
            newTrace.front = front;
            newTrace.Width = width;
            Traces_.Add(newTrace);
            return to;
        }


        /// <summary>
        /// Continue a trace from current point toward a center of a circle
        /// </summary>
        /// <param name="center">center of circle to draw towards</param>
        /// <param name="distance">Length of trace. positive numbers towards center, negative out</param>
        public PointF ContinueTraceTowards(PointF center, double distance)
        {
            double dy = lastPoint.Y - center.Y;
            double dx = lastPoint.X - center.X;
            double start_angle = Math.Atan2(-(dy), dx) * 180 / Math.PI;
            double radius = Math.Sqrt(dx * dx + dy * dy);

            var in_radians2 = (start_angle) / 180 * Math.PI;
            var cos2 = Math.Cos(in_radians2);
            var sin2 = Math.Sin(in_radians2);
            var next2 = new PointF(
                (float)(center.X + (radius - distance) * cos2),
                (float)(center.Y - (radius - distance) * sin2)
                );
            return ContinueTrace(next2);

        }

        /// <summary>
        /// Continue a trace to a specified distance from a center towards the last point.
        /// </summary>
        /// <param name="center">center to draw towards</param>
        /// <param name="distance">distance of new point from center</param>
        /// <returns></returns
        public PointF ContinueTraceDistanceOnCircle(PointF center, double distance)
        {
            return ContinueTraceTowards(center, Utils.Distance(lastPoint, center) - distance);
        }

        /// <summary>
        /// Draw arc of certain degrees
        /// </summary>
        /// <param name="center">center of arc</param>
        /// <param name="d_angle">amount of degrees. can be negative</param>
        /// <returns></returns>
        public PointF ContinueTraceWithArc(PointF center, double d_angle)
        {
            double dy = lastPoint.Y - center.Y;
            double dx = lastPoint.X - center.X;
            double start_angle = Math.Atan2(-(dy), dx) * 180 / Math.PI;
            return ContinueTraceWithArcToAngle(center, start_angle + d_angle);
        }

        /// <summary>
        /// Continue trace with arc of a certain circumfrance length
        /// </summary>
        /// <param name="center">center of arc</param>
        /// <param name="length">circumfrence length of arc.</param>
        /// <returns></returns>
        public PointF ContinueTraceWithArcLength(PointF center, double length)
        {
            double angle = 360 * length / (Utils.Distance(lastPoint, center) * 2 * Math.PI);
            return ContinueTraceWithArc(center, angle);
        }

        /// <summary>
        /// Continue trace with an arc, ending at a certain absolute angle from the center
        /// </summary>
        /// <param name="center">center of arc</param>
        /// <param name="angle">arc will end at this angle</param>
        /// <returns></returns>
        public PointF ContinueTraceWithArcToAngle(PointF center, double angle)
        {
            double dy = lastPoint.Y - center.Y;
            double dx = lastPoint.X - center.X;
            double start_angle = Math.Atan2(-(dy), dx) * 180 / Math.PI;
            double radius = Math.Sqrt(dx * dx + dy * dy);
            // Will create a step every mm
            double step = Math.Sign(angle - start_angle) * ((360 / (2 * Math.PI)) / radius);
            
            for (double cur_angle = start_angle + step; Math.Abs(cur_angle - angle) > Math.Abs(step); cur_angle += step)
            {
                var in_radians = cur_angle / 180 * Math.PI;
                var cos = Math.Cos(in_radians);
                var sin = Math.Sin(in_radians);
                var next = new PointF(
                    (float)(center.X + radius * cos),
                    (float)(center.Y - radius * sin)
                    );
                ContinueTrace(next);
            }

            var in_radians2 = (angle) / 180 * Math.PI;
            var cos2 = Math.Cos(in_radians2);
            var sin2 = Math.Sin(in_radians2);
            var next2 = new PointF(
                (float)(center.X + radius * cos2),
                (float)(center.Y - radius * sin2)
                );
            return ContinueTrace(next2);
        }

        /// <summary>
        /// Continue trace with a via. this switches the drawing layer
        /// </summary>
        public Via ContinueWithVia(double size, double drill)
        {
            //lastFront = !lastFront;
            return DrawVia(lastPoint, size, drill, lastNet, !lastFront);
        }

        /// <summary>
        /// Draw a via on the board
        /// </summary>
        public Via DrawVia(PointF where, double size, double drill, Net net, bool last_front = false)
        {
            Via via = new Via();
            via.Location = where;
            via.Size = size;
            via.DrillSize = drill;
            via.Net = net;
            Vias.Add(via);
            lastNet = net;
            lastPoint = where;
            lastFront = last_front;
            return via;
        }

        /// <summary>
        /// Return string of all traces on the board
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var via in Vias)
            {
                sb.AppendFormat("(via(at {0} {1})(size {2})(drill {3})(layers F.Cu B.Cu)(net {4}))\r\n",
                    via.Location.X, via.Location.Y, via.Size, via.DrillSize, via.Net.Number);

            }
            foreach (var trace in Traces_)
            {
                sb.AppendFormat("(segment (start {0} {1}) (end {2} {3})  (width {4}) (layer {5}.Cu)",
                    trace.From.X, trace.From.Y, trace.To.X, trace.To.Y, trace.Width, trace.front ? "F" : "B");

                if (trace.net != null) sb.AppendFormat("(net {0})", trace.net.Number); ;
                sb.Append(")\r\n");

            }
            return sb.ToString();
        }
    }
}
