using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    public class OverUseDetectorOptions
    {
      public  OverUseDetectorOptions()  {
            initial_e[0,0] = 100;
            initial_e[1,1] = 1e-1;
            initial_e[0,1]  = 0;
            initial_e[1, 0] = 0;
            initial_process_noise[0] = 1e-13;
            initial_process_noise[1] = 1e-3;
        }
        public double initial_slope { get; set; } = (double)8.0 / (double)512.0;
        public double initial_offset { get; set; } = 0;
        public double[,]initial_e { get; set; } = new double[2,2];
        public double[] initial_process_noise { get; set; } = new List<double>() { 1e-13, 1e-3 }.ToArray();
        public double initial_avg_noise { get; set; }=0;
        public double initial_var_noise { get; set; } = 50;
    }
}
