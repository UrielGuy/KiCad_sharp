using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    /// <summary>
    /// Represntation of a KiCadNet
    /// </summary>
    public class Net
    {
        public Net(int num, string name)
        {
            Number = num;
            Name = name;
        }
        public int Number { get; }
        public string Name { get; }

    }

    /// <summary>
    /// All nets in a PCB
    /// </summary>
    public class Nets
    {
        public Net AddNet(string name)
        {
            var keys = ByNum.Keys;
            int num = keys.Count == 0 ? 1 : (keys.Max() + 1);
            return AddNet(num, name);
        }

        public Net AddNet(int number, string name)
        {
            var res = new Net(number, name);
            ByName[res.Name] = res;
            ByNum[res.Number] = res;
            return res;
        }
        public readonly Dictionary<string, Net> ByName = new Dictionary<string, Net>();
        public readonly Dictionary<int, Net> ByNum = new Dictionary<int, Net>();
    }
}