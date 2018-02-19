using System.Collections.Generic;
using System.Threading.Tasks;
using PeanutButter.ServiceShell;

namespace VHDMounter
{
    public class Program
    {
        static void Main(string[] args)
        {
            Shell.RunMain<VhdMounterShell>(args);
        }
    }
}