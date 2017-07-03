using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class Kolejka
    {
        int dlugoscBufora;
        Queue<Pakiet> bufor;
        String nazwa;

        public Kolejka(String nazwa, int dlugoscBufora)
        {
            this.nazwa = nazwa;
            this.dlugoscBufora = dlugoscBufora;
            bufor = new Queue<Pakiet>(dlugoscBufora);            
        }

        public string Nazwa
        {
            get
            {
                return nazwa;
            }
        }

        public int DlugoscBufora
        {
            get
            {
                return dlugoscBufora;
            }
        }

        public void dodajDoKolejki(Pakiet pakiet)
        {
            if (bufor.Count == dlugoscBufora)
            {
                return;
            }
            bufor.Enqueue(pakiet);
        }

        public Pakiet zdejmijZKolejki()
        {
            if (bufor.Count > 0)
            {
                return bufor.Dequeue();
            }
            return null;
        }

        public double DlugoscPierwszegoPakietu()
        {
            if (bufor.Count > 0)
            {
                return bufor.ElementAt(0).Dlugosc;
            }
            return 0;
        }

        public bool CzyPelna()
        {
            if(bufor.Count >= dlugoscBufora)
            {
                return true;
            }
            return false;
        }

        public int LiczbaPakietowWKolejce
        {
            get
            {
                return bufor.Count;
            }
        }
        public bool CzyPusta()
        {
            if (bufor.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
