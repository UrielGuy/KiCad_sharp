using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;

namespace KiCad_sharp_samples
{
    /// <summary>
    /// Example of parametric design and use of round drawing/trace layers
    /// </summary>
    class Rainbow
    {
        static float out_r = 50;
        static float in_r = 25;

        // Try changing this
        static bool const_angle = true;

        static float d_angle = 12;
        static float d_distance = 6;

        public static void Create()
        {
            KiCad.PCB pcb = new KiCad.PCB();
            var GND = pcb.Nets.AddNet(1, "GND");
            var GCC = pcb.Nets.AddNet(2, "VCC");

            var center = new PointF(0, 0);
            pcb.Edge.AddArc(center, new PointF(-out_r, 0), 180);
            pcb.Edge.AddArc(center, new PointF(- in_r, 0), 180);
            pcb.Edge.AddLine(in_r, 0, out_r, 0);
            pcb.Edge.AddLine(-in_r, 0, -out_r, 0);

            int slices = (int)Math.Floor((out_r - in_r - 2) / 5);
            float interval = (out_r - in_r - 2) / slices;
            for (float distance = in_r + 1; distance < out_r; distance += interval)
            {
                pcb.FSilk.AddArc(center, center + new SizeF(-distance, 0), 180);
            }
            int led_counter = 0;

            List<KiCad.Components.Component> firsts = new List<KiCad.Components.Component>();
            for (float distance = in_r + 1 + interval / 2; distance < out_r; distance += interval)
            {
                bool first = true;
                for(float angle = 12; angle < 170; angle += (const_angle ? d_angle : (d_distance * 360f)/(2f * (float)Math.PI * distance)))
                {
                    var led = pcb.Components.AddComponent("LED_THT", "LED_D5.0mm", "LED" + (++led_counter).ToString(), KiCad.Utils.PointOnCircle(center, angle, distance - 1.27), angle);
                    led.Pads[2].Net = GND;
                    if (first)
                    {
                        pcb.Traces.SetTraceStart(led.Pads[1], 0.3);
                        firsts.Add(led);
                        first = false;
                    }
                    else
                    {
                        pcb.Traces.ContinueTraceWithArcToAngle(center, KiCad.Utils.AngleOf(center,  led.Pads[1].Location));
                    }

                }
            }

            var power = pcb.Components.AddComponent("Connector_PinHeader_2.54mm", "PinHeader_1x" +(firsts.Count+1).ToString().PadLeft(2, '0') + "_P2.54mm_Horizontal", "PWR");
            power.Location = new PointF((float)((out_r + in_r) / 2 + firsts.Count * 2.54 / 2), -2);
            power.Angle = 270;
            for (int i = 0; i < firsts.Count; i++)
            {
                pcb.Traces.DrawTrace(firsts[i].Pads[1], power.Pads[firsts.Count + 1 - i], 0.3);
            }

            var zone = pcb.Zones.AddZone(GND, false);
            zone.Points.Add(new PointF(-out_r, 0));
            zone.Points.Add(new PointF(-out_r, -out_r));
            zone.Points.Add(new PointF(out_r, -out_r));
            zone.Points.Add(new PointF(out_r, 0));


            pcb.MoveAll(new SizeF(100 + out_r, 100 + out_r));
            File.WriteAllText("rainbow.kicad_pcb", pcb.ToString());


        }
    }
}
