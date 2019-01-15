using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    /// <summary>
    /// The represents a KiCad pcb file. 
    /// </summary>
    public class PCB
    {

        /// <summary>
        /// Nets on the PCB
        /// </summary>
        public readonly Nets Nets = new Nets();
        /// <summary>
        /// Edge cuts for the PCB
        /// </summary>
        public readonly DrawingLayer Edge = new DrawingLayer("Edge.Cuts", true);
        /// <summary>
        /// Front silkscreen layer
        /// </summary>
        public readonly DrawingLayer FSilk = new DrawingLayer("F.SilkS", true);
        /// <summary>
        /// Back silkscreen layer
        /// </summary>
        public readonly DrawingLayer BSilk = new DrawingLayer("B.SilkS", false);
        /// <summary>
        /// Components located on the PCB
        /// </summary>
        public readonly Components Components = new Components();
        /// <summary>
        ///  All traces on the PCB
        /// </summary>
        public readonly Traces Traces = new Traces();
        /// <summary>
        /// All zones.
        /// </summary>
        public readonly Zones Zones = new Zones();

        /// <summary>
        /// Offset the whole PCB by size
        /// </summary>
        public void MoveAll(SizeF size)
        {
            foreach (var c in Components.Components_) c.Value.Location += size;
            foreach (var z in Zones.Zones_)
            {
                for (int i = 0; i < z.Points.Count(); i++) z.Points[i] = z.Points[i] + size;
            }
            foreach (var dl in new DrawingLayer[] {  Edge, FSilk, BSilk})
            {
                foreach (var l in dl.lines)
                {
                    l.start += size;
                    l.end += size;
                }
                foreach (var l in dl.arcs)
                {
                    l.start += size;
                    l.center += size;
                }
                foreach (var l in dl.circles)
                {
                    l.center += size;
                }
                foreach (var l in dl.texts)
                {
                    l.Location += size;
                }
            }
            foreach (var t in Traces.Traces_)
            {
                t.From += size;
                t.To += size;
            }
            foreach (var v in Traces.Vias)
            {
                v.Location += size;
            }
        }

        /// <summary>
        /// Bounding rectangle of the pcb.
        /// </summary>
        public RectangleF Bounds
        {
            get
            {
                return Edge.BoundingRect;
            }
        }

        /// <summary>
        /// Get the string for the KiCad pcbnew file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"(kicad_pcb (version 4) (host pcbnew 4.0.7)

  (page A3)
  (layers
    (0 F.Cu signal)
    (31 B.Cu signal)
    (32 B.Adhes user)
    (33 F.Adhes user)
    (34 B.Paste user)
    (35 F.Paste user)
    (36 B.SilkS user)
    (37 F.SilkS user)
    (38 B.Mask user)
    (39 F.Mask user)
    (40 Dwgs.User user)
    (41 Cmts.User user)
    (42 Eco1.User user)
    (43 Eco2.User user)
    (44 Edge.Cuts user)
    (45 Margin user)
    (46 B.CrtYd user)
    (47 F.CrtYd user)
    (48 B.Fab user)
    (49 F.Fab user)
  )

  (setup
    (last_trace_width 0.25)
    (user_trace_width 0.11111)
    (user_trace_width 0.15)
    (user_trace_width 0.2)
    (user_trace_width 0.25)
    (user_trace_width 0.4)
    (trace_clearance 0.2)
    (zone_clearance 0.508)
    (zone_45_only no)
    (trace_min 0.1)
    (segment_width 0.2)
    (edge_width 0.15)
    (via_size 0.6)
    (via_drill 0.4)
    (via_min_size 0.3)
    (via_min_drill 0.2)
    (user_via 0.3 0.2)
    (user_via 0.6 0.4)
    (uvia_size 0.3)
    (uvia_drill 0.1)
    (uvias_allowed no)
    (uvia_min_size 0.2)
    (uvia_min_drill 0.1)
    (pcb_text_width 0.3)
    (pcb_text_size 1.5 1.5)
    (mod_edge_width 0.15)
    (mod_text_size 1 1)
    (mod_text_width 0.15)
    (pad_size 1.524 1.524)
    (pad_drill 0.762)
    (pad_to_mask_clearance 0.2)
    (aux_axis_origin 0 0)
    (visible_elements 7FFFFFFF)
    (pcbplotparams
      (layerselection 0x00030_80000001)
      (usegerberextensions false)
      (excludeedgelayer true)
      (linewidth 0.100000)
      (plotframeref false)
      (viasonmask false)
      (mode 1)
      (useauxorigin false)
      (hpglpennumber 1)
      (hpglpenspeed 20)
      (hpglpendiameter 15)
      (hpglpenoverlay 2)
      (psnegative false)
      (psa4output false)
      (plotreference true)
      (plotvalue true)
      (plotinvisibletext false)
      (padsonsilk false)
      (subtractmaskfromsilk false)
      (outputformat 1)
      (mirror false)
      (drillshape 1)
      (scaleselection 1)
      (outputdirectory """"))
  )
");
            foreach(var net in Nets.ByNum.Values.OrderBy(s => s.Number))
            {
                sb.AppendFormat("  (net {0} \"{1}\")\n", net.Number, net.Name);
            }

            sb.Append(@"(net_class Default ""This is the default net class.""
    (clearance 0.2)
    (trace_width 0.25)
    (via_dia 0.6)
    (via_drill 0.4)
    (uvia_dia 0.3)
    (uvia_drill 0.1)
");
            foreach (var net in Nets.ByNum.Values.OrderBy(s => s.Number))
            {
                sb.AppendFormat("  (add_net \"{0}\")\n", net.Name);
            }
            sb.Append("  )\n");
            sb.Append(Components.ToString());
            sb.Append(Traces.ToString());
            sb.Append(Edge.GetKiCadData());
            sb.Append(FSilk.GetKiCadData());
            sb.Append(BSilk.GetKiCadData());
            sb.Append(Zones.ToString());
            sb.Append(")");

            return sb.ToString();
        }
    }
}
