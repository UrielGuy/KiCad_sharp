using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    /// <summary>
    /// Describes a zone in a KiCad board
    /// </summary>
    public class Zone
    {
        public List<PointF> Points = new List<PointF>();
        public bool Front = false;
        public Net Net;
        public double HatchEdge = 0.508;
        public double ThermalBridgeWidth = 0.508;
        public double Clearance = 0.05;
        public double ThermalGap = 0.2;

        public override string ToString()
        {
            StringBuilder points = new StringBuilder();
            foreach (var p in Points)
            {
                points.AppendFormat("(xy {0} {1})\r\n", p.X, p.Y);
            }
            return string.Format(
            @"(zone (net {0}) (net_name {1}) (layer {2}.Cu) (tstamp 5A0BD152) (hatch edge {3})
    (connect_pads (clearance {4}))
    (min_thickness {4})
    (fill yes (mode segment) (arc_segments 32) (thermal_gap {5}) (thermal_bridge_width {6}))
    (polygon
      (pts
        {7}
      )
    )
)", Net.Number, Net.Name, Front ? "F" : "B", HatchEdge, Clearance, ThermalGap, ThermalBridgeWidth,
            points.ToString());
        }
    }

    /// <summary>
    /// Zones on a KicadBoard
    /// </summary>
    public class Zones
    {
        public Zone AddZone(Net net, bool front)
        {
            Zone zone = new Zone();
            Zones_.Add(zone);
            zone.Net = net;
            zone.Front = front;
            return zone;
        }

        public Zone AddZone(Net net, bool front, RectangleF area)
        {
            var zone = AddZone(net, front);
            zone.Points.Add(new PointF(area.Left, area.Top));
            zone.Points.Add(new PointF(area.Right, area.Top));
            zone.Points.Add(new PointF(area.Right, area.Bottom));
            zone.Points.Add(new PointF(area.Left, area.Bottom));
            return zone;
        }

        public List<Zone> Zones_ = new List<Zone>();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var z in Zones_)
            {
                sb.AppendLine(z.ToString());
            }
            return sb.ToString();
        }
    }
}
