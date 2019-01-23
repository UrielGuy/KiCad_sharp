using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad_sharp_samples
{
    /// <summary>
    /// An example of a microcontroller with ICSP block + serial breakout. 
    /// Also, lots of auto generated traces with logic.
    /// It's a bit tedious, but there's alot here. 
    /// Check out the pogo pin board at the bottom, try and play with the boolean there and see a board generated for the bottom layer.
    /// </summary>
    class programmer_demo
    {
        public static void Create()
        {
            KiCad.PCB pcb = new KiCad.PCB();
            var atmega328 = pcb.Components.AddComponent("Package_QFP:TQFP-32_7x7mm_P0.8mm", "IC1", 13, 3 + 8 * 2.54, -45);
            var VCC = pcb.Nets.AddNet("VCC");               atmega328.Pads[4].Net = atmega328.Pads[6].Net = VCC;
            var GND = pcb.Nets.AddNet("GND");               atmega328.Pads[3].Net = atmega328.Pads[5].Net = atmega328.Pads[21].Net = GND;
            var AREF = pcb.Nets.AddNet("AREF");             atmega328.Pads[20].Net = AREF;
            var XTAL1 = pcb.Nets.AddNet("XTAL1");           atmega328.Pads[7].Net = XTAL1;
            var XTAL2 = pcb.Nets.AddNet("XTAL2");           atmega328.Pads[8].Net = XTAL2;
            var AVCC = pcb.Nets.AddNet("AVCC");             atmega328.Pads[18].Net = AVCC;
            var NOT_RESET = pcb.Nets.AddNet("NOT_RESET");   atmega328.Pads[29].Net = NOT_RESET;

            var D0_RX = pcb.Nets.AddNet("D0_RX");           atmega328.Pads[30].Net = D0_RX;
            var D1_RX = pcb.Nets.AddNet("D1_TX");           atmega328.Pads[31].Net = D1_RX;
            var D2 = pcb.Nets.AddNet("D2");                 atmega328.Pads[32].Net = D2;
            var D3 = pcb.Nets.AddNet("D3");                 atmega328.Pads[1].Net = D3;
            var D4 = pcb.Nets.AddNet("D4");                 atmega328.Pads[2].Net = D4;
            var D5 = pcb.Nets.AddNet("D5");                 atmega328.Pads[9].Net = D5;
            var D6 = pcb.Nets.AddNet("D6");                 atmega328.Pads[10].Net = D6;
            var D7 = pcb.Nets.AddNet("D7");                 atmega328.Pads[11].Net = D7;

            var C0 = pcb.Nets.AddNet("C0");                 atmega328.Pads[23].Net = C0;
            var C1 = pcb.Nets.AddNet("C1");                 atmega328.Pads[24].Net = C1;
            var C2 = pcb.Nets.AddNet("C2");                 atmega328.Pads[25].Net = C2;
            var C3 = pcb.Nets.AddNet("C3");                 atmega328.Pads[26].Net = C3;
            var C4_SDA = pcb.Nets.AddNet("C4_SDA");         atmega328.Pads[27].Net = C4_SDA;
            var C5_SCL = pcb.Nets.AddNet("C5_SCL");         atmega328.Pads[28].Net = C5_SCL;

            var B0 = pcb.Nets.AddNet("B0");                 atmega328.Pads[12].Net = B0;
            var B1 = pcb.Nets.AddNet("B1");                 atmega328.Pads[13].Net = B1;
            var B2 = pcb.Nets.AddNet("B2");                 atmega328.Pads[14].Net = B2;
            var B3_MOSI = pcb.Nets.AddNet("B3_MOSI");       atmega328.Pads[15].Net = B3_MOSI;
            var B4_MISO = pcb.Nets.AddNet("B4_MISO");       atmega328.Pads[16].Net = B4_MISO;
            var B5_SCK = pcb.Nets.AddNet("B5_SCK");         atmega328.Pads[17].Net = B5_SCK;

            var ANALOG6 = pcb.Nets.AddNet("ANLG6");         atmega328.Pads[19].Net = ANALOG6;
            var ANALOG7 = pcb.Nets.AddNet("ANLG7");         atmega328.Pads[22].Net = ANALOG7;


            PointF start = new PointF(3, 0);
            pcb.Edge.LastAngle = 0;
            pcb.Edge.LastPoint = start;

            List<KiCad.DrawingLayer.Line> lines = new List<KiCad.DrawingLayer.Line>(); ;
            for (int i = 0; i < 2; i++)
            {
                lines.Add(pcb.Edge.ContinueLine(20));
                pcb.Edge.ContinueArc(3, -90);
                lines.Add(pcb.Edge.ContinueLine(2.54f * 16));
                pcb.Edge.ContinueArc(3, -90);
            }
            pcb.Edge.ContinueLine(start);

            pcb.Zones.AddZone(VCC, true, pcb.Bounds);
            pcb.Zones.AddZone(GND, false, pcb.Bounds);

            var leftHeader = pcb.Components.AddComponent("Connector_PinHeader_2.54mm:PinHeader_1x16_P2.54mm_Vertical", "CONN1", 1.27, 3 + 1.27, 0);
            var rightHeader = pcb.Components.AddComponent("Connector_PinHeader_2.54mm:PinHeader_1x16_P2.54mm_Vertical", "CONN2", 26 - 1.27, 3 + 1.27, 0);


            for (int i = 1; i <= 16; i++)
            {
                pcb.Traces.SetTraceStart(atmega328.Pads[i]);
                pcb.Traces.ContinueTraceAngle(atmega328.Angle + atmega328.Pads[i].RelativeAngle + 180, 1.1);
                pcb.Traces.ContinueTrace(0, leftHeader.Pads[i].Location.Y - pcb.Traces.LastPoint.Y);
                pcb.Traces.ContinueTrace(leftHeader.Pads[i]);

                pcb.FSilk.AddText(leftHeader.Pads[i].Net.Name, leftHeader.Pads[i].Location + new SizeF(4, 0), 1, 1);
                pcb.BSilk.AddText(leftHeader.Pads[i].Net.Name, leftHeader.Pads[i].Location + new SizeF(6, 0), 1, 1);
            }

            var via_points = new List<PointF>();
            for (int i = 1; i <= 16; i++)
            {
                pcb.Traces.SetTraceStart(atmega328.Pads[33 - i]);
                pcb.Traces.ContinueTraceAngle(atmega328.Angle + atmega328.Pads[33 - i].RelativeAngle, 1.1);
                via_points.Add(pcb.Traces.ContinueTrace(0, rightHeader.Pads[i].Location.Y - pcb.Traces.LastPoint.Y));
                pcb.Traces.ContinueTrace(rightHeader.Pads[i]);
                pcb.FSilk.AddText(rightHeader.Pads[i].Net.Name, rightHeader.Pads[i].Location + new SizeF(-4, 0), 1, 1);
                pcb.BSilk.AddText(rightHeader.Pads[i].Net.Name, rightHeader.Pads[i].Location + new SizeF(-6, 0), 1, 1);

            }

            List<KiCad.Components.Component> test_points = new List<KiCad.Components.Component>();
            for (int i = 0; i < 8; i++)
            {
                test_points.Add(pcb.Components.AddComponent("TestPoint:TestPoint_Pad_D1.5mm", "TP" + (i + 1).ToString(), 3 + 1.27 + i * 2.54, 1.27));
            }

            test_points[0].Pads[1].Net = VCC;
            test_points[7].Pads[1].Net = GND;
            pcb.Traces.SetTraceStart(test_points[7].Pads[1], 0.4);
            pcb.Traces.ContinueTrace(0, 2);
            pcb.Traces.ContinueWithVia(0.6, 0.4);

            pcb.Traces.SetTraceStart(atmega328.Pads[15]);
            pcb.Traces.ContinueTraceAngle(45, 1);
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(new PointF(pcb.Traces.LastPoint.X, test_points[1].Pads[1].Location.Y + 2.5f));
            pcb.Traces.ContinueTrace(new PointF(test_points[1].Pads[1].Location.X, pcb.Traces.LastPoint.Y));
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(test_points[1].Pads[1]);

            pcb.Traces.SetTraceStart(atmega328.Pads[16]);
            pcb.Traces.ContinueTraceAngle(45, 1);
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(new PointF(pcb.Traces.LastPoint.X, test_points[2].Pads[1].Location.Y + 2f));
            pcb.Traces.ContinueTrace(new PointF(test_points[2].Pads[1].Location.X, pcb.Traces.LastPoint.Y));
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(test_points[2].Pads[1]);

            pcb.Traces.SetTraceStart(atmega328.Pads[17]);
            pcb.Traces.ContinueTraceAngle(135, 1.2);
            pcb.Traces.ContinueTrace(0, -3);
            pcb.Traces.ContinueTrace(1, 0);
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(-1, -1);
            pcb.Traces.ContinueTrace(new PointF(pcb.Traces.LastPoint.X, test_points[3].Pads[1].Location.Y + 1.5f));
            pcb.Traces.ContinueTrace(new PointF(test_points[3].Pads[1].Location.X, pcb.Traces.LastPoint.Y));
            pcb.Traces.ContinueWithVia(0.45, 0.3);
            pcb.Traces.ContinueTrace(test_points[3].Pads[1]);

            for (int i = 0; i < 3; i++)
            {
                pcb.Traces.DrawVia(via_points[i + 1], 0.45, 0.3, rightHeader.Pads[i + 2].Net);
                pcb.Traces.ContinueTrace(new PointF(pcb.Traces.LastPoint.X, test_points[i + 4].Location.Y + 1.5f + i * 0.5f));
                pcb.Traces.ContinueTrace(new PointF(test_points[i+4].Pads[1].Location.X, pcb.Traces.LastPoint.Y));
                pcb.Traces.ContinueWithVia(0.45, 0.3);
                pcb.Traces.ContinueTrace(test_points[i+4].Pads[1]);


            }


            System.IO.File.WriteAllText("Atemga328_breakout.kicad_pcb", pcb.ToString());

            KiCad.PogoPinAdapter pga = new KiCad.PogoPinAdapter(pcb);
            pga.AddLatch(lines[0]);
            pga.AddLatch(lines[2]);
            foreach (var tp in test_points) pga.AddPads(tp);
            // Try to change this for true and see the bottom layer PCB created.
            pga.Generate("atmega_programmer", KiCad.PogoPinAdapter.BottomMode.PCB_connect_direct);

            
        }
    }
}
