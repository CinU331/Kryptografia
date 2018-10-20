using System;
using System.Collections;
using System.Text;
using System.Windows;

namespace Kryptografia_Rabin_Cryptosystem_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{
    public partial class MainWindow : Window
    {
        private Int16[] iMessage;
        private Int16[] iCryptogram;

        private Int16[] decoded1;
        private Int16[] decoded2;
        private Int16[] decoded3;
        private Int16[] decoded4;

        private Int32 privateKey1;
        private Int32 privateKey2;
        private Int32 publicKey;

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
                var letters = insertedTextBox.Text.ToCharArray();
                iMessage = new Int16[letters.Length];
                StringBuilder stringBuilder = new StringBuilder();

                for (Int32 i = 0; i < letters.Length; i++)
                {
                    iMessage[i] = (Int16)letters[i];
                    stringBuilder.Append(ConvertIntToBinaryString(iMessage[i]));
                }

                displayBinaryTextBox.Text = stringBuilder.ToString();

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

            iMessage = null;
            iCryptogram = null;

            decoded1 = null;
            decoded2 = null;
            decoded3 = null;
            decoded4 = null;

            privateKey1 = 0;
            privateKey2 = 0;
            publicKey = 0;
        }

        private void GenerateKeyEventHandler(object sender, RoutedEventArgs e)
        {
            GenerateKeys();
            keyTextBox.Text = "Private key:\np = " + privateKey1.ToString() + "\nq = " + privateKey2.ToString() + "\nPublic key = " + publicKey.ToString();
            EncodeButton.IsEnabled = true;
        }

        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            Encode();
            cryptogramTextBox.Text = ConvertDecStringToBinStringUnicode(iCryptogram);
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            Decode();

            decodedBinTextBox.Text = ConvertDecStringToBinStringUnicode(decoded1) + "\n" +
                ConvertDecStringToBinStringUnicode(decoded2) + "\n" +
                ConvertDecStringToBinStringUnicode(decoded3) + "\n" +
                ConvertDecStringToBinStringUnicode(decoded4) + "\n";

            StringBuilder output1 = new StringBuilder();
            StringBuilder output2 = new StringBuilder();
            StringBuilder output3 = new StringBuilder();
            StringBuilder output4 = new StringBuilder();

            for (Int32 i = 0; i < decoded1.Length; i++)
            {
                output1.Append(i + ". " + (char)decoded1[i] + " ");
                output2.Append(i + ". " + (char)decoded2[i] + " ");
                output3.Append(i + ". " + (char)decoded3[i] + " ");
                output4.Append(i + ". " + (char)decoded4[i] + " ");
            }

