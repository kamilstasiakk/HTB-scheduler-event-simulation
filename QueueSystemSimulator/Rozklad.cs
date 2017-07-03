using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Rozklad
{
    private string nazwa;
    private double lambda;

    public string Nazwa
    {
        get
        {
            return nazwa;
        }
    }

    public double Lambda
    {
        get
        {
            return lambda;
        }
    }

    public Rozklad(string nazwa, double lambda)
    {
        this.nazwa = nazwa;
        this.lambda = lambda;
    }

    // zwraca czas nadejscia lub wielkosc pakietu zgodnie z rozkladem poissona
    public double WygenerujWartosc(double y)
    {
        return ((-1) * Math.Log(1 - y) / lambda); //przeksztalcony wzor na rozklad wykladniczy
    }
}
