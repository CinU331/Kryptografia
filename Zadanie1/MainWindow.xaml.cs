using System.Windows;
using System;
using System.Text;

namespace Kryptografia_OTP_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{
    public partial class MainWindow : Window
    {
        byte [] iMessage;
        byte [] iKey;
        byte [] iCryptogram;
        byte [] iDecoded;

        public MainWindow()
        {
            InitializeComponent();

            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;
            
        }


        private void ConvertToBinEventHandler(object sender, RoutedEventArgs e)
        {
            if (insertedTextBox.Text.Length != 0)
            {
                iMessage = System.Text.Encoding.UTF8.GetBytes(insertedTextBox.Text);
                displayBinaryTextBox.Text = ConvertDecStringToBinString(iMessage);
                ConvertToBinButton.IsEnabled = false;
                insertedTextBox.IsReadOnly = true;
                GenerateKeyButton.IsEnabled = true;
            }
        }
        private void ResetEventHandler(object sender, RoutedEventArgs e)
        {
            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;
            ConvertToBinButton.IsEnabled = true;
            insertedTextBox.IsReadOnly = false;
            GenerateKeyButton.IsEnabled = false;


            cryptogramTextBox.Clear();
            decodedTextBox.Clear();
            displayBinaryTextBox.Clear();
            insertedTextBox.Clear();
            keyTextBox.Clear();
            decodedBinTextBox.Clear();
        }

        private void GenerateKeyEventHandler(object sender, RoutedEventArgs e)
        {
            GenerateKey();
            keyTextBox.Text = ConvertDecStringToBinString(iKey);
            EncodeButton.IsEnabled = true;
        }

        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            Encode();
            cryptogramTextBox.Text = ConvertDecStringToBinString(iCryptogram);
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            Decode();
            decodedBinTextBox.Text = ConvertDecStringToBinString(iDecoded);
            decodedTextBox.Text = Encoding.UTF8.GetString(iDecoded);
        }

        public void Encode()
        {
            iCryptogram = (byte[])iMessage.Clone();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iCryptogram[i] ^= iKey[i];
            }
        }
        public void Decode()
        {
            iDecoded = (byte[])iCryptogram.Clone();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iDecoded[i] ^= iKey[i];
            }
        }

        public void GenerateKey()
        {
            Random randomGenerator = new Random();
            iKey = new byte[iMessage.Length];
            randomGenerator.NextBytes(iKey);
        }

        public string ConvertDecStringToBinString(byte [] aBytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < aBytes.Length; i++)
            {
                string tmp = Convert.ToString(aBytes[i], 2);
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