            decodedTextBox.Text = output1 + "\n" +
               output2 + "\n" +
                output3 + "\n" +
                output4 + "\n";
        }
        public void Encode()
        {
            iCryptogram = new Int16[iMessage.Length];
            publicKey = privateKey1 * privateKey2;
            for (Int32 i = 0; i < iMessage.Length; i++)
            {
                Int32 newValue = (int)FastModulo(iMessage[i], 2, publicKey);
                iCryptogram[i] = (Int16)newValue;
            }
        }
        public void Decode()
        {
            decoded1 = new Int16[iCryptogram.Length];
            decoded2 = new Int16[iCryptogram.Length];
            decoded3 = new Int16[iCryptogram.Length];
            decoded4 = new Int16[iCryptogram.Length];
            Int32 a = 0, b = 0;
            ComputeFactors(privateKey1, privateKey2, ref a, ref b);
            for (Int32 i = 0; i < iCryptogram.Length; i++)
            {
                Int32 r = (int)FastModulo(iCryptogram[i], (privateKey1 + 1) / 4, privateKey1);
                Int32 s = (int)FastModulo(iCryptogram[i], (privateKey2 + 1) / 4, privateKey2);

                Int32 x = (int)FastModulo(FastModulo(a * privateKey1 * s, 1, publicKey) + FastModulo(b * privateKey2 * r, 1, publicKey), 1, publicKey);
                Int32 y = (int)FastModulo(FastModulo(a * privateKey1 * s, 1, publicKey) - FastModulo(b * privateKey2 * r, 1, publicKey), 1, publicKey);

                decoded1[i] = (Int16)FastModulo(x, 1, publicKey);
                decoded2[i] = (Int16)FastModulo(-x, 1, publicKey);
                decoded3[i] = (Int16)FastModulo(y, 1, publicKey);
                decoded4[i] = (Int16)FastModulo(-y, 1, publicKey);
            }
        }

        public string ConvertDecStringToBinStringUnicode(Int16[] unicodeValues)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (Int32 i = 0; i < unicodeValues.Length; i++)
            {
                string tmp = Convert.ToString(unicodeValues[i], 2);
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

        public void GenerateKeys()
        {
            //Random random = new Random();
            //int width = 1000;
            //int point = random.Next(width, int.MaxValue);

            //bool isFound = false;
            //for (int i = point - 1000; i < point && isFound == false; i++)
            //{
            //    bool isPrime = true;
            //    for (int j = 2; j <= Math.Sqrt(i); j++)
            //    {

            //        if (i % j == 0)
            //        {
            //            isPrime = false;
            //            break;
            //        }
            //    }
            //    if (isPrime)
            //    {
            //        iPrimeP = i;
            //        isFound = true;
            //    }
            ArrayList arrayList = new ArrayList
            {
                3, 7, 11, 19, 23, 31, 43,
                47, 59,  67, 71, 79, 83,
                103, 107, 127, 131, 139,
                151, 163, 167, 179,  191, 199,
                211, 223, 227, 239, 241, 251
            };
            Random random = new Random();

            Int32 randIndex = random.Next(arrayList.Count);

            if ((int)arrayList[randIndex] % 4 == 3)
            {
                privateKey1 = (int)arrayList[randIndex];
            }

            arrayList.RemoveAt(randIndex);
            randIndex = random.Next(arrayList.Count);

            if ((int)arrayList[randIndex] % 4 == 3)
            {
                privateKey2 = (int)arrayList[randIndex];
            }

            publicKey = privateKey1 * privateKey2;
        }

        public Int32 ComputeFactors(Int32 p, Int32 q, ref Int32 a, ref Int32 b)
        {
            // Base Case 
            if (p == 0)
            {
                a = 0;
                b = 1;
                return q;
            }

            Int32 x1 = 1, y1 = 1;
            Int32 gcd = ComputeFactors(q % p, p, ref x1, ref y1);

            a = y1 - (q / p) * x1;
            b = x1;

            return gcd;
        }

        public string ConvertIntToBinaryString(Int32 value)
        {
            string tmp = Convert.ToString(value, 2);
            if (tmp.Length < 16)
            {
                tmp = (tmp.PadLeft(16, '0'));
            }
            return tmp;
        }


        // Converts character to cypher, which it represents
        public static byte ParseChar(char character)
        {
            return byte.Parse(character.ToString());
        }

        // Converts cypher to character representin it
        public static char ParseByte(byte number)
        {
            return Convert.ToString(number)[0];
        }


        private Int64 FastModulo(Int64 aBase, Int64 aExp, Int64 aModulo)
        {
            if (aBase < 0)
            {
                aBase = Math.Abs(aBase * aModulo) + aBase;
            }
            if (aExp == 0)
            {
                return 1;
            }

            if (aExp == 1)
            {
                return aBase % aModulo;
            }

            Int64 t = FastModulo(aBase, aExp / 2, aModulo);
            t = (t * t) % aModulo;

            if (aExp % 2 == 0)
            {
                return t;
            }
            else
            {
                return ((aBase % aModulo) * t) % aModulo;
            }
        }

    }
}