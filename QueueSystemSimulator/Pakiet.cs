class Pakiet
{
    private double dlugosc;
    private int numerStrumienia;
    private double czas;


    public Pakiet(double dlugosc, int strumien, double czas)
    {
        this.dlugosc = dlugosc;
        numerStrumienia = strumien;
        this.czas = czas;
    }


    public double Dlugosc
    {
        get
        {
            return dlugosc;
        }
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
}

