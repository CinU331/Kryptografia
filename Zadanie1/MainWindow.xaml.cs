using System;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Numerics;
using System.Linq;
using Microsoft.Win32;
using System.IO;

namespace Kryptografia_Elgamal_signature_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{

    public partial class MainWindow : Window
    {
        private byte[] messageBytes;
        private byte[] messageHashBytes;
        private BigInteger iMessageHashValue;
        private BigInteger iSignature;
        private BigInteger p;
        private BigInteger g;
        private BigInteger y;

        private BigInteger x;
        private BigInteger r;

        public MainWindow()
        {
            InitializeComponent();
            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;
            ReloadSignature.IsEnabled = false;
        }


        private void ReadDataFromFileEventHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                messageBytes = File.ReadAllBytes(openFileDialog.FileName);
            }

            // Wyznaczenie wartości funkcji hashującej od załadowanej wiadomości
            messageHashBytes = new MD5CryptoServiceProvider().ComputeHash(messageBytes);
            BigInteger messageHashValue = new BigInteger(messageHashBytes.Concat(new byte[] { 0 }).ToArray());
            iMessageHashValue = messageHashValue;

            LoadDataButton.IsEnabled = false;
            insertedTextBox.Text = "Message was loaded from selected file";
            insertedTextBox.IsReadOnly = true;
            GenerateKeyButton.IsEnabled = true;

