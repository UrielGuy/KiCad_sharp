using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KiCad.Components;

namespace KiCad
{
    /// <summary>
    /// Creates a Pogo pin block for a PCB
    /// </summary>
    public class PogoPinAdapter
    {
        /// <summary>
        /// Create the adapter
        /// </summary>
        /// <param name="pcb">PCB to use for the generation</param>
        public PogoPinAdapter(PCB pcb)
        {
            this.pcb = pcb;

            circuit_bounds = pcb.Bounds;
        }

        /// <summary>
        /// Basic shape of a block layer
        /// </summary>
        /// <param name="height">total block height</param>
        /// <param name="board_support_h">height of the supports going into the board</param>
        /// <param name="through_hole_d">diamater of holes going all the way throught the block</param>
        /// <param name="top_hole_d">diamater of top part of pogo hole</param>
        /// <param name="top_hole_offset">Height to start top hole at</param>
        /// <param name="screw_hole_r">Corner screw holes radius</param>
        /// <returns></returns>
        private string BaseShape(double height, double board_support_h, double through_hole_d, double top_hole_d, double top_hole_offset, double screw_hole_r)
        {
            StringBuilder sb = new StringBuilder();
            sb
                .AppendLine("$fn = 90;")
                .AppendLine("difference() {")
                .AppendLine("   union() {")
                // Create basic block shape
                .AppendLine("        difference() {")
                .AppendFormat("             linear_extrude({0}) offset(r =5) offset(delta =5) square([{1}, {2}]);\n", height, circuit_bounds.Width, circuit_bounds.Height)
                .AppendFormat("            translate([-5, -5, 0]) cylinder(30, {0}, {0});\n", screw_hole_r)
                .AppendFormat("             translate([{0} , -5, 0]) cylinder(30, {1}, {1});\n", circuit_bounds.Width + 5, screw_hole_r)
                .AppendFormat("             translate([{0}, {1}, 0]) cylinder(30, {2}, {2});\n", circuit_bounds.Width + 5, circuit_bounds.Height + 5, screw_hole_r)
                .AppendFormat("             translate([-5 , {0}, 0]) cylinder(30, {1}, {1});\n", circuit_bounds.Height + 5, screw_hole_r)
                .AppendLine()
                // Take out the board shape
                .AppendLine(pcb.Edge.GetOpenScadCode(height + 1, 0.3, true))
                .AppendLine("        }")
                .AppendLine();
                // Add supports areas
                foreach (var support in latch_lines) { 
                    sb.AppendFormat("        translate([{0}, {1}, {5}/2]) rotate([0,0,{2}]) cube([{3}, {4}, {5}], center = true);\n",
                    support.Center.X, support.Center.Y, 90 - support.Angle, 6, support.Length + 5, board_support_h);
                }

            sb.AppendLine("    }");

            foreach (var p in pads)
            {
                sb
                    .AppendFormat("translate([{0}, {1}, 25]) rotate([0,0,{2}]) cube([{3},{3},50], center = true) ;\n", p.Location.X, p.Location.Y, 90 - p.Owner.Angle, through_hole_d)
                    .AppendFormat("translate([{0}, {1}, 25 + {3}]) rotate([0,0,{2}]) cube([{4},{4},50], center = true) ;\n", p.Location.X, p.Location.Y, 90 - p.Owner.Angle, top_hole_offset, top_hole_d);
            }
            sb.AppendLine("}");


            return sb.ToString();
        }

