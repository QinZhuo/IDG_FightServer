using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
namespace RatioTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int length = 1000;
            int n = 100000;
            
            int[] u=new int[length];
            int[] d=new int[length];
            for (int i = 0; i < length; i++)
            {
                u[i] = i;
                d[i] = length-i;
            }

            //RatioInit(u, d,1);
            //DoubleInit(u, d,1);
            //IntInit(u, d, 1);
           

            Add(DoubleInit(u, d, 1), DoubleInit(u, d, 1), n);
            Add(RatioInit(u, d, 1), RatioInit(u, d, 1), n);

            //Console.WriteLine(sizeof(double));
            Console.ReadLine();
        }
        public static bool[] Add(Ratio[] a, Ratio[] b,int n)
        {
            bool[] value = new bool[a.Length];
            var start = DateTime.Now;
            var memery = GetProcessUsedMemory();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    value[j] = a[j] > b[j];
                }
            }
            Console.WriteLine("Ratio 用时：" + (DateTime.Now - start) + "内存使用：" + (GetProcessUsedMemory() - memery));
            return value;
        }
        public static Ratio[] RatioInit(int[] u,int[] d,int n=1)
        {
            Ratio[] value = new Ratio[u.Length];
            var start = DateTime.Now;
            var memery = GetProcessUsedMemory();
            for (int z = 0; z < n; z++)
            {


                for (int i = 0; i < u.Length; i++)
                {
                    value[i] = new Ratio(u[i], d[i]);
                }

            }
            Console.WriteLine("Ratio 用时：" + (DateTime.Now - start) + "内存使用：" + (GetProcessUsedMemory() - memery));
            return value;
        }
        public static double[] DoubleInit(int[] u, int[] d, int n = 1)
        {
            double[] value = new double[u.Length];
            var start = DateTime.Now;
            var memery = GetProcessUsedMemory();
            

            for (int z = 0; z < n; z++)
            {
                for (int i = 0; i < u.Length; i++)
                {
                    value[i] = u[i] * 1.0 / d[i];
                }
            }
            Console.WriteLine("double 用时：" + (DateTime.Now - start) + "内存使用：" + (GetProcessUsedMemory() - memery));
            return value;
        }

        public static bool[] Add(double[] a, double[] b, int n)
        {
            bool[] value = new bool[a.Length];
            var start = DateTime.Now;
            var memery = GetProcessUsedMemory();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    value[j] = a[j] > b[j];
                }
            }
            Console.WriteLine("double 用时：" + (DateTime.Now - start) + "内存使用：" + (GetProcessUsedMemory() - memery));
            return value;
        }
        public static int[] IntInit(int[] u, int[] d, int n = 1)
        {
            int[] value = new int[u.Length];
            var start = DateTime.Now;
            var memery = GetProcessUsedMemory();


            for (int z = 0; z < n; z++)
            {
                for (int i = 0; i < u.Length; i++)
                {
                    value[i] = u[i];
                }
            }
            Console.WriteLine("int 用时：" + (DateTime.Now - start) + "内存使用：" + (GetProcessUsedMemory() - memery));
            return value;
        }

        public static double GetProcessUsedMemory()

        {

            double usedMemory = 0;

            usedMemory = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;

            return usedMemory;

        }
        }

    public struct Ratio
    {
        
        private static readonly int precision = 100;
        private int _ratio;
        public Ratio(int up, int down)
        {
            _ratio = (up * precision) / down;
           
        }
        private Ratio(int precisionRatio)
        {
            _ratio = precisionRatio;

        }
        public override string ToString()
        {
            return (_ratio*1.0f/precision).ToString();
        }
       
        public static Ratio operator +(Ratio a, Ratio b)
        {
            return new Ratio(a._ratio+b._ratio);
        }
        public static Ratio operator -(Ratio a, Ratio b)
        {
            return new Ratio(a._ratio - b._ratio);
        }
        public static Ratio operator *(Ratio a, Ratio b)
        {
            return new Ratio(a._ratio * b._ratio/precision);
        }

        public static Ratio operator /(Ratio a, Ratio b)
        {
            return new Ratio(a._ratio * precision / b._ratio);
        }
        public static bool operator >(Ratio a, Ratio b)
        {
            return a._ratio > b._ratio;
        }
        public static bool operator <(Ratio a, Ratio b)
        {
            return a._ratio < b._ratio;
        }
        public static bool operator ==(Ratio a, Ratio b)
        {
            return a._ratio == b._ratio;
        }

        public static bool operator !=(Ratio a, Ratio b)
        {
            return a._ratio != b._ratio;
        }

        public float ToFloat()
        {
            return _ratio*1.0f/precision;
        }

    }
}