            LoadDataEventHandler(this, null);
        }

        private void LoadDataEventHandler(object sender, RoutedEventArgs e)
        {
            if (insertedTextBox.Text.Length != 0 || messageBytes != null)
            {
                if (messageBytes == null)
                    messageBytes = Encoding.Unicode.GetBytes(insertedTextBox.Text);

                LoadDataButton.IsEnabled = false;
                insertedTextBox.IsReadOnly = true;
                GenerateKeyButton.IsEnabled = true;
                messageSizeTextBox.Text = "Loaded message was consisted of " + messageBytes.Length + " bytes";
            }
            else
                messageSizeTextBox.Text = "There was problem during reading message";

        }

        private void GenerateKeysEventHandler(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            // Losowanie stosunkowo dużej liczby pierwszej w zakresie od połowy do maksimum wartości typu int
            int randomValue = 0;
            do
            {
                randomValue = random.Next(int.MaxValue / 2, int.MaxValue);
                bool isPrime = true;
                for (int i = 2; i <= Math.Sqrt(randomValue); i++)
                {

                    if (randomValue % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime )
                {
                    p = randomValue;
                    break;
                }
            } while (true);

            // Kolejno wyznaczane są wartości g,x,y
            // g to taka liczba, której potęgi użyte jako lewy argument operacji modulo, dają wszelkie możliwe reszty
            g = FindPrimitiveRoot((int)(p));
            // x dowolna liczba całkowita większa niż 1, a mniejsza niż p - 1
            x = random.Next(2, (int)(p - 1));
            // y to wynik operacji g^x mod p
            y = BigInteger.ModPow(g, x, p);

            keyTextBox.Text = "Public key 'y' value: \n" + y.ToString() +
                              "\nPrivate key 'x' value: \n" + x.ToString() +
                              "\nGenerator g of the multiplicative group of integers modulo p:\n" +
                              g.ToString() +
                              "\nRandomly chosen prime p:\n" +
                              p.ToString();
            EncodeButton.IsEnabled = true;
        }

        private void SignMessageEventHandler(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            
            // Wyznaczenie k
            Int64 k = 0;
            do
            {
                k = random.Next(3, (int)(p - 2));
            } while (BigInteger.GreatestCommonDivisor((p - 1), k) != 1 );

            // Wyznaczenie r
            r = BigInteger.ModPow(g, k, p);

            ////Wyznaczenie wartości funkcji hashującej od załadowanej wiadomości
            messageHashBytes = new MD5CryptoServiceProvider().ComputeHash(messageBytes);
            BigInteger messageHashValue = new BigInteger(messageHashBytes.Concat(new byte[] { 0 }).ToArray());
            iMessageHashValue = messageHashValue;

            // Wyznaczenie wartości podpisu dla załadowanej wartości
            BigInteger xr = BigInteger.Multiply(x, r);
            BigInteger reversedMod = new BigInteger(InvertedModulo(k, (int)(p - 1)));
            BigInteger moduloLeftArg = (iMessageHashValue - xr) * reversedMod;
            iSignature = BigInteger.ModPow(moduloLeftArg, 1, p - 1);

            signatureCreationTextBox.Text = "Message is now signed with pair of numbers (r,s)\nr = " +  r.ToString() + "\ns = " + iSignature.ToString();
            ReloadSignature.IsEnabled = true;
            DecodeButton.IsEnabled = true;

            using (StreamWriter sw = new StreamWriter("sr.txt"))
            {
                sw.WriteLine(r);
                sw.WriteLine(iSignature);
                sw.WriteLine(iMessageHashValue);
            }
        }

        private void ReloadSignatureEventHandler(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new StreamReader("sr.txt"))
            {
                r = BigInteger.Parse(sr.ReadLine());
                iSignature = BigInteger.Parse(sr.ReadLine());
                iMessageHashValue = BigInteger.Parse(sr.ReadLine());
            }
            signatureCreationTextBox.Text = "Message is now signed with pair of numbers (r,s)\nr = " + r.ToString() + "\ns = " + iSignature.ToString();
            DecodeButton.IsEnabled = true;
        }

        private void VerifySignatureEventHandler(object sender, RoutedEventArgs e)
        {
            string firstCondtion = "0 < " + r.ToString() + " < " + p.ToString() + (0 < r && r < p ? " => true\n" : " => false\n");
            string secondCondtion = "0 < " + iSignature.ToString() + " < " + (p - 1).ToString() + (0 < iSignature && iSignature < (p -1) ? " => true" : " => false");
            verificationStep1TextBox.Text = firstCondtion + secondCondtion;

            
            BigInteger lSide = BigInteger.ModPow(g, iMessageHashValue, p);
            BigInteger first = BigInteger.ModPow(y, r, p);
            BigInteger second = BigInteger.ModPow(r, iSignature, p);
            BigInteger rSide = BigInteger.ModPow((BigInteger.Multiply(first, second)), 1, p);

            string sLSide = "g^(H(m)) mod p = " + lSide.ToString();
            string sRSide = "y^r * r^s  mod p = " + rSide.ToString();

            verificationStep2TextBox.Text = sLSide + "\n" + sRSide + "\n";
            verificationStep2TextBox.Text += lSide.ToString() + " == " + rSide.ToString() + (lSide.Equals(rSide) ? " => true" : " => false");
        }

        #region MathsMethods
        private static Int64 InvertedModulo(Int64 leftValue, Int64 rValue)
        {
            Int64 m0 = rValue;
            Int64 y = 0, x = 1;

            if (rValue == 1)
            {
                return 0;
            }

            while (leftValue > 1)
            {
                Int64 q = leftValue / rValue;
                Int64 t = rValue;
                rValue = leftValue % rValue;
                leftValue = t;
                t = y;
                y = x - q * y;
                x = t;
            }

            if (x < 0)
            {
                x += m0;
            }

            return x;
        }


        private static Int32 ModExp(Int32 aBase, Int32 aExponent, Int32 aModulo)
        {
            return (Int32)Math.Pow(aBase, aExponent) % aModulo;
        }

        private static Int64 FindPrimitiveRoot(Int32 aPrime)
        {
            Random random = new Random();
            if (aPrime == 2)
            {
                return 1;
            }
            Int32 p1 = 2;
            Int32 p2 = p1 - 1;
            while (true)
            {
                int i = random.Next(2, aPrime);
                if (ModExp(i, (aPrime - 1) / p1, aPrime) != 1)
                {
                    if (ModExp(i, (aPrime - 1) / p2, aPrime) != 1)
                    {
                        return i;
                    }
                }
            }
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
        #endregion


        private void ResetEventHandler(object sender, RoutedEventArgs e)
        {
            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;
            LoadDataFromFileButton.IsEnabled = true;
            insertedTextBox.IsReadOnly = false;
            GenerateKeyButton.IsEnabled = false;
            ReloadSignature.IsEnabled = false;


            signatureCreationTextBox.Clear();
            verificationStep2TextBox.Clear();
            messageSizeTextBox.Clear();
            insertedTextBox.Clear();
            keyTextBox.Clear();
            verificationStep1TextBox.Clear();


            messageBytes = null;
            p = 0;
            g = 0;
            y = 0;
            x = 0;
            r = 0;
        }
    }
}