using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class HTB
    {
        Klasa korzen;
        Klasa[] liscie;
        int liczbaPoziomow;
        int liczbaPriorytetow;
        DRR drr;

        public HTB (Klasa korzen, int liczbaPoziomow, int liczbaPriorytetow)
        {
            this.korzen = korzen;
            this.liczbaPoziomow = liczbaPoziomow;
            this.liczbaPriorytetow = liczbaPriorytetow;
            drr = new DRR();
            List<Klasa> listaLisci = new List<Klasa>();
            foreach (Klasa dziecko in korzen.Dzieci)
            {
                wyszukajLiscie(dziecko, listaLisci);
            }
            listaLisci.Sort();
            liscie = listaLisci.ToArray();
            korzen.Tb.Pojemnosc = 12000 * liscie.Length * 2;


        }
        private void wyszukajLiscie(Klasa klasa, List<Klasa> listaLisci)
        {
            if (klasa.TypKlasy == Klasa.Typ.lisc)
            {
                listaLisci.Add(klasa);
                return;
            }

            Klasa[] dzieci = klasa.Dzieci;
            if(dzieci == null)
            {
                return;
            }
            foreach(Klasa dziecko in dzieci)
            {
                wyszukajLiscie(dziecko, listaLisci);
            }
        }

        public string wybierzKolejkeDoObslugi(double aktualnyCzas, List<Klasa> wyjatki)
        {
            List<Klasa> rywalizujaceLiscie = new List<Klasa>();
            int aktualnieNajwyzszyPriorytet = liczbaPriorytetow-1;
            int aktualnyKolor = 2;
            foreach (Klasa lisc in liscie)
            {
                if (wyjatki.Contains(lisc))
                {
                    continue;
                }
                if (!lisc.Kolejka.CzyPusta())
                {
                    lisc.aktualizujKolor(aktualnyCzas, lisc.Kolejka.DlugoscPierwszegoPakietu());

                    if ((int)lisc.Kolor <= aktualnyKolor)
                    {
                        if ((int)lisc.Kolor == aktualnyKolor)
                        {
                            if (lisc.Priorytet < aktualnieNajwyzszyPriorytet)
                            {
                                rywalizujaceLiscie.Clear();
                                rywalizujaceLiscie.Add(lisc);
                                aktualnieNajwyzszyPriorytet = lisc.Priorytet;
                                aktualnyKolor = (int)lisc.Kolor;
                            }else if (lisc.Priorytet == aktualnieNajwyzszyPriorytet)
                            {
                                rywalizujaceLiscie.Add(lisc);
                                aktualnieNajwyzszyPriorytet = lisc.Priorytet;
                                aktualnyKolor = (int)lisc.Kolor;
                            }

                        } else
                        {
                            rywalizujaceLiscie.Clear();
                            rywalizujaceLiscie.Add(lisc);
                            aktualnieNajwyzszyPriorytet = lisc.Priorytet;
                            aktualnyKolor = (int)lisc.Kolor;
                        }
                        
                    } 
                }
            }
            if(aktualnyKolor == 2)
            {
                return null;
            }
            switch (rywalizujaceLiscie.Count)
            {
                case 0:
                    return null;
                case 1: //wysylamy z tej kolejki o ile nie jest czerwona
                    //sprawdzam kolor
                    Klasa jedyny = rywalizujaceLiscie.ElementAt(0);
                    if (jedyny.obsluzPakiet(aktualnyCzas, jedyny.Kolejka.DlugoscPierwszegoPakietu()))
                    {
                        return jedyny.Kolejka.Nazwa;
                    }
                    else
                    {
                        wyjatki.Add(jedyny);
                        return wybierzKolejkeDoObslugi(aktualnyCzas, wyjatki);
                    }

                default:
                    if (rywalizujaceLiscie.Count < 0)
                    {
                        return null;
                    }

                    //DRR
                    //zmienic! tutaj dre nie bedzie sie zmienial, ale moze byc ich kilka
                    string nazwaKolejki;
                    if (drr.CzyDziala)
                    {
                        drr.sprawdzISkorygujZgodnoscWejsc(rywalizujaceLiscie.ToArray());
                        nazwaKolejki = drr.WskazKolejkeDoObslugi();
                    } else
                    {
                        drr = new DRR(rywalizujaceLiscie.ToArray(), aktualnyCzas);
                        nazwaKolejki = drr.WskazKolejkeDoObslugi();
                    }
                    return nazwaKolejki;
            }
           
        }
      
        
    }
}
