using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad_sharp_samples
{
    /// <summary>
    /// Simple example of a 555 blinky board
    /// </summary>
    class Blinky555
    {
        public static SizeF size = new SizeF(20, 30);
        public static float corner_r = 3;
        public static void Create()
        {
            KiCad.PCB pcb = new KiCad.PCB();

            var VCC = pcb.Nets.AddNet(1, "VCC");
            var GND = pcb.Nets.AddNet(2, "GND");

            var ic = pcb.Components.AddComponent("Package_DIP", "DIP-8_W7.62mm", "IC555");
            var r1 = pcb.Components.AddComponent("Resistor_THT", "R_Axial_DIN0204_L3.6mm_D1.6mm_P7.62mm_Horizontal", "R1");
            var r2 = pcb.Components.AddComponent("Resistor_THT", "R_Axial_DIN0204_L3.6mm_D1.6mm_P7.62mm_Horizontal", "R2");
            var r3 = pcb.Components.AddComponent("Resistor_THT", "R_Axial_DIN0204_L3.6mm_D1.6mm_P7.62mm_Horizontal", "R3");
            var c1 = pcb.Components.AddComponent("Capacitor_THT", "CP_Radial_D5.0mm_P2.50mm", "C1");
            var c2 = pcb.Components.AddComponent("Capacitor_THT", "C_Disc_D3.0mm_W1.6mm_P2.50mm", "C2");
            var led = pcb.Components.AddComponent("LED_THT", "LED_D5.0mm", "LED1");
            var power = pcb.Components.AddComponent("Connector_PinHeader_2.54mm", "PinHeader_1x02_P2.54mm_Horizontal", "PWR");

            PointF start = new PointF(corner_r, 0);
            pcb.Edge.AddLine(start, new PointF(size.Width - corner_r, 0));
            pcb.Edge.ContinueArc(3, -90);
            pcb.Edge.ContinueLine(new PointF(size.Width, size.Height - corner_r));
            pcb.Edge.ContinueArc(3, -90);
            pcb.Edge.ContinueLine(new PointF(corner_r, size.Height));
            pcb.Edge.ContinueArc(3, -90);
            pcb.Edge.ContinueLine(new PointF(0, corner_r));
            pcb.Edge.ContinueArc(3, -90);
            pcb.Edge.ContinueLine(start);

            ic.Location = new PointF(6, 12);
            r1.Location = new PointF(16.8f, 14.5f);
            r1.Angle = 90;
            r2.Location = new PointF(16.8f, 24.5f);
            r2.Angle = 90;
            c1.Angle = 180;
            c1.Location = new PointF(13.5f, 24.5f);
            c2.Location = new PointF(5.5f, 22.86f);
            r3.Angle = 90;
            r3.Location = new PointF(2, 16.86f);
            led.Angle = 180;
            led.Location = new PointF(8.3f, 5.5f);
            power.Angle = 270;
            power.Location = new PointF(8, 26);

            ic.Pads[4].Net = ic.Pads[8].Net = r1.Pads[2].Net = power.Pads[1].Net = VCC;
            ic.Pads[1].Net = led.Pads[1].Net = c1.Pads[2].Net = c2.Pads[1].Net = power.Pads[2].Net = GND;

            pcb.Traces.SetTraceStart(ic.Pads[3]);
            pcb.Traces.ContinueTrace(r3.Pads[1]);

            pcb.Traces.SetTraceStart(r3.Pads[2]);
            pcb.Traces.ContinueTrace(led.Pads[2]);

            pcb.Traces.SetTraceStart(ic.Pads[7]);
            pcb.Traces.ContinueTrace(r1.Pads[1]);
            pcb.Traces.ContinueTrace(r2.Pads[2]);

            pcb.Traces.SetTraceStart(c1.Pads[1]);
            pcb.Traces.ContinueTrace(r2.Pads[1]);
            pcb.Traces.ContinueTrace(0, -4);
            pcb.Traces.ContinueTrace(ic.Pads[6]);
            pcb.Traces.ContinueTrace(ic.Pads[2]);

            pcb.Traces.SetTraceStart(ic.Pads[5]);
            pcb.Traces.ContinueTrace(c2.Pads[2]);

            var zone_vcc = pcb.Zones.AddZone(VCC, true, pcb.Bounds);
            var zone_gnd = pcb.Zones.AddZone(GND, false, pcb.Bounds);

            pcb.FSilk.AddText("+", power.Pads[1].Location + new SizeF(3, 2.5f));
            pcb.FSilk.AddText("-", power.Pads[2].Location + new SizeF(-3, 2.5f));

            pcb.MoveAll(new SizeF(100, 100));  
            File.WriteAllText("Blinky555.kicad_pcb", pcb.ToString());


        }
    }
}
