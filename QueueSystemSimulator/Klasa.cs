using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class Klasa : IComparable
    {
        public enum Typ {lisc = 0, srodkowa = 1, root = 2 };
        public enum Kolory { zielony = 0, zolty = 1, czerwony = 2 };
        int gwarantowanaSzybkoscBitowa, szczytowaSzybkoscBitowa, aktualnaSzybkoscBitowa;
        int poziom, priorytet, kwant;
        Klasa rodzic;
        Klasa[] dzieci;
        Kolejka kolejka;
        Typ typ;
        TB tb;
        TB maxTb;
        Kolory kolor;


        public Klasa(int gwarantowanaSzybkoscBitowa, int szczytowaSzybkoscBitowa, int poziom, int priorytet, int kwant, Klasa rodzic, Kolejka kolejka, Typ typ, double czas)
        {
            this.gwarantowanaSzybkoscBitowa = gwarantowanaSzybkoscBitowa;
            this.szczytowaSzybkoscBitowa = szczytowaSzybkoscBitowa;
            this.poziom = poziom;
            this.priorytet = priorytet;
            this.kwant = kwant;
            this.rodzic = rodzic;
            this.kolejka = kolejka;
            this.typ = typ;
            tb = new TB(gwarantowanaSzybkoscBitowa, 16000, czas);
            maxTb = new TB(szczytowaSzybkoscBitowa, 16000, czas);
            kolor = Kolory.zielony;
        }

        public Klasa(int gwarantowanaSzybkoscBitowa, int szczytowaSzybkoscBitowa, int poziom, Klasa rodzic, Typ typ, double czas)
        {
            this.gwarantowanaSzybkoscBitowa = gwarantowanaSzybkoscBitowa;
            this.szczytowaSzybkoscBitowa = szczytowaSzybkoscBitowa;
            this.poziom = poziom;
            this.rodzic = rodzic;
            this.typ = typ;
            tb = new TB(gwarantowanaSzybkoscBitowa, 16000, czas);
            maxTb = new TB(szczytowaSzybkoscBitowa, 16000, czas);
            kolor = Kolory.zielony;
        }

        public Klasa(int gwarantowanaSzybkoscBitowa, int szczytowaSzybkoscBitowa, int poziom, Typ typ, double czas)
        {
            this.gwarantowanaSzybkoscBitowa = gwarantowanaSzybkoscBitowa;
            this.szczytowaSzybkoscBitowa = szczytowaSzybkoscBitowa;
            this.poziom = poziom;
            this.typ = typ;
            tb = new TB(gwarantowanaSzybkoscBitowa, 16000, czas);
            maxTb = new TB(szczytowaSzybkoscBitowa, 16000, czas);
            kolor = Kolory.zielony;
        }

        public Klasa[] Dzieci
        {
            get
            {
                return dzieci;
            }
            set
            {
                dzieci = value;
            }
        }

        public int Poziom
        {
            get
            {
                return poziom;
            }
        }

        public Typ TypKlasy
        {
            get
            {
                return typ;
            }
        }

        public int Priorytet
        {
            get
            {
                return priorytet;
            }
        }

        public int CompareTo(object obj)
        {
            Klasa doPorownania = obj as Klasa;
            if (doPorownania.Priorytet < priorytet)
            {
                return 1;
            }
            if (doPorownania.Priorytet > priorytet)
            {
                return -1;
            }

            // The orders are equivalent.
            return 0;
        }
        public Kolejka Kolejka
        {
            get
            {
                return kolejka;
            }
        }

        public Kolory Kolor
        {
            get
            {
                return kolor;
            }
        }

        public int Kwant
        {
            get
            {
                return kwant;
            }
        }

        public TB Tb
        {
            get
            {
                return tb;
            }
        }


        public Kolory aktualizujKolor(double aktualnyCzas, double dlugoscPierwszegoPakietu)
        {
            double stan = tb.aktualizujStan(aktualnyCzas);
            double maxTbStan = maxTb.aktualizujStan(aktualnyCzas);
            //dzieki temu ze zetony pobierane sa za kazda obsluga pakietu przez wszystkich przodkow to mozemy rownie okreslic kolor nielisci bez aktualizowania ich potomkow
            if (stan > dlugoscPierwszegoPakietu)
            {
                kolor = Kolory.zielony;
            } else if (maxTbStan < dlugoscPierwszegoPakietu)//byc moze to jeszce tzeba podzizelic przez kwant czasu
            {
                kolor = Kolory.czerwony;
            } else
            {
                kolor = Kolory.zolty;
            }
            if (rodzic != null)
            {
                rodzic.aktualizujKolor(aktualnyCzas, dlugoscPierwszegoPakietu);
            }
            
            return kolor;
        }

        public bool obsluzPakiet(double aktualnyCzas, double dlugoscPakietu)
        {
            switch (kolor)
            {
                case Klasa.Kolory.zielony:
                    if (typ == Typ.root)
                    {
                        tb.pobierzZetony(dlugoscPakietu);
                        maxTb.pobierzZetony(dlugoscPakietu);
                        return true;
                    }else if (rodzic.obsluzPakiet(aktualnyCzas, dlugoscPakietu))
                    {
                        tb.pobierzZetony(dlugoscPakietu);
                        maxTb.pobierzZetony(dlugoscPakietu);
                        return true;
                    }
                    return false;

                case Klasa.Kolory.zolty:
                    if (typ == Typ.root)
                    {
                        tb.pobierzZetony(dlugoscPakietu);
                        maxTb.pobierzZetony(dlugoscPakietu);
                        return true;
                    } else if(rodzic.obsluzPakiet(aktualnyCzas, dlugoscPakietu))
                    {
                        tb.pobierzZetony(dlugoscPakietu);
                        maxTb.pobierzZetony(dlugoscPakietu);
                        return true;
                    }
                    return false;

                case Klasa.Kolory.czerwony:
                    return false;
                    
            }
            return false;
        }
    }
}
