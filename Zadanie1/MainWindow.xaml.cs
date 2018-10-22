using System.Windows;
using System;
using System.Text;

namespace Kryptografia_OTP_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{
    public partial class MainWindow : Window
    {
        Int16 [] iMessage;
        Int16 [] iKey;
        Int16 [] iCryptogram;
        Int16 [] iDecoded;

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
                iMessage = new Int16[insertedTextBox.Text.Length];
                for(Int16 i = 0; i < iMessage.Length; i++)
                {
                    iMessage[i] = (Int16)insertedTextBox.Text[i];
                }
                displayBinaryTextBox.Text = ConvertDecStringToBinString(iMessage);
                ConvertToBinButton.IsEnabled = false;
                insertedTextBox.IsReadOnly = true;
                GenerateKeyButton.IsEnabled = true;
            }
        }

        private void GenerateKeyEventHandler(object sender, RoutedEventArgs e)
        {
            Random randomGenerator = new Random();
            iKey = new Int16[iMessage.Length];
            for(int i = 0; i < iKey.Length; i++)
            {
                iKey[i] = (Int16)randomGenerator.Next(Int16.MaxValue);
            }
            
            keyTextBox.Text = ConvertDecStringToBinString(iKey);
            EncodeButton.IsEnabled = true;
        }

        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iCryptogram = (Int16[])iMessage.Clone();

            for (Int16 i = 0; i < iCryptogram.Length; i++)
            {
                iCryptogram[i] ^= iKey[i];
            }

            cryptogramTextBox.Text = ConvertDecStringToBinString(iCryptogram);
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iDecoded = (Int16[])iCryptogram.Clone();

            for (Int16 i = 0; i < iCryptogram.Length; i++)
            {
                iDecoded[i] ^= iKey[i];
            }

            decodedBinTextBox.Text = ConvertDecStringToBinString(iDecoded);
            StringBuilder stringBuilder = new StringBuilder();
            foreach(Int16 value in iDecoded)
            {
                stringBuilder.Append((char)value);
            }
            decodedTextBox.Text = stringBuilder.ToString();
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

        public string ConvertDecStringToBinString(Int16 [] aints)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (Int16 i = 0; i < aints.Length; i++)
            {
                string tmp = Convert.ToString(aints[i], 2);
                if (tmp.Length < 16)
                {
                    stringBuilder.Append(tmp.PadLeft(16, '0'));
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
