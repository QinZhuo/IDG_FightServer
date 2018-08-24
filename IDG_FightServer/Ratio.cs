using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDG
{
    public class Ratio
    {
        private int _u;//分子
        public int u
        {
            get { return _u; }
            private set { _u = value; }
        }
        private int _d = 1;//分母 
        public int d
        {
            get { return _d; }
            private set { if (value == 0) { throw new InvalidOperationException("分母不能为零"); }
                else if(value>0){  _d = value; }
                else { _d = -value;u=-u; } }
        }

        public Ratio(int up,int down=1)
        {
            u = up;
            d = down;
            Reduction();
        }
        public override string ToString()
        {
            return u + "/" + d;
        }
        private void Reduction()//约分
        {
            int gcd = GCD(u, d);
            u /= gcd;
            d /= gcd;
        }
        private int GCD(int a,int b)
        {
            int temp = 1;
            while (b!=0)
            {
                temp = a % b;
                a = b;
                b = temp;
            }
            return a;
        }
        public Ratio Add(Ratio fra)
        {
            u = u * fra.d + fra.u * d;
            d = d * fra.d;
            Reduction();
            return this;
        }
        public Ratio Sub(Ratio fra)
        {
            u = u * fra.d - fra.u * d;
            d = d * fra.d;
            Reduction();
            return this;
        }
        public Ratio Mult(Ratio fra)
        {
            
            u *= fra.u;
            d *= fra.d;
            Reduction();
            return this;
        }
        
        public Ratio Division(Ratio fra)
        {
            return Mult(fra.Reciprocal());
        }
        private Ratio Reciprocal()
        {
            int t = u;
            u = d;
            d = t;
            return this;
        }
    }
}
