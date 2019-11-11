using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    public class TesData
    {
        public long t_d;
        public double ts_d;
        public int size_d;
    }
    class Program
    {
        static void Main(string[] args)
        {
            OverUseDetectorOptions overUseDetectorOptions = new OverUseDetectorOptions();
            OveruseEstimator overuseEstimator = new OveruseEstimator(overUseDetectorOptions);

            var list = new List<TesData>();

            for (int i = 0; i < 100; i++)
            {
                TesData tesData = new TesData() { size_d=new Random().Next(0,10000), ts_d= new Random().Next(0, 100),t_d= new Random().Next(10, 200) };
                list.Add(tesData);
                
            }

            overuseEstimator.Update(20, 10, 1084, BandwidthUsage.kBwNormal);

            Console.ReadLine();
        }
    }
}
