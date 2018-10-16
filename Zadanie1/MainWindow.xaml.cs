using System;
using System.Collections;
using System.Collections.Generic;
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

        private int privateKey1;
        private int privateKey2;
        private int publicKey;

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

                for (int i = 0; i < letters.Length; i++)
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

            for( int i = 0; i < decoded1.Length; i++)
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
            for (int i = 0; i < iMessage.Length; i++)
            {
                int squareValue = iMessage[i] * iMessage[i];
                int newValue = MathMod(squareValue, publicKey);
                iCryptogram[i] = (Int16)newValue;
            }
        }
        public void Decode()
        {
            decoded1 = new Int16[iCryptogram.Length];
            decoded2 = new Int16[iCryptogram.Length];
            decoded3 = new Int16[iCryptogram.Length];
            decoded4 = new Int16[iCryptogram.Length];
            int a = 0, b = 0;
            ComputeFactors(privateKey1, privateKey2, ref a, ref b);
            for (int i = 0; i < iCryptogram.Length; i++)
            {
                int r = LargeModFromString(PowerToString(iCryptogram[i].ToString(), (privateKey1 + 1) / 4), privateKey1);
                int s = LargeModFromString(PowerToString(iCryptogram[i].ToString(), (privateKey2 + 1) / 4), privateKey2);

                int x = MathMod((a * privateKey1 * s + b * privateKey2 * r), publicKey);
                int y = MathMod((a * privateKey1 * s - b * privateKey2 * r), publicKey);

                decoded1[i] = (Int16)MathMod(x, publicKey);
                decoded2[i] = (Int16)MathMod(-x, publicKey);
                decoded3[i] = (Int16)MathMod(y, publicKey);
                decoded4[i] = (Int16)MathMod(-y, publicKey);
            }
        }

        public string ConvertDecStringToBinStringUnicode(Int16[] unicodeValues)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < unicodeValues.Length; i++)
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
            ArrayList arrayList = new ArrayList
            {
                3, 7, 11, 19, 23, 31, 43,
                47, 59,  67, 71, 79, 83,
                103, 107, 127, 131, 139,
                151, 163, 167, 179,  191, 199,
                211, 223, 227, 239, 241, 251
            };
            Random random = new Random();

            int randIndex = random.Next(arrayList.Count);

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

        public int ComputeFactors(int p, int q, ref int a, ref int b)
        {
            // Base Case 
            if (p == 0)
            {
                a = 0;
                b = 1;
                return q;
            }

            int x1 = 1, y1 = 1;
            int gcd = ComputeFactors(q % p, p, ref x1, ref y1);

            a = y1 - (q / p) * x1;
            b = x1;

            return gcd;
        }

        public string ConvertIntToBinaryString(int value)
        {
            string tmp = Convert.ToString(value, 2);
            if (tmp.Length < 16)
            {
                tmp = (tmp.PadLeft(16, '0'));
            }

            return tmp;
        }

        // Implemented due to diffrences between % operator and mod operation in mathematics
        private static int MathMod(double a, double b)
        {
            return (int)((Math.Abs(a * b) + a) % b);
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

        // Adds two lists of chars, so it behaves like adding numbers manually
        public static List<char> AddCharsets(List<char> aFirst, List<char> aSecond)
        {
            // Adding zeroes on the left
            if (aFirst.Count != aSecond.Count)
            {
                List<char> bigger;
                List<char> smaller;
                int maxLenght = aFirst.Count > aSecond.Count ? aFirst.Count : aSecond.Count;

                if (aFirst.Count == maxLenght)
                {
                    bigger = aFirst;
                    smaller = aSecond;
                }

                else
                {
                    smaller = aFirst;
                    bigger = aSecond;
                }

                List<char> zeros = new List<char>();

                for (int i = 0; i < maxLenght - smaller.Count; i++)
                {
                    zeros.Add('0');
                }
                smaller.InsertRange(0, zeros);
            }

            byte overflow = 0;

            // Calculating value for all indexes and transferring result to aFirst
            for (int i = aFirst.Count - 1; i >= 0; i--)
            {
                byte sum = (byte)(ParseChar(aFirst[i]) + ParseChar(aSecond[i]) + overflow);
                if (sum >= 10)
                {
                    if (i > 0)
                    {
                        aFirst[i] = sum.ToString()[1];
                        overflow = 1;
                    }
                    else
                    {
                        aFirst[i] = sum.ToString()[1];
                        aFirst.InsertRange(0, new List<char> { '1' });
                        break;
                    }
                }
                else
                {
                    aFirst[i] = ParseByte(sum);
                    overflow = 0;
                }
            }
            return aFirst;
        }

        public static string MultiplyToString(string aFirst, string aSecond)
        {
            char[] biggerValueChars;
            char[] smallerValueChars;

            if (aFirst.Length > aSecond.Length)
            {
                biggerValueChars = aFirst.ToCharArray();
                smallerValueChars = aSecond.ToCharArray();
            }
            else
            {
                biggerValueChars = aSecond.ToCharArray();
                smallerValueChars = aFirst.ToCharArray();
            }

            byte smaller = 0;
            byte bigger = 0;
            byte multiplication = 0;

            List<List<char>> storage = new List<List<char>>();

            List<char> smallerValueLetters = new List<char>();

            // Rewriting char arrays backwards and transferring to lists
            for (int i = smallerValueChars.Length - 1; i >= 0; i--)
            {
                smallerValueLetters.Add(smallerValueChars[i]);
            }

            // Process of muliplying by each cypher if number
            for (int i = 0; i < smallerValueLetters.Count; i++)
            {
                List<char> biggerValueLetters = new List<char>();

                for (int j = biggerValueChars.Length - 1; j >= 0; j--)
                {
                    biggerValueLetters.Add(biggerValueChars[j]);
                }

                smaller = ParseChar((char)smallerValueLetters[i]);
                byte overflow = 0;

                for (int j = 0; j < biggerValueLetters.Count; j++)
                {
                    bigger = ParseChar((char)biggerValueLetters[j]);
                    multiplication = (byte)((smaller * bigger) + overflow);
                    string multiplicationString = multiplication.ToString();

                    biggerValueLetters[j] = multiplicationString[multiplicationString.Length - 1];

                    if (multiplication >= 10)
                    {
                        if (j == biggerValueLetters.Count - 1)
                        {
                            biggerValueLetters.Add(multiplicationString[0]);
                            break;
                        }
                        overflow = ParseChar(multiplicationString[0]);
                    }
                    else
                    {
                        overflow = 0;
                    }
                }
                storage.Add(biggerValueLetters);
            }

            // Preparing list components to summing
            for (int i = 0; i < storage.Count; i++)
            {
                storage[i].Reverse();
                for (int j = 0; j < i; j++)
                {
                    storage[i].Add('0');
                }
            }

            for (int i = 1; i < storage.Count; i++)
            {
                AddCharsets(storage[0], storage[i]);
            }

            // Converting result to string
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in storage[0])
            {
                stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }

        public static int LargeModFromString(string numberString, int devider)
        {
            int res = 0;

            for (int i = 0; i < numberString.Length; i++)
            {
                res = (res * 10 + (int)numberString[i] -
                       '0') % devider;
            }

            return res;
        }

        public static string PowerToString(string aBase, int aExponent)
        {
            string baseValueString = (string)aBase.Clone();

            for (int i = aExponent; i > 1; i--)
            {
                aBase = MultiplyToString(aBase, baseValueString);
            }
            return aBase;
        }
    }
}