using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    
    /// <summary>
    ///  Class representing all of the components on a PCB
    /// </summary>
    public class Components
    {
        /// <summary>
        /// Representation of a pad belonging to a component
        /// </summary>
        public class Pad
        {
            private static int fake_ids = 1000000;
            internal Pad(Component owner, string text)
            {
                this.Owner = owner;
                var words_list = text.Split(' ').ToList();
                string x_str = words_list[words_list.IndexOf("(at") + 1];
                string y_str = words_list[words_list.IndexOf("(at") + 2];
                string angle_str = "0";
                if (!y_str.EndsWith(")"))
                {
                    angle_str = words_list[words_list.IndexOf("(at") + 3].TrimEnd(')');
                }
                else
                {
                    y_str = y_str.TrimEnd(')');
                }
                RelativeLocation.X = float.Parse(x_str);
                RelativeLocation.Y = float.Parse(y_str);
                RelativeAngle = float.Parse(angle_str);

                Name = words_list[words_list.IndexOf("(pad") + 1];
                if (!int.TryParse(Name, out Id)) Id = fake_ids++; ;

                var at_index = text.IndexOf("(at");
                var at_end_index = text.IndexOf(")", at_index);
                Text = text.Remove(at_index, at_end_index - at_index + 1).Insert(at_index, "$LOCATION");
            }
            /// <summary>
            /// Component owning this pad
            /// </summary>
            public readonly Component Owner;
            /// <summary>
            /// Pad name
            /// </summary>
            public string Name;
            /// <summary>
            /// Text in KiCad  for the pad
            /// </summary>
            private string Text;
            /// <summary>
            /// Pad ID
            /// </summary>
            public readonly int Id;
            /// <summary>
            /// Location of pad relative to owner's location
            /// </summary>
            public PointF RelativeLocation;
            /// <summary>
            /// Angle relative to the owner.
            /// </summary>
            public double RelativeAngle;
            /// <summary>
            /// Absolute location calculated from the relative location, the owner and the angle.
            /// </summary>
            public PointF Location { get
                {
                    float sin = (float)Math.Sin((Owner.Front ? 1 : -1) * Owner.Angle * 2 * Math.PI / 360);
                    float cos = (float)Math.Cos((Owner.Front ? 1 : -1) * Owner.Angle * 2 * Math.PI / 360);

                    // Rotate relatives location around owners location.
                    return new PointF(
                            Owner.Location.X +  ((Owner.Front ? 1 : -1)  * (RelativeLocation.X * cos + RelativeLocation.Y * sin)),
                            Owner.Location.Y - RelativeLocation.X * sin + RelativeLocation.Y * cos
                        );

                }
            }
            /// <summary>
            /// Net for the pad
            /// </summary>
            public Net Net;
            /// <summary>
            /// Create KiCad representation of the pad
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var res = Text.Replace("$LOCATION", string.Format("(at {0} {1} {2})", RelativeLocation.X, RelativeLocation.Y, Owner.Angle + RelativeAngle));
                if (Net != null)
                {
                    res = res.Insert(
                        res.LastIndexOf(')'),
                        "(net " + Net.Number + " \"" + Net.Name + "\")");
                }
                return res;
            }
        }


        /// <summary>
        /// Class represnting a component, or rather a footprint.
        /// </summary>
        public class Component
        {
            /// <summary>
            /// Ctor for component
            /// </summary>
            /// <param name="uri">Uri of the component text.</param>
            /// <param name="refernce">reference of the component</param>
            public Component(string uri, string refernce)
            {
                text_key = uri;
                Reference = refernce;

                Pads = new Dictionary<int, Pad>();
                var lines = Components.m_component_texts[uri].Split('\r', '\n').Where(s => s.Contains("(pad "));
                foreach (string pad_line in lines)
                {
                    Pad pad = new Pad(this, pad_line);
                    Pads[pad.Id] = pad;
                }
            }

            /// <summary>
            /// Get the KiCad text for the component
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string res = m_component_texts[text_key];

                int ref_index = res.IndexOf("fp_text reference ") + 18;
                int ref_end = res.IndexOfAny(new char[] { ' ', ')' }, ref_index);
                res = res.Remove(ref_index, ref_end - ref_index);
                res = res.Insert(ref_index, Reference);

                int pads_index = res.IndexOf("(pad");
                int work_index = pads_index;
                while (work_index != -1)
                {
                    res = res.Remove(work_index, res.IndexOf('\n', work_index) - work_index + 1);
                    work_index = res.IndexOf("(pad");
                }
                foreach (var pad in Pads.Values)
                {
                    res = res.Insert(pads_index, pad.ToString() + Environment.NewLine);
                }

                if (!Front)
                {
                    foreach (var s in new string[] { "Cu", "Paste", "Mask", "SilkS", "Fab", "CrtYd" })
                    {
                        res = res.Replace("F." + s, "!!!@@@###$$$");
                        res = res.Replace("B." + s, "F." + s);
                        res = res.Replace("!!!@@@###$$$", "B." + s);
                    }

                    foreach (var s in new string[] { "at", "start", "end" })
                    {
                        res = res.Replace(s + " ", s + " -");
                        res = res.Replace(s + " --", s + " ");
                        res = res.Replace(s + " -(", s + " (");
                    }
                }
                res = res.Insert(res.IndexOf('\n') + 1, string.Format("  (at {0} {1} {2})\n", Location.X, Location.Y, Angle));

                return res;
                
            }
            /// <summary>
            /// </summary>
            public readonly string Reference;
            public PointF Location;
            public double Angle;
            public bool Front = true;
            /// <summary>
            /// All of the pads for the component. ID is according to the footprint pad number, not zero based
            /// </summary>
            public readonly Dictionary<int, Pad> Pads;
            private string text_key;
        }
        /// <summary>
        /// All of the component in the class.
        /// </summary>
        public Dictionary<string, Component> Components_ = new Dictionary<string, Component>();
        /// <summary>
        /// Cache of raw texts loaded from URIs.
        /// </summary>
        static protected Dictionary<string, string> m_component_texts = new Dictionary<string, string>();


        public Component AddComponent(string library_and_footprint, string fp_ref, double x = 0, double y = 0, double angle = 0, bool front = true)
        {
            var parts = library_and_footprint.Split(':');
            if (parts.Length != 2) throw new ArgumentException();
            return AddComponent(parts[0], parts[1], fp_ref, x, y, angle, front);
        }

        /// <summary>
        /// Load a component and add it to the PCB
        /// </summary>
        /// <param name="library">Library name, from KiCad component library</param>
        /// <param name="footprint">Name of footprint to add</param>
        /// <param name="fp_ref">Component ref</param>
        /// <returns></returns>
        public Component AddComponent(string library, string footprint, string fp_ref, double x = 0, double y = 0, double angle = 0, bool front = true)
        {
            return AddComponent(library, footprint, fp_ref, new PointF((float)x, (float)y), angle, front);
        }
        /// <summary>
        /// Load a component and add it to the PCB
        /// </summary>
        /// <param name="library">Library name, from KiCad component library. Supported locally or from GitHub</param>
        /// <param name="footprint">Name of footprint to add</param>
        /// <param name="fp_ref">Component ref</param>
        /// <returns></returns>
        public Component AddComponent(string library, string footprint, string fp_ref, PointF location, double angle = 0, bool front = true)
        {
            if (Components_.ContainsKey(fp_ref))
            {
                throw new Exception("reference already exists");
            }
            string key = library + '/' + footprint;
            if (!m_component_texts.ContainsKey(key))
            {
                var libraries = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables("%appdata%\\kicad\\fp-lib-table"));
                string library_entry = libraries.FirstOrDefault(s => s.Contains("(name " + library + ")"));
                if (library_entry == null || library_entry == "") return null;
                var words = library_entry.Split(' ', '(', ')').ToList();
                string type = words[words.IndexOf("type") + 1];
                string uri = words[words.IndexOf("uri") + 1];

                if (type == "KiCad")
                {
                    var path = uri.Replace("${KISYSMOD}", @"C:\Program Files (x86)\KiCad\share\kicad\modules") + "\\" + footprint + ".kicad_mod";
                    m_component_texts[key] = File.ReadAllText(path);
                }
                else if (type.ToLower() == "github")
                {
                    if (!Directory.Exists("github_cache"))
                    {
                        Directory.CreateDirectory("github_cache");
                    }
                    uri = uri.Replace("${KIGITHUB}", "https://raw.githubusercontent.com/KiCad");
                    HttpClient client = new HttpClient();
                    string req_addr = uri + "/master/" + footprint + ".kicad_mod";
                    string cache_name = "github_cache\\" + req_addr.Replace(":", "_").Replace("\\", "_").Replace("/", "_");
                    if (File.Exists(cache_name) && File.GetLastWriteTime(cache_name) > DateTime.Now - TimeSpan.FromDays(1))
                    {
                        m_component_texts[key] = File.ReadAllText(cache_name);
                    }
                    else
                    {
                        var http = client.GetStringAsync(req_addr);
                        http.Wait(5000);
                        if (!http.IsCompleted) throw new Exception("Failed HTTP thingy");
                        m_component_texts[key] = http.Result;
                        File.WriteAllText(cache_name, http.Result);
                    }
                }
                else
                {
                    throw new NotImplementedException("Unsupported library type");
                }
            }

            if (string.IsNullOrEmpty(m_component_texts[key]))
            {
                throw new Exception("Empty component");
            }

            Component comp = new Component(key, fp_ref);
            comp.Location = location;
            comp.Front = front;
            comp.Angle = angle;
            Components_[fp_ref] = comp;

            return comp;
        }

        /// <summary>
        /// Get KiCad text for all components
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in Components_.Values)
            {
                sb.AppendLine(v.ToString());
            }
            return sb.ToString();
        }
    }
}
