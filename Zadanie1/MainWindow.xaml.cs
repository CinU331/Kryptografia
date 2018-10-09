using System.Windows;
using System.IO;
using System;
using System.Text;

namespace Zadanie1_Marcin_Kwapisz_Dawid_Gierowski
{
    public partial class MainWindow : Window
    {
        byte [] message;
        byte [] key;
        byte [] cryptogram;
        byte [] decoded;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Konwersja_Do_Bin(object sender, RoutedEventArgs e)
        {
            message = System.Text.Encoding.UTF8.GetBytes(wprowadzoneTextBox.Text);
            wyswietlBinarnieTextBox.Text = ConvertDecStringToBinString(message);
        }

        private void Generuj_Klucz(object sender, RoutedEventArgs e)
        {
            GenerujKlucz();
            kluczTextBox.Text = ConvertDecStringToBinString(key);
        }

        private void Zakoduj_Wiadomosc(object sender, RoutedEventArgs e)
        {
            Zakoduj();
            szyfrogramTextBox.Text = ConvertDecStringToBinString(cryptogram);
        }

        private void Dekoduj_Wiadomosc(object sender, RoutedEventArgs e)
        {
            Dekoduj();
            zdekodowanyBinTextBox.Text = ConvertDecStringToBinString(decoded);
            zdekodowanyTextBox.Text = Encoding.UTF8.GetString(decoded);
        }

        public void Zakoduj()
        {
            cryptogram = (byte[])message.Clone();

            for (int i = 0; i < cryptogram.Length; i++)
            {
                cryptogram[i] ^= key[i];
            }
        }
        public void Dekoduj()
        {
            decoded = (byte[])cryptogram.Clone();

            for (int i = 0; i < cryptogram.Length; i++)
            {
                decoded[i] ^= key[i];
            }
        }

        public void GenerujKlucz()
        {
            Random randomGenerator = new Random();
            key = new byte[message.Length];
            randomGenerator.NextBytes(key);
        }

        public string ConvertDecStringToBinString(byte [] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                string tmp = Convert.ToString(bytes[i], 2);
                if (tmp.Length < 8)
                {
                    stringBuilder.Append(tmp.PadLeft(8, '0'));
                }
                else
                {
                    stringBuilder.Append(tmp);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
