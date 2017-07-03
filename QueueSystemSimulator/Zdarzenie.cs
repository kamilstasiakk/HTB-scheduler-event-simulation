using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Zdarzenie
{
    public enum TypZdarzenia { przybycie_pakietu = 0, obsluga_pakietu = 1 };


    private TypZdarzenia typ;
    private double czas;
    private int numerStrumienia;

    
    public Zdarzenie(TypZdarzenia zdarzenie, double czas, int numerStrumienia)
    {
        typ = zdarzenie;
        this.czas = czas;
        this.numerStrumienia = numerStrumienia;
    }

    

    public int NumerStrumienia
    {
        get
        {
            return numerStrumienia;
        }
    }

    public double Czas
    {
        get
        {
            return czas;
        }
    }


    public TypZdarzenia Typ
    {
        get
        {
            return typ;
        }
    }
}
