using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class DRR
    {
        List<Klasa> wejscia;
        List<DaneWejscia> daneWejsc;
        int ostatniIndex;
        bool czyDziala;

        public DRR()
        {
            wejscia = new List<Klasa>();
            daneWejsc = new List<DaneWejscia>();
            ostatniIndex = -1;
            czyDziala = false;
        }

        public DRR(Klasa[] wejscia, double aktualnyCzas)
        {
            this.wejscia = new List<Klasa>();
            daneWejsc = new List<DaneWejscia>();
            foreach (Klasa wejscie in wejscia)
            {
                this.wejscia.Add(wejscie);
                daneWejsc.Add(new DaneWejscia(wejscie, aktualnyCzas));
            }
            ostatniIndex = -1;
            czyDziala = true;
        }

        public void dodajWejscie(Klasa wejscie, double aktualnyCzas)
        {
            wejscia.Add(wejscie);
            daneWejsc.Add(new DaneWejscia(wejscie, aktualnyCzas));
        }

        public void usunWejscie(Klasa wejscie)
        {
            wejscia.Remove(wejscia.Find(x => x.Kolejka == wejscie.Kolejka));
            daneWejsc.Remove(daneWejsc.Find(x => x.Wejscie.Kolejka == wejscie.Kolejka));
        }

        public void sprawdzISkorygujZgodnoscWejsc(Klasa[] wejscia)
        {

            foreach (Klasa wejscie in wejscia)
            {
                if (!this.wejscia.Contains(wejscie))
                {
                    this.wejscia.Add(wejscie);

                }
            }
            if (wejscia.Length < this.wejscia.Count)
            {
                foreach (Klasa wejscie in this.wejscia)
                {
                    if (!wejscia.Contains(wejscie))
                    {
                        this.wejscia.Remove(wejscie);

                    }
                }
            }

        }

        public string WskazKolejkeDoObslugi()
        {
            if (wejscia.Count < 1)
            {
                return null;
            }
            while (true)
            {
                if (ostatniIndex >= wejscia.Count - 1)
                {
                    ostatniIndex = 0;
                    foreach (DaneWejscia dane in daneWejsc)
                    {
                        dane.LiczbaUzbieranychJednostek += dane.Wejscie.Kwant;
                    }
                }
                else
                {
                    ostatniIndex++;
                }
                DaneWejscia daneWejscia = daneWejsc.ElementAt(ostatniIndex);

                if (daneWejscia.LiczbaUzbieranychJednostek > daneWejscia.Wejscie.Kolejka.DlugoscPierwszegoPakietu())
                {
                    daneWejscia.LiczbaUzbieranychJednostek -= daneWejscia.Wejscie.Kolejka.DlugoscPierwszegoPakietu();
                    return daneWejscia.Wejscie.Kolejka.Nazwa;
                }
            }
        }

        public void Czysc()
        {
            wejscia = new List<Klasa>();
            daneWejsc = new List<DaneWejscia>();
            czyDziala = false;
        }

        public bool CzyDziala
        {
            get
            {
                return czyDziala;
            }
        }



        class DaneWejscia
        {
            Klasa wejscie;
            double liczbaUzbieranychJednostek;

            public DaneWejscia(Klasa wejscie, double czas)
            {
                this.wejscie = wejscie;
                liczbaUzbieranychJednostek = wejscie.Kwant;
            }


            public double LiczbaUzbieranychJednostek
            {
                get
                {
                    return liczbaUzbieranychJednostek;
                }
                set
                {
                    liczbaUzbieranychJednostek = value;
                }
            }

            public Klasa Wejscie
            {
                get
                {
                    return wejscie;
                }
                set
                {
                    wejscie = value;
                }
            }
        }
    }
}