        /// <summary>
        /// Create latches catching the board on top layer
        /// </summary>
        /// <returns></returns>
        private string TopLatches()
        {
            StringBuilder sb = new StringBuilder();
            sb
                .AppendLine("difference() {")
                .AppendLine("    union() {");

            foreach (var latch in latch_lines)
            {
                sb
                    .AppendFormat("        translate([{0}, {1}, 0]) rotate([0, 0, {2}]) difference() {{\n", latch.Center.X, latch.Center.Y, 90 - latch.Angle)
                    .AppendFormat("            translate([0,0,4]) cube([7, {0} + 5, 8], center = true);\n", latch.Length)
                    .AppendLine  ("            translate([0,0,2.5]) cube([7, 5, 5], center = true);")
                    .AppendLine  ("            translate([0,6,2.5]) rotate([0, 90, 0]) cylinder(8, 1.5, 1.5, center = true);")
                    .AppendLine  ("            translate([0,-6,2.5]) rotate([0, 90, 0]) cylinder(8, 1.5, 1.5, center = true);")
                    .AppendLine  ("        }");


            }
            sb.AppendLine("    }");
            sb.AppendLine(pcb.Edge.GetOpenScadCode(10, 0.3, true));
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Create latch. actually hardcoded, here for order
        /// </summary>
        /// <returns></returns>
        private string Latch()
        {
            return
 @"$fn = 90;
difference() {
    union() {
        cube([18, 5, 4.5]);
        translate([(18 - 4.5) / 2, 5, 0]) cube([4.5, 3.5, 4.5]);
        translate([(18 - 4.5) / 2, 8.5, 0]) rotate([90, 0, 90]) linear_extrude(4.5) polygon([[0 ,0], [1.3, 0], [0, 4.5]]);
    }
    translate([9, 0, 2.25]) rotate([-90, 0, 0]) cylinder(6, 1.5, 1.5);
    translate([9 - 6, 0, 2.25]) rotate([-90, 0, 0]) cylinder(6, 1.7, 1.7);
    translate([9 + 6, 0, 2.25]) rotate([-90, 0, 0]) cylinder(6, 1.7, 1.7);
}";
        }


        /// <summary>
        /// Create PCB file for the bottom layer if configured
        /// </summary>
        private PCB CreatePCBBottom(BottomMode bottom_mode)
        {
            PCB out_pcb = new PCB();

            float left = pcb.Bounds.Left - 10;
            float top = pcb.Bounds.Top - 10;
            float right = pcb.Bounds.Right + 10;
            float bottom = pcb.Bounds.Bottom + 10;

            out_pcb.Edge.AddLine(left + 5, top, right - 5, top);
            out_pcb.Edge.AddArc(new PointF(right - 5, top + 5), new PointF(right - 5, top), 90);

            out_pcb.Edge.AddLine(right, top + 5, right, bottom - 5);
            out_pcb.Edge.AddArc(new PointF(right - 5, bottom -5), new PointF(right, bottom - 5), 90);

            out_pcb.Edge.AddLine(right - 5, bottom, left + 5, bottom);
            out_pcb.Edge.AddArc(new PointF(left + 5, bottom - 5), new PointF(left + 5, bottom), 90);

            out_pcb.Edge.AddLine(left, bottom - 5, left, top + 5);
            out_pcb.Edge.AddArc(new PointF(left + 5, top + 5), new PointF(left, top + 5), 90);

            out_pcb.Edge.AddCircle(new PointF(left + 5, top + 5), 2.05);
            out_pcb.Edge.AddCircle(new PointF(left + 5, bottom - 5), 2.05);
            out_pcb.Edge.AddCircle(new PointF(right - 5, top + 5), 2.05);
            out_pcb.Edge.AddCircle(new PointF(right - 5, bottom -  5), 2.05);

            Dictionary<Net, Net> nets = new Dictionary<Net, Net>();
            List<Components.Component> testPoints = new List<Component>();
            foreach (var pad in pads)
            {
                nets[pad.Net] = out_pcb.Nets.AddNet(pad.Net.Number, pad.Net.Name);
                var testpoint = out_pcb.Components.AddComponent("TestPoint", "TestPoint_Pad_D1.5mm", pad.Net.Name, pad.Location, pad.Owner.Angle, false);
                testpoint.Pads[1].Net = nets[pad.Net];
                testPoints.Add(testpoint);
            }

            var connector = out_pcb.Components.AddComponent("Connector_PinSocket_2.54mm",
                "PinSocket_1x" + pads.Count.ToString().PadLeft(2, '0') + "_P2.54mm_Horizontal",
                "OUT_CONN_1",
                (right + left) / 2 + 1.27 * (pads.Count - 1),
                bottom - 10.15,
                270,
                false
                );

            pads.Sort((b, a) => (a.Location.X.CompareTo(b.Location.X) != 0 ? a.Location.X.CompareTo(b.Location.X) : b.Location.Y.CompareTo(a.Location.Y)));
            for (int i = 0; i < pads.Count; i++)
            {
                connector.Pads[i + 1].Net = nets[pads[i].Net];
                out_pcb.FSilk.AddText(connector.Pads[i + 1].Net.Name, connector.Pads[i + 1].Location + new SizeF(0, 5.65f), 90, 1, 1, 0.2);
                out_pcb.BSilk.AddText(connector.Pads[i + 1].Net.Name, connector.Pads[i + 1].Location + new SizeF(0, 5.65f), 90, 1, 1, 0.2);
            }

            switch (bottom_mode)
            {
                case BottomMode.PCB_no_connect:
                    out_pcb.FSilk.AddText("This PCB is NOT ready!\nConnect test points to something,\n preferably the connector at the bottom\n autorouter should do well", pcb.Bounds.Center(), 0, 4.5, 4.5, 0.9);
                    break;
                case BottomMode.PCB_connect_direct:
                    for (int i = 0; i < pads.Count; i++)
                    {
                        out_pcb.Traces.SetTraceStart(testPoints[i].Pads[1], 0.6); 
                        out_pcb.Traces.ContinueTrace(connector.Pads[pads.Count - i]);
                    }
                    out_pcb.FSilk.AddText("This PCB is NOT ready!\nVerify all connections are good", pcb.Bounds.Center(), 0, 4.5, 4.5, 0.9);
                    break;
                case BottomMode.PCB_connect_via_grid:
                    for (int i = 0; i < pads.Count; i++)
                    {
                        out_pcb.Traces.SetTraceStart(testPoints[i].Pads[1], 0.6);
                        out_pcb.Traces.ContinueTrace(new PointF(connector.Pads[pads.Count - i].Location.X, testPoints[i].Pads[1].Location.Y));
                        out_pcb.Traces.ContinueWithVia(0.7, 0.6);
                        out_pcb.Traces.ContinueTrace(connector.Pads[pads.Count - i]);
                   }
                    out_pcb.FSilk.AddText("This PCB is NOT ready!\nVerify all connections are good", pcb.Bounds.Center(), 0, 4.5, 4.5, 0.9);
                    break;
                default:
                    throw new NotImplementedException();

            }
            out_pcb.MoveAll(new SizeF(100, 100));
            return out_pcb;
        }

        public enum BottomMode
        {
            Printed,
            PCB_no_connect,
            PCB_connect_direct,
            PCB_connect_via_grid
        }

        /// <summary>
        /// Create files for programming block.
        /// </summary>
        /// <param name="base_name">base bame for files created</param>
        /// <param name="pcb_bottom">should the bottom layer be plastic (with standard headers inserts) or a PCB (needs to be finished manually)</param>
        public void Generate(string base_name, BottomMode bottom_mode)
        {
            if (bottom_mode == BottomMode.Printed)
            {
                File.WriteAllText(base_name + "_bottom.scad", BaseShape(5, 5, 1.1, 3.2, 2, 2.2));
                File.WriteAllText(base_name + "_spacer.scad", BaseShape(16, 16, 1.1, 1.5, 2, 2.2));
            }
            else
            {
                File.WriteAllText(base_name + "_bottom.kicad_pcb", CreatePCBBottom(bottom_mode).ToString());
                File.WriteAllText(base_name + "_spacer_top.scad", BaseShape(10.5, 10.5, 1.5, 1.5, 0, 2.2));

                float left = pcb.Bounds.Left -10;
                float right = pcb.Bounds.Right + 10;
                float bottom = pcb.Bounds.Bottom + 10;

                File.WriteAllText(base_name + "_spacer_bottom.scad", "difference() {\n");
                File.AppendAllText(base_name + "_spacer_bottom.scad", BaseShape(3, 3, 1.5, 1.5, 0, 2.2));
                File.AppendAllText(base_name + "_spacer_bottom.scad", string.Format("translate[{0}, {1}, 0]) cube([{2}, 12.5, 3]);\n}}\n", (left + right) / 2 - 1.27 * pads.Count, bottom - 12.5, 2.54 * pads.Count));

            }
            File.WriteAllText(base_name + "_top.scad", BaseShape(2.8, 1, 1, 0, 0, 1.97));
            File.AppendAllText(base_name + "_top.scad", "translate([0,0,2.8]) { " + TopLatches() + "}");
            File.WriteAllText(base_name + "latch.scad", Latch());
        }

        /// <summary>
        /// Add pads to create a pogo pin for
        /// </summary>
        /// <param name="to_add">Pads to add</param>
        public void AddPads(IEnumerable<Pad> to_add)
        {
            foreach (var pad in to_add)
                AddPad(pad);
        }

        /// <summary>
        /// Add a pad to create a pogo pin for
        /// </summary>
        /// <param name="to_add"></param>
        public void AddPad(Pad to_add)
        {
            if (!to_add.Owner.Front)
            {
                throw new Exception("All pogo pads have to be on the fron layer");
            }
            pads.Add(to_add);
        }

        /// <summary>
        /// Create pogo pin mounts for all pads of a components. useful for test point foot prints.
        /// </summary>
        /// <param name="component"></param>
        public void AddPads(Component component)
        {
            AddPads(component.Pads.Values);
        }

        /// <summary>
        /// Add a latch = this will create a support area next to a line and a latch area on the top layer.
        /// </summary>
        /// <param name="latch">line to use for latch</param>
        public void AddLatch(DrawingLayer.Line latch)
        {
            latch_lines.Add(latch);
        }

        private List<Pad> pads = new List<Pad>();
        private List<DrawingLayer.Line> latch_lines = new List<DrawingLayer.Line>();
        private PCB pcb;
        private RectangleF circuit_bounds;
    }
}
