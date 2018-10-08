using System.Windows;
using System.IO;
using System;

namespace Zadanie1_Marcin_Kwapisz_Dawid_Gierowski
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //macierz kodująca:
        private byte[,] h = new byte[,]
               {{1,1,1,0,0,1,1,1,1,0,0,0,0,0,0,0},
                {0,1,1,1,1,0,1,1,0,1,0,0,0,0,0,0},
                {1,0,0,1,0,1,0,1,0,0,1,0,0,0,0,0},
                {1,1,1,0,1,0,0,1,0,0,0,1,0,0,0,0},
                {0,1,0,1,0,1,1,0,0,0,0,0,1,0,0,0},
                {1,0,1,0,1,0,1,0,0,0,0,0,0,1,0,0},
                {1,1,0,0,1,1,0,0,0,0,0,0,0,0,1,0},
                {1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1}};
        private byte[] tablicaBinarna;

        #region Elementy graficzne
        public void Zapisz_do_pliku_txt(object sender, RoutedEventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("wprowadzone.txt"))
            {
                writer.Write(wprowadzone.Text);
            }

        }
        public void Otworz_zapisany_txt(object sender, RoutedEventArgs e)
        {
            wyswietlZapisane.Clear();
            using (StreamReader sr = new StreamReader("wprowadzone.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    wyswietlZapisane.Text += line + Environment.NewLine;
                }
            }
        }
        public void Przycisk_Zakoduj(object sender, RoutedEventArgs e)
        {
            tablicaBinarna = Odczytaj("wprowadzone.txt");
            Zapisz_do_bin(tablicaBinarna);
        }
        public void Przycisk_Dekoduj(object sender, RoutedEventArgs e)
        {
            //czyszczenie TextBox'ów
            czyBledy.Clear();
            odkodowany.Clear();
            //właściwe działanie przycisku
            tablicaBinarna = Odczytaj("zakodowane.bin");
            Zapisz_do_txt(tablicaBinarna); 
            using (StreamReader sr = new StreamReader("odkodowane.txt"))
            {
                String linia;
                while ((linia = sr.ReadLine()) != null)
                {
                    odkodowany.Text += linia + Environment.NewLine;
                }
            }
        }
        #endregion

        #region Konwersja binarne <=> dziesiętne
        public byte[] Na_binarne(byte x)    //liczba dziesiętna -> liczba binarna
        {
            char bit = new char();
            byte[] binarnie = new byte[8];
            String str = Convert.ToString(x, 2);
            for (int i = str.Length - 1, j = 7; i >= 0; i--, j--)
            {
                bit = str[i];
                if (bit == '1')
                {
                    binarnie[j] = 1;
                }
                else
                {
                    binarnie[j] = 0;
                }
            }
            return binarnie;
        }
        public byte Na_dziesiętne(byte[] Tab)    //liczba binarna -> liczba dziesiętna
        {
            byte dziesiętnie = new byte();
            for (int i = 0; i < Tab.Length; i++)
            {
                dziesiętnie += (byte)(Math.Pow(2, i) * Tab[Tab.Length - i - 1]);
            }
            return dziesiętnie;
        }
        #endregion

        #region Kodowanie i dekodowanie
        public byte[] Zakoduj(byte[] tab)
        {
            byte[] zakodowaneBajty = new byte[tab.Length * 2];  //8 bitów * 2, ponieważ 8 bitów to bity danych, a 8 będzie bitami kontrolnymi
            for (int i = 0; i < tab.Length; i++)
            {
                zakodowaneBajty[i * 2] = tab[i];    //bity danych bez zmian
                zakodowaneBajty[i * 2 + 1] = Na_dziesiętne(Iloczyn_z_macierzą_kodującą(Na_binarne(tab[i])));    //bity kontrolne jako wynik iloczynu bitów danych z macierzą kodującą
            }
            return zakodowaneBajty;
        }
        public byte[] Dekoduj(byte[] tab)
        {
            byte[] zdekodowaneBajty = new byte[tab.Length / 2];
            byte[] pomocnicza;
            byte[] znak;
            bool błąd = false;
            for (int i = 0; i < zdekodowaneBajty.Length; i++)
            {
                znak = Połącz_bajt_danych_i_kontrolny(Na_binarne(tab[i * 2]), Na_binarne(tab[i * 2 + 1]));  //odczyt jednego znaku jako 8 bitów danych i 8 bitów kontrolnych
                pomocnicza = Iloczyn_z_macierzą_kodującą(znak);
                if (Czy_wektor_zerowy(pomocnicza))   //sprawdzenie czy wynikiem iloczynu jest wektor zerowy, jeśli tak to błędy nie wystąpiły
                {
                    //jeśli nie ustawiamy flagę błąd i następnie sprawdzamy ilość błędów
                    błąd = true;
                    if (Jeden_Błąd(pomocnicza, ref znak))
                    {
                        Informacje_o_błędach(tab[i * 2], tab[i * 2 + 1], znak, 1);  //wypisanie komunikatu i poprawa 1 bita
                        for (int j = 0; j < 8; j++)
                        {
                            pomocnicza[j] = znak[j];
                        }
                        zdekodowaneBajty[i] = Na_dziesiętne(pomocnicza);
                    }
                    else if (Dwa_Błędy(pomocnicza, ref znak))
                    {
                        Informacje_o_błędach(tab[i * 2], tab[i * 2 + 1], znak, 2);  //wypisanie komunikatu i poprawa 2 bitów
                        for (int j = 0; j < 8; j++)
                        {
                            pomocnicza[j] = znak[j];
                        }
                        zdekodowaneBajty[i] = Na_dziesiętne(pomocnicza);
                    }
                    else
                    {
                        Informacje_o_błędach(tab[i * 2], tab[i * 2 + 1], znak, 3);  //jeśli więcej niż 2 błędy, komunikat o niemożliwości ich naprawienia
                    }
                }
                else  //jeśli brak błedów, przypisujemy te otrzymane
                {  
                    for (int j = 0; j < 8; j++)
                    {
                        pomocnicza[j] = znak[j];
                    }
                    zdekodowaneBajty[i] = Na_dziesiętne(pomocnicza);
                }
            }
            if (błąd == false)
            czyBledy.Text = "Nie wykryto błędów.";
            return zdekodowaneBajty;
        }
        #endregion

        #region Odczyt i zapis do plików
        public byte[] Odczytaj(String nazwaPliku)
        {
            byte[] fileBytes = File.ReadAllBytes(nazwaPliku);   //zapis bajt po bajcie do pliku
            return fileBytes;
        }
        public void Zapisz_do_bin(byte[] tab)
        {
            byte[] zakodowaneBajty = Zakoduj(tab);  //kodowanie
            using (BinaryWriter bw = new BinaryWriter(File.Open("zakodowane.bin", FileMode.Create)))
            {
                for (int i = 0; i < zakodowaneBajty.Length; i++)
                {
                    bw.Write(zakodowaneBajty[i]);   //zapis zakodowanych bajtów do pliku bin
                }
            }
        }
        public void Zapisz_do_txt(byte[] tab)
        {
            byte[] zdekodowaneBajty = Dekoduj(tab); //dekodowanie
            using (BinaryWriter bw = new BinaryWriter(File.Open("odkodowane.txt", FileMode.Create)))
            {
                for (int i = 0; i < zdekodowaneBajty.Length; i++)
                {
                    bw.Write(zdekodowaneBajty[i]);  //zapis odkodowanych bajtów do pliku txt
                }
            }
        }
        #endregion

        #region Łączenie bitów danych i kontrolnych oraz iloczyn z macierzą kodującą
        public byte[] Połącz_bajt_danych_i_kontrolny(byte[] x, byte[] y)    //połączenie dwóch bajtów do jednego wektora
        {
            byte[] połączone = new byte[16];
            for (int i = 0; i < x.Length; i++)
            {
                połączone[i] = x[i];
                połączone[i + 8] = y[i];
            }
            return połączone;
        }
        public byte[] Iloczyn_z_macierzą_kodującą(byte[] Tab)   //mnożenie wektora z macierzą kodującą
        {
            byte[] wynik = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < Tab.Length; j++)
                {
                    wynik[i] += (byte)(Tab[j] * h[i, j]);
                }
                wynik[i] %= 2;
            }
            return wynik;
        }
        #endregion

        #region Obsługa błędów
        public bool Czy_wektor_zerowy(byte[] tab)   //sprawdzenie czy tablica bajtów jest zerowa
        {
            for (int i = 0; i < tab.Length; i++)
            {
                if (tab[i] != 0) return true;
            }
            return false;
        }
        public bool Jeden_Błąd(byte[] tab, ref byte[] Znak) //sprawdzenie czy błąd dotyczy jednego bitu
        {
            bool jest;
            for (int i = 0; i < 16; i++)
            {
                jest = true;
                for (int j = 0; j < 8; j++)
                {
                    if (h[j, i] != tab[j])
                    {
                        jest = false;
                        break;
                    }
                }
                if (jest)   //naprawienie bitu
                {
                    NaprawBit(i, ref Znak);
                    return true;
                }
            }
            return false;
        }
        public bool Dwa_Błędy(byte[] Tab, ref byte[] Znak)  //sprawdzenie czy błąd dotyczy dwóch bitów
        {
            bool jest;
            for (int i = 0; i < 16 - 1; i++)
            {
                for (int j = i + 1; j < 16; j++)
                {
                    jest = true;
                    for (int k = 0; k < 8; k++)
                    {
                        if (((h[k, i] + h[k, j]) % 2) != Tab[k])
                        {
                            jest = false;
                            break;
                        }
                    }
                    if (jest)   //naprawienie bitów
                    {
                        NaprawBit(i, ref Znak);
                        NaprawBit(j, ref Znak);
                        return true;
                    }
                }
            }
            return false;
        }
        public void Informacje_o_błędach(byte x, byte y, byte[] z, int i)   //komunikaty o znalezionych i naprawionych błędach
        {
            byte[] tab = Na_binarne(x);
            if (i == 1)
            {
                czyBledy.Text += "Wystąpił jeden błąd:" + Environment.NewLine;
            }
            if (i == 2)
            {
                czyBledy.Text += "Wystąpiły dwa błędy:" + Environment.NewLine;
            }
            if (i == 3)
            {
                czyBledy.Text = "Wystąpiły więcej niż dwa błędy, brak możliwości naprawy";
            }
            if (i == 1 || i == 2)
            {
                for (int k = 0; k < tab.Length; k++)
                {
                    czyBledy.Text += (tab[k].ToString());
                }
                tab = Na_binarne(y);
                czyBledy.Text += " ";
                for (int k = 0; k < tab.Length; k++)
                {
                    czyBledy.Text += (tab[k].ToString());
                }
                if (i == 1 || i == 2)
                {
                    czyBledy.Text += " naprawiono na: ";
                    for (int k = 0; k < z.Length; k++)
                    {
                        if (k == 8) czyBledy.Text += " ";
                        czyBledy.Text += (z[k].ToString());
                    }
                    czyBledy.Text += Environment.NewLine;
                }
            }
        }
        public void NaprawBit(int i, ref byte[] tab)    //negacja bitu
        {
            tab[i] += 1;
            tab[i] %= 2;
        }
        #endregion
    }
}
