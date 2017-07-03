using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QueueSystemSimulator
{
    class SystemKolejkowy
    {
        SortedList<double, Zdarzenie> listaZdarzen;

        // prametry
        private int przeplywnosc, liczbaKolejek, liczbaStrumieni, liczbaRozkladow;
        //elementy
        private Kolejka[] kolejki;
        Dictionary<string, int> mapowanieNazwyKolejkiNaJejNumer = new Dictionary<string, int>();
        Dictionary<string, Rozklad> rozklady = new Dictionary<string, Rozklad>();
        private Strumien[] strumienie;
        HTB htb;

        //zmienne pomocnicze
        Random rnd = new Random(DateTime.Now.Millisecond);
        private double aktualnyCzas;


        //statystyki
        //zajetosc lacza wyjsciowego
        private double czasZajetosciLacza = 0;
        private double liczbaBitowWyslanych = 0;
        private double[] liczbaBitowWyslanychZKolejki;

        //srednia zajetosc(liczba pakietow w kolejce) poszczegolnych kolejek, indeks=numer kolejki
        private double[] zajetoscKolejek;

        //czas ostatniej operacji na kolejce
        private double[] czasOstatniejOperacjiNaKolejce;

        //liczba pakietow z poszczeglonych strumieni: indeks=numer stumienia
        private int[] pakietyOdrzucone;
        private int[] pakietyPrzybyle;

        //sredni czas obslugi pakietu pochodzacego z danego strumienia
        private double[] skumulowanyCzasObslugi;
        private int[] liczbaPakietowObsluzonych;

        
        //plik wynikowy
        string plikWynikowy;

        public SystemKolejkowy()
        {
            Console.WriteLine("Podaj nazwe pliku wejsciowego: ");
            String plikKonfiguracyjny = Console.ReadLine();
            Console.WriteLine("Podaj nazwe pliku wyjsciowego: ");
            plikWynikowy = Console.ReadLine();
            WczytajKonfiguracje(plikKonfiguracyjny);
            // lista uporzadkowana przechwywujaca zdarzenia 
            listaZdarzen = new SortedList<double, Zdarzenie>(liczbaStrumieni + 1);
        }


        private void dodajZdarzeniePrzybyciaPakietu(int numerStrumienia)
        {

            Strumien strumien = strumienie[numerStrumienia];
            
            //ograniczenie czasu od 0 do 100/lambda 
            double czas = (strumien.RozkladCzasu).WygenerujWartosc(rnd.NextDouble());
           /* if (czas > 100 / (strumien.Rozklad_czasu).Lambda)
                czas = 100 / (strumien.Rozklad_czasu).Lambda;*/

            Zdarzenie noweZdarzenie = new Zdarzenie(Zdarzenie.TypZdarzenia.przybycie_pakietu, czas + aktualnyCzas, numerStrumienia);
            //wstawienie do listy zdarzen 
            listaZdarzen.Add(noweZdarzenie.Czas, noweZdarzenie);
        }

        private void dodajZdarzenieKolejnejObslugi(double dlugoscPakietu)
        {
            //obliczanie wg. rozkladu wyjscia i parametrow wyjscia kiedy ma nastapic event wyjscia pakietu
            double czasTransmisji = dlugoscPakietu / (double)przeplywnosc;

            liczbaBitowWyslanych += dlugoscPakietu;
            if (liczbaBitowWyslanych > double.MaxValue)
            {
                czasZajetosciLacza += liczbaBitowWyslanych / (double) przeplywnosc;
                liczbaBitowWyslanych = 0;
            }
            //dodanie eventu wyjscia pakietu
            Zdarzenie noweZdarzenie = new Zdarzenie(Zdarzenie.TypZdarzenia.obsluga_pakietu, aktualnyCzas + czasTransmisji, -1);
            //wstawienie do listy zdarzen 
            listaZdarzen.Add(noweZdarzenie.Czas, noweZdarzenie);
        }

        private Zdarzenie ZdejmijKolejneZdarzenie()
        {
            // pobieramy zdarzenie ktore najwczesniej nastapi
            Zdarzenie tymczasowe = listaZdarzen.ElementAt(0).Value;
            listaZdarzen.RemoveAt(0);
            // ustawiamy aktualny czas na czas tego zdarzenia
            aktualnyCzas = tymczasowe.Czas;

            // pobralismy zdarzenie przyjscia pakietu
            if (tymczasowe.Typ == Zdarzenie.TypZdarzenia.przybycie_pakietu)
            {
                //dodanie eventu na ten sam strumien
                dodajZdarzeniePrzybyciaPakietu(tymczasowe.NumerStrumienia);

                //notujemy przyjscie pakietu
                pakietyPrzybyle[tymczasowe.NumerStrumienia]++;

                //dodanie do kolejki pakietu z pobranego eventu
                int numerKolejki = strumienie[tymczasowe.NumerStrumienia].NumerKolejki;

                //kolejka jest pelna
                if (kolejki[numerKolejki].CzyPelna())
                {
                    pakietyOdrzucone[tymczasowe.NumerStrumienia]++;
                }
                //kolejka nie jest pelna
                else
                {
                    zajetoscKolejek[numerKolejki] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[numerKolejki]) * kolejki[numerKolejki].LiczbaPakietowWKolejce;
                    czasOstatniejOperacjiNaKolejce[numerKolejki] = aktualnyCzas;
                    double dlugoscPakietu;
                    if (strumienie[tymczasowe.NumerStrumienia].RozkladDlugosci != null)
                    {
                        dlugoscPakietu = strumienie[tymczasowe.NumerStrumienia].RozkladDlugosci.WygenerujWartosc(rnd.NextDouble());
                    }
                    else
                    {
                        dlugoscPakietu = strumienie[tymczasowe.NumerStrumienia].StalaDlugoscPakietow;
                    }
                    Pakiet pakiet = new Pakiet(dlugoscPakietu, tymczasowe.NumerStrumienia, aktualnyCzas);
                    kolejki[numerKolejki].dodajDoKolejki(pakiet);
                }
            }
            // pobralismy zdarzenie wyjscia pakietu
            else
            {
                ObsluzZdarzenieObslugi();
            }

            return tymczasowe;
        }

        public void ObsluzZdarzenieObslugi()
        {
            bool zdjecieZKolejki = false;
           /* //wysylamy pakiet (usuwamy pakiet z listy o najwiekszym priorytecie)
            for (int priorytet = 0; priorytet < liczbaKolejek; priorytet++)
            {
                if (kolejki[priorytet].CzyPusta() == false)
                {
                    // znaleziono niepusta kolejke o priorytecie priorytet
                    zdjecieZKolejki = true;
                    //statystyka zajetosci kolejki
                    zajetoscKolejek[priorytet] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[priorytet]) * kolejki[priorytet].LiczbaPakietowWKolejce;
                    czasOstatniejOperacjiNaKolejce[priorytet] = aktualnyCzas;

                    //TODO: nie kazda kolejka ma inny priorytet!!!!!!!!!!!!!!!!!!!!!!!
                    Pakiet pakiet = kolejki[priorytet].zdejmijZKolejki();

                    //czas obslugi zdjetego pakietu
                    skumulowanyCzasObslugi[pakiet.NumerStrumienia] += aktualnyCzas - pakiet.Czas;
                    liczbaPakietowObsluzonych[pakiet.NumerStrumienia]++;

                    //liczba bitow wyslanych
                    liczbaBitowWyslanychZKolejki[priorytet] += pakiet.Dlugosc;

                    dodajZdarzenieKolejnejObslugi(pakiet.Dlugosc);
                }
                if (zdjecieZKolejki == true)
                    break;
            } /*/ //PQ
            string nazwaKolejkiDoObslugi = htb.wybierzKolejkeDoObslugi(aktualnyCzas, new List<Klasa>());
            if (nazwaKolejkiDoObslugi != null)
            {
                zdjecieZKolejki = true;
                int numerKolejki;
                if (mapowanieNazwyKolejkiNaJejNumer.TryGetValue(nazwaKolejkiDoObslugi, out numerKolejki))
                {
                    //statystyka zajetosci kolejki
                    zajetoscKolejek[numerKolejki] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[numerKolejki]) * kolejki[numerKolejki].LiczbaPakietowWKolejce;
                    czasOstatniejOperacjiNaKolejce[numerKolejki] = aktualnyCzas;

                    Pakiet pakiet = kolejki[numerKolejki].zdejmijZKolejki();

                    //liczba bitow wyslanych
                    liczbaBitowWyslanychZKolejki[numerKolejki] += pakiet.Dlugosc;

                    

                    //czas obslugi zdjetego pakietu
                    skumulowanyCzasObslugi[pakiet.NumerStrumienia] += aktualnyCzas - pakiet.Czas;
                    liczbaPakietowObsluzonych[pakiet.NumerStrumienia]++;


                    dodajZdarzenieKolejnejObslugi(pakiet.Dlugosc);
                }

            }

            if (zdjecieZKolejki == false)
            {

                //pobieramy kolejne zdarzenie - nic nie wydarzy sie az nie przyjdzie kolejny pakiet, to zdarzenie napewno bedzie wejsciowym
                Zdarzenie chwilowe = listaZdarzen.ElementAt(0).Value;
                listaZdarzen.RemoveAt(0);
                aktualnyCzas = chwilowe.Czas;

                //dodanie eventu na ten sam strumien
                dodajZdarzeniePrzybyciaPakietu(chwilowe.NumerStrumienia);
                //notujemy przyjscie pakietu
                pakietyPrzybyle[chwilowe.NumerStrumienia]++;

                //dodanie do kolejki pakietu z pobranego eventu
                int numerKolejki = strumienie[chwilowe.NumerStrumienia].NumerKolejki;

                zajetoscKolejek[numerKolejki] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[numerKolejki]) * kolejki[numerKolejki].LiczbaPakietowWKolejce;
                czasOstatniejOperacjiNaKolejce[numerKolejki] = aktualnyCzas;
                double dlugoscPakietu;
                if(strumienie[chwilowe.NumerStrumienia].RozkladDlugosci != null)
                {
                    dlugoscPakietu = strumienie[chwilowe.NumerStrumienia].RozkladDlugosci.WygenerujWartosc(rnd.NextDouble());
                }
                else
                {
                    dlugoscPakietu = strumienie[chwilowe.NumerStrumienia].StalaDlugoscPakietow;
                }
                Pakiet pakiet = new Pakiet(dlugoscPakietu, chwilowe.NumerStrumienia, aktualnyCzas);
                kolejki[numerKolejki].dodajDoKolejki(pakiet);



                ObsluzZdarzenieObslugi();
            }
        }

        public void start()
        {
            int czasSymulacji = 100000;

            // ustawiam aktualny czas na 0 - poczatek
            aktualnyCzas = 0;

            //dodanie po 1 zdarzeniu wejscia pakietu z kazdego strumienia
            for (int nrStrumienia = 0; nrStrumienia < liczbaStrumieni; nrStrumienia++)
            {
                dodajZdarzeniePrzybyciaPakietu(nrStrumienia);
            }

           
            //dodanie 1 eventu wyjsciowego
            dodajZdarzenieKolejnejObslugi(0);
            //obsluga docelowego ruchu - w tym momencie mamy 4 eventy - 3x wejscia i 1x wyjscia w lista_zdarzen
            int a = 0;
            while (aktualnyCzas < czasSymulacji)
            {
                ZdejmijKolejneZdarzenie();
                if (aktualnyCzas > 95)
                    a = 1;
                
            }

            for (int i = 1; i <= liczbaKolejek; i++)
            {
                zajetoscKolejek[i - 1] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[i - 1]) * kolejki[i - 1].LiczbaPakietowWKolejce;
                Console.WriteLine("srednia zajetosc kolejki  nr " + i + "=" + zajetoscKolejek[i - 1] / czasSymulacji);
            }
            czasZajetosciLacza += liczbaBitowWyslanych / przeplywnosc;
            Console.WriteLine("srednia zajetosc lacza " + czasZajetosciLacza / aktualnyCzas * 100 + "%");
            for (int i = 0; i < liczbaStrumieni; i++)
            {
                if (pakietyPrzybyle[i] == 0)
                    Console.WriteLine("prawdopodobienstwo odrzucenia pakietu pochadzacego ze strumienia " + i + "=0 ");
                else
                    Console.WriteLine("prawdopodobienstwo odrzucenia pakietu pochadzacego ze strumienia " +
                        i + "= " + (float)pakietyOdrzucone[i] / (float)pakietyPrzybyle[i]);
                if (liczbaPakietowObsluzonych[i] == 0)
                    Console.WriteLine("sredni czas obslugi pakietu w strumieniu  nr = 0");
                else
                {
                    Console.WriteLine("sredni czas obslugi pakietu w strumieniu  nr " +
                        i + "= " + skumulowanyCzasObslugi[i] / liczbaPakietowObsluzonych[i]);
                    Console.WriteLine("liczba pakietow obsluzonych w strumieniu  nr " +
                        i + "= " + liczbaPakietowObsluzonych[i]);
                }
                    
            }
            //jh
            zapisz(czasSymulacji);
        }

        private void WczytajKonfiguracje(string sciezka)
        {
            XmlDocument reader = new XmlDocument();
            reader.Load(sciezka);
            XmlNode wezelPrzeplywnosc = reader.DocumentElement?.SelectSingleNode("/system/serwer/przeplywnosc");
            if (wezelPrzeplywnosc != null)
            {
                przeplywnosc = int.Parse(wezelPrzeplywnosc.InnerText);
            }

            //Parsuje kolejki
            XmlNode wezelKolejki = reader.DocumentElement?.SelectSingleNode("/system/kolejki");
            if (wezelKolejki != null)
            {
                //nie robie zabiezpieczenia na wypadek gdy liczba kolejek = 0, 
                //bo lepiej niech w tym miejscu wyrzuci wyjatek niz interpretowac wyniki ze zlej konfiguracja
                liczbaKolejek = int.Parse(wezelKolejki.FirstChild.InnerText);
                kolejki = new Kolejka[liczbaKolejek];
                czasOstatniejOperacjiNaKolejce = new double[liczbaKolejek];
                zajetoscKolejek = new double[liczbaKolejek];
                liczbaBitowWyslanychZKolejki = new double[liczbaKolejek];

                XmlNodeList listaKolejek = wezelKolejki.SelectNodes("/system/kolejki/kolejka");
                for (int numerKolejki =0; numerKolejki < liczbaKolejek; numerKolejki++)
                {
                    string nazwaKolejki = listaKolejek.Item(numerKolejki).FirstChild.InnerText;
                    int dlugoscBufora = int.Parse(listaKolejek.Item(numerKolejki).LastChild.InnerText);
                    kolejki[numerKolejki] = new Kolejka(nazwaKolejki, dlugoscBufora);
                    mapowanieNazwyKolejkiNaJejNumer.Add(nazwaKolejki, numerKolejki);
                }
            }

            //Parsuje rozklady
            XmlNode wezelRozklady = reader.DocumentElement?.SelectSingleNode("/system/rozklady");
            if (wezelRozklady != null)
            {
                liczbaRozkladow = int.Parse(wezelRozklady.FirstChild.InnerText);

                XmlNodeList listaRozkladow = wezelRozklady.SelectNodes("/system/rozklady/rozklad");
                for (int numerRozkladu = 0; numerRozkladu < liczbaRozkladow; numerRozkladu++)
                {
                    string nazwaRozkladu = listaRozkladow.Item(numerRozkladu).FirstChild.InnerText;
                    double lambda = double.Parse(listaRozkladow.Item(numerRozkladu).LastChild.InnerText);
                    rozklady.Add(nazwaRozkladu, new Rozklad(nazwaRozkladu, lambda));
                }
            }

            //Parsuje strumienie
            XmlNode wezelStrumienie = reader.DocumentElement?.SelectSingleNode("/system/strumienie");
            if (wezelStrumienie != null)
            {
                //nie robie zabiezpieczenia na wypadek gdy liczba kolejek = 0, 
                //bo lepiej niech w tym miejscu wyrzuci wyjatek niz interpretowac wyniki ze zlej konfiguracja
                liczbaStrumieni = int.Parse(wezelStrumienie.FirstChild.InnerText);
                strumienie = new Strumien[liczbaStrumieni];
                pakietyOdrzucone = new int[liczbaStrumieni];
                pakietyPrzybyle = new int[liczbaStrumieni];

                skumulowanyCzasObslugi = new double[liczbaStrumieni];
                liczbaPakietowObsluzonych = new int[liczbaStrumieni];


                XmlNodeList listaStrumieni = wezelStrumienie.SelectNodes("/system/strumienie/strumien");
                for (int numerStrumienia = 0; numerStrumienia < liczbaStrumieni; numerStrumienia++)
                {
                    string nazwaStrumienia = listaStrumieni.Item(numerStrumienia).FirstChild.InnerText;
                    string nazwaKolejki = listaStrumieni.Item(numerStrumienia).SelectSingleNode("kolejka").InnerText;
                    string typObslugi = listaStrumieni.Item(numerStrumienia).SelectSingleNode("typObslugi").InnerText;

                    string nazwaRozkladu = listaStrumieni.Item(numerStrumienia).LastChild.InnerText;
                    int numerKolejki;
                    if (mapowanieNazwyKolejkiNaJejNumer.TryGetValue(nazwaKolejki, out numerKolejki)) {
                        Rozklad rozklad;
                        if (rozklady.TryGetValue(nazwaRozkladu, out rozklad) )
                        {
                            if (typObslugi.Equals("Exp"))
                            {
                                string nazwaRozkladuObslugi = listaStrumieni.Item(numerStrumienia).SelectSingleNode("rozkladObslugi").InnerText;
                                Rozklad obsluga;
                                if (rozklady.TryGetValue(nazwaRozkladuObslugi, out obsluga))
                                {
                                    strumienie[numerStrumienia] = new Strumien(nazwaStrumienia, numerKolejki, rozklad, obsluga, 0);
                                }
                                
                            } else
                            {
                                string dlugoscPakietuString = listaStrumieni.Item(numerStrumienia).SelectSingleNode("dlugosPakietu").InnerText;
                                int dlugoscPakietu = int.Parse(dlugoscPakietuString);
                                strumienie[numerStrumienia] = new Strumien(nazwaStrumienia, numerKolejki, rozklad, null, dlugoscPakietu);
                            }


                        }                   
                    }
                    
                }
            }

            XmlNode wezelHTB = reader.DocumentElement?.SelectSingleNode("/system/HTB");
            int liczbaPriorytetow = int.Parse(wezelHTB.SelectSingleNode("liczbaPriorytetow").InnerText);
            
            XmlNode wezelKorzen = wezelHTB.SelectSingleNode("korzen");
            int liczbaDzieci = int.Parse(wezelKorzen.SelectSingleNode("dzieci/liczbaDzieci").InnerText);
            int poziomKorzenia = int.Parse(wezelKorzen.SelectSingleNode("poziom").InnerText);
            Klasa korzen = new Klasa(przeplywnosc, przeplywnosc, poziomKorzenia, Klasa.Typ.root, aktualnyCzas);
            Klasa[] dzieci = new Klasa[liczbaDzieci];
            XmlNodeList listaDzieci = wezelKorzen.SelectNodes("dzieci/dziecko");
            for (int numerDziecka = 0; numerDziecka < liczbaDzieci; numerDziecka++)
            {
                if (listaDzieci.Item(numerDziecka).SelectSingleNode("typ").InnerText.Equals("lisc"))
                {
                    dzieci[numerDziecka] = wczytajLiscia(listaDzieci.Item(numerDziecka), korzen);
                } else
                {
                    dzieci[numerDziecka] = wczytajSrodkowego(listaDzieci.Item(numerDziecka), korzen);

                }
                
            }
            korzen.Dzieci = dzieci;

            htb = new HTB(korzen, korzen.Poziom + 1, liczbaPriorytetow);
        }

        public Klasa wczytajLiscia(XmlNode wezelDziecka, Klasa rodzic)
        {
            Klasa dziecko = null;
            int gwarantowanaSzybkoscBitowa = int.Parse(wezelDziecka.SelectSingleNode("gwarantowanaSzybkoscBitowa").InnerText);
            int szczytowaSzybkoscBitowa = int.Parse(wezelDziecka.SelectSingleNode("szczytowaSzybkoscBitowa").InnerText);
            int priorytet = int.Parse(wezelDziecka.SelectSingleNode("priorytet").InnerText);
            int kwant = int.Parse(wezelDziecka.SelectSingleNode("kwant").InnerText);
            string nazwaKolejki = wezelDziecka.SelectSingleNode("kolejka").InnerText;
            int numerKolejki;
            if (mapowanieNazwyKolejkiNaJejNumer.TryGetValue(nazwaKolejki, out numerKolejki))
            {
                dziecko = new Klasa(gwarantowanaSzybkoscBitowa, szczytowaSzybkoscBitowa,
                rodzic.Poziom - 1, priorytet, kwant, rodzic, kolejki[numerKolejki], Klasa.Typ.lisc, aktualnyCzas);
            }
            return dziecko;
        }

        public Klasa wczytajSrodkowego(XmlNode wezelSrodkowy, Klasa rodzic)
        {
            Klasa srodkowy = null;
            int gwarantowanaSzybkoscBitowa = int.Parse(wezelSrodkowy.SelectSingleNode("gwarantowanaSzybkoscBitowa").InnerText);
            int szczytowaSzybkoscBitowa = int.Parse(wezelSrodkowy.SelectSingleNode("szczytowaSzybkoscBitowa").InnerText);
            int liczbaDzieci = int.Parse(wezelSrodkowy.SelectSingleNode("dzieci/liczbaDzieci").InnerText);
            XmlNodeList listaDzieci = wezelSrodkowy.SelectNodes("dzieci/dziecko");
            srodkowy = new Klasa(gwarantowanaSzybkoscBitowa, szczytowaSzybkoscBitowa, rodzic.Poziom - 1, rodzic, Klasa.Typ.srodkowa, aktualnyCzas);
            Klasa[] dzieci = new Klasa[liczbaDzieci];
            for (int numerDziecka = 0; numerDziecka < liczbaDzieci; numerDziecka++)
            {
                if (listaDzieci.Item(numerDziecka).SelectSingleNode("typ").InnerText.Equals("lisc"))
                {
                    dzieci[numerDziecka] =  wczytajLiscia(listaDzieci.Item(numerDziecka), srodkowy);
                }
                else
                {
                    dzieci[numerDziecka] = wczytajSrodkowego(listaDzieci.Item(numerDziecka), srodkowy);

                }
            }
            srodkowy.Dzieci = dzieci;
            return srodkowy;
        }

        public void zapisz(int czasSymulacji)
        {
            FileStream fs = new FileStream(plikWynikowy,
                    FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine("OBCIAZENIE SYSTEMU:");
                sw.WriteLine("Srednia zajetosc lacza: " + czasZajetosciLacza / aktualnyCzas * 100 + "%");
                sw.WriteLine("Srednia zajetosc kolejek:");
                for (int i = 0; i < liczbaKolejek; i++)
                {
                    sw.WriteLine("Kolejka nr " + i);

                    zajetoscKolejek[i] += (aktualnyCzas - czasOstatniejOperacjiNaKolejce[i]) * kolejki[i].LiczbaPakietowWKolejce;
                    sw.WriteLine("srednia zajetosc kolejki  nr " + i + "=" + zajetoscKolejek[i] / czasSymulacji);
                    sw.WriteLine("srednia szybkosc bitowa kolejki  nr " + i + "=" + liczbaBitowWyslanychZKolejki[i] / czasSymulacji);


                }

                for (int i = 0; i < liczbaStrumieni; i++)
                {
                    if (pakietyPrzybyle[i] == 0)
                        sw.WriteLine("prawdopodobienstwo odrzucenia pakietu pochadzacego ze strumienia nr " +
                            i + " nie moze zostac obliczone z powodu braku obsluzonych pakietow ");
                    else
                        sw.WriteLine("prawdopodobienstwo odrzucenia pakietu pochadzacego ze strumienia " + 
                            i + "= " + (float)pakietyOdrzucone[i] / (float)pakietyPrzybyle[i]);
                    if (liczbaPakietowObsluzonych[i] == 0)
                        sw.WriteLine("sredni czas obslugi pakietu w strumieniu nr" + 
                            i + " nie moze zostac obliczony z powodu braku obsluzonych pakietow");
                    else
                    {
                        sw.WriteLine("sredni czas obslugi pakietu w strumieniu  nr " +
                            i + "= " + skumulowanyCzasObslugi[i] / liczbaPakietowObsluzonych[i]);
                        sw.WriteLine("liczba pakietow obsluzonych w strumieniu  nr " +
                            i + "= " + liczbaPakietowObsluzonych[i]);
                    }
                        
                }
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
    
}
