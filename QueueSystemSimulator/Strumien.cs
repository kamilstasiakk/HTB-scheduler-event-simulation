using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Strumien
{
    string nazwa;
    int numerKolejki;
    private Rozklad rozkladCzasu;
    private Rozklad rozkladDlugosci;
    int stalaDlugoscPakietow;

    public Rozklad RozkladCzasu
    {
        get
        {
            return rozkladCzasu;
        }
    }

    public Rozklad RozkladDlugosci
    {
        get
        {
            return rozkladDlugosci;
        }
    }


    public int NumerKolejki
    {
        get
        {
            return numerKolejki;
        }
    }

    public int StalaDlugoscPakietow
    {
        get
        {
            return stalaDlugoscPakietow;
        }
    }

    public Strumien(string nowaNazwa, int kolejka, Rozklad czas, Rozklad obsluga, int dlugosPakietu)
    {
        nazwa = nowaNazwa;
        numerKolejki = kolejka;
        rozkladCzasu = czas;
        stalaDlugoscPakietow = dlugosPakietu;
        rozkladDlugosci = obsluga;
    }




}

