using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class TB
    {
        int szybkoscNaplywu;
        double pojemnosc;
        double czasOstatniejAktualizacji;
        double stan;

        public TB (int szybkosc, int pojemnosc, double czas)
        {
            szybkoscNaplywu = szybkosc;
            this.pojemnosc = pojemnosc;
            czasOstatniejAktualizacji = czas;
            stan = pojemnosc;
        }
        
        public double aktualizujStan(double aktualnyCzas)
        {
            if (stan != pojemnosc)
            {
                stan += szybkoscNaplywu * (aktualnyCzas - czasOstatniejAktualizacji);
                czasOstatniejAktualizacji = aktualnyCzas;
                if ( stan > pojemnosc)
                {
                    stan = pojemnosc;
                }
            }
            return stan;
        }

        public double Pojemnosc
        {
            set
            {
                pojemnosc = value;
            }

        }
        //pozyczajac uzyskujemy ujemny stan

        public bool pobierzZetony (double liczbaZetonow)
        {

            //if (liczbaZetonow <= stan)
            //{
                stan -= liczbaZetonow;
                return true;
           // }
           // return false;
        }
    }
}
