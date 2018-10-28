using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace Kryptografia_Rabin_Cryptosystem_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{
    public partial class MainWindow : Window
    {
        private List<UInt16> iMessage;
        private List<SByte> iMetaInfo;
        private bool isFileUsed = false;
        private List<char> iFileChars;
        private byte[] iAllBytes;
        private Int32[] iCryptogram;

        private List<Int32>[] iDecodedVariants;

        private UInt16[] iResult;

        private Int64 privateKey1;
        private Int64 privateKey2;
        private Int64 publicKey;

        private Int32 var1;
        private Int32 var2;
        private Int32 var3;
        private Int32 var4;

        public MainWindow()
        {
            InitializeComponent();

            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;

            iDecodedVariants = new List<Int32>[4];
            iDecodedVariants[0] = new List<Int32>();
            iDecodedVariants[1] = new List<Int32>();
            iDecodedVariants[2] = new List<Int32>();
            iDecodedVariants[3] = new List<Int32>();

            iFileChars = new List<char>();
            iMessage = new List<UInt16>();
            iMetaInfo = new List<SByte>();
        }
        private void ConvertFromFileEventHandler(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            OpenFileDialog openFileDialog = new OpenFileDialog();

            byte[] allBytes = null;
            if (openFileDialog.ShowDialog() == true)
            {
                allBytes = File.ReadAllBytes(openFileDialog.FileName);
            }

            if (allBytes != null)
            {
                for (int i = 0; i < allBytes.Length; i += 2)
                {
                    List<byte> words = new List<byte>
                    {
                        (byte)allBytes[i]
                    };

                    if (i + 1 < allBytes.Length)
                    {
                        words.Add((byte)allBytes[i + 1]);
                    }
                    if (words.Count == 2)
                    {
                        iFileChars.Add((char)((words[0] << 8) | words[1]));
                    }
                    else
                    {
                        iFileChars.Add((char)words[0]);
                    }

                }
            }

            iAllBytes = allBytes;
            isFileUsed = true;

            ConvertToBinButton.IsEnabled = false;
            ConvertToBinFromFileButton.IsEnabled = false;
            insertedTextBox.IsReadOnly = true;
            GenerateKeyButton.IsEnabled = true;

            ConvertToBinEventHandler(sender, e);
        }

        private void ConvertToBinEventHandler(object sender, RoutedEventArgs e)
        {

            if (insertedTextBox.Text.Length != 0 || ConvertToBinButton.IsEnabled == false)
            {
                char[] letters;
                if (ConvertToBinButton.IsEnabled == false)
                {
                    letters = iFileChars.ToArray();
                }
                else
                {
                    letters = insertedTextBox.Text.ToCharArray();
                }


                StringBuilder stringBuilder = new StringBuilder();

                for (Int32 i = 0; i < letters.Length; i++)
                {
                    stringBuilder.Append((UInt32)letters[i]);
                    stringBuilder.Append(" -> ");
                    int numOfSignificantBits;
                    byte[] bytes = BitConverter.GetBytes(letters[i]);
                    string binaryString = ConvertIntToBinaryString(bytes[1], 8) + ConvertIntToBinaryString(bytes[0], 8);
                    string result = "";
                    if (binaryString.Equals("0000000000000000"))
                    {
                        numOfSignificantBits = 0;
                        iMetaInfo.Add(-10);
                        result = "0000000000000000";
                    }
                    else
                    {
                        numOfSignificantBits = 16 - binaryString.IndexOf('1');

                        if (numOfSignificantBits <= 8)
                        {
                            iMetaInfo.Add(-1);
                            result = binaryString.TrimStart('0');
                            result = (result + result).PadLeft(16, '0');
                        }

                        else
                        {
                            result = binaryString.TrimStart('0');
                            string copy = (string)result.Clone();
                            for (int j = 0; j < binaryString.Length - numOfSignificantBits; j++)
                            {
                                result = result.Insert(0, binaryString[binaryString.Length - 1 - j].ToString());
                            }
                            if(result.Substring(0, result.Length - copy.Length).IndexOf('1') == -1)
                            {
                                iMetaInfo.Add(0);
                            }
                            else
                            {
                                iMetaInfo.Add((SByte)(binaryString.Length - numOfSignificantBits));
                            }
                        }
                    }
                    iMessage.Add(Convert.ToUInt16(result, 2));
                    stringBuilder.Append(iMessage[i] + "\n");

                }
                displayBinaryTextBox.Text = stringBuilder.ToString();

                ConvertToBinButton.IsEnabled = false;
                insertedTextBox.IsReadOnly = true;
                GenerateKeyButton.IsEnabled = true;
            }
        }

        private void GenerateKeyEventHandler(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            List<Int16> primes = new List<Int16>();

            do
            {
                Int16 randomValue = (Int16)random.Next(Int16.MaxValue);
                bool isPrime = true;
                for (int i = 2; i <= Math.Sqrt(randomValue); i++)
                {

                    if (randomValue % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime && !primes.Contains(randomValue) && FastModulo(randomValue, 1, 4) == 3)
                {
                    primes.Add(randomValue);
                }
            } while (primes.Count < 2);

            privateKey1 = primes[0];
            privateKey2 = primes[1];
            publicKey = privateKey1 * privateKey2;

            keyTextBox.Text = "Private key:\np = " + privateKey1.ToString() + "\nq = " + privateKey2.ToString() + "\nPublic key = " + publicKey.ToString();
            EncodeButton.IsEnabled = true;
        }

        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            iCryptogram = new Int32[iMessage.Count];
            for (Int32 i = 0; i < iMessage.Count; i++)
            {
                iCryptogram[i] = (Int32)FastModulo(iMessage[i], 2, publicKey);
                stringBuilder.Append(iCryptogram[i] + "\n");
            }

            cryptogramTextBox.Text = stringBuilder.ToString();
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iResult = new UInt16[iCryptogram.Length];

            Int64 a = 0, b = 0;
            ComputeFactors(privateKey1, privateKey2, ref a, ref b);

            StringBuilder stringBuilder = new StringBuilder();

            for (Int32 i = 0; i < iCryptogram.Length; i++)
            {
                Int32 messageRootOfPrivateKey1 = (Int32)FastModulo(iCryptogram[i], (privateKey1 + 1) / 4, privateKey1);
                Int32 messageRootOfPrivateKey2 = (Int32)FastModulo(iCryptogram[i], (privateKey2 + 1) / 4, privateKey2);

                Int32 moduledFirstPart = (Int32)FastModulo(FastModulo(a, 1, publicKey) * FastModulo(privateKey1, 1, publicKey) * FastModulo(messageRootOfPrivateKey2, 1, publicKey), 1, publicKey);
                Int32 moduledSecondPart = (Int32)FastModulo(FastModulo(b, 1, publicKey) * FastModulo(privateKey2, 1, publicKey) * FastModulo(messageRootOfPrivateKey1, 1, publicKey), 1, publicKey);

                Int32 r = (Int32)FastModulo(moduledFirstPart + moduledSecondPart, 1, publicKey);
                Int32 s = (Int32)FastModulo(moduledFirstPart - moduledSecondPart, 1, publicKey);

                iDecodedVariants[0].Add((Int32)r);
                iDecodedVariants[1].Add((Int32)publicKey - r);
                iDecodedVariants[2].Add((Int32)s);
                iDecodedVariants[3].Add((Int32)publicKey - s);


                stringBuilder.Append("Decrypted portions of 16 bits - nr [" + i + "] - possible outputs: \n");
                List<string> decodedStrings = new List<string>();

                for (int j = 0; j < iDecodedVariants.Length; j++)
                {
                    stringBuilder.Append(iDecodedVariants[j][i] + "\n");
                    decodedStrings.Add(Convert.ToString(iDecodedVariants[j][i], 2).PadLeft(16, '0'));
                }

                if (iMetaInfo[i] == -1)
                {
                    for (int j = 0; j < decodedStrings.Count; j++)
                    {
                        string tmp = decodedStrings[j].TrimStart('0');
                        string leftPart = tmp.Substring(0, tmp.Length / 2);
                        string rightPart = tmp.Substring(tmp.Length / 2, tmp.Length / 2);
                        if (leftPart.Equals(rightPart))
                        {
                            iResult[i] = Convert.ToUInt16(leftPart, 2);
                        }
                    }
                }
                else if (iMetaInfo[i] == -10)
                {
                    iResult[i] = Convert.ToUInt16("0000000000000000", 2);
                }
                else if(iMetaInfo[i] == 0)
                {
                    for (int j = 0; j < decodedStrings.Count; j++)
                    {
                        if(decodedStrings[j].Length <= 16)
                        {
                            iResult[i] = Convert.ToUInt16(decodedStrings[j], 2);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < decodedStrings.Count; j++)
                    {
                        string coppiedBits = decodedStrings[j].Substring(0, iMetaInfo[i]);
                        string toCompare = decodedStrings[j].Substring(decodedStrings[j].Length - iMetaInfo[i], iMetaInfo[i]);
                        if (toCompare.Equals(coppiedBits))
                        {
                            string actualValue = decodedStrings[j].Substring(iMetaInfo[i]);
                            if (actualValue.Length <= 16)
                            {
                                iResult[i] = Convert.ToUInt16(actualValue, 2);
                            }
                        }
                    }
                }

                if(iResult[i] != (UInt32)iFileChars[i])
                {
                    var meta = iMetaInfo[i];
                    var mess = iMessage[i];
                    var chr = iFileChars[i];
                    var res = iResult[i];
                    var1 = r;
                    var2 = (Int32)publicKey - r;
                    var3 = s;
                    var4 = (Int32)publicKey - s;
                    Console.Write("crap");
                }
            }

            #region OutputProduction
            if (!isFileUsed)
            {
                decodedBinTextBox.Text = stringBuilder.ToString();

                StringBuilder chosenResult = new StringBuilder();

                for (Int32 i = 0; i < iResult.Length; i++)
                {
                    chosenResult.Append((char)iResult[i]);
                }


                decodedTextBox.Text = chosenResult.ToString();
            }
            else
            {
                using (FileStream fs = new FileStream("outputFile.jpg", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        for (int i = 0; i < iResult.Length; i++)
                        {
                            byte[] bytes = BitConverter.GetBytes(iResult[i]);
                            byte[] source = BitConverter.GetBytes(iFileChars[i]);
                            if(bytes[0] !=  source[0] || bytes[1] != source[1])
                            {
                                var it = i;
                                var meta = iMetaInfo[i];
                                var mess = iMessage[i];
                                var chr = iFileChars[i];
                                var res = iResult[i];
                                Console.WriteLine("kek");
                            }
                            if (iAllBytes.Length % 2 == 1 && i == iResult.Length - 1)
                            {
                                bw.Write(bytes[0]);
                            }
                            else
                            {
                                bw.Write(bytes[1]);
                                bw.Write(bytes[0]);
                            }

                        }
                    }
                }
                decodedTextBox.Text = "Decoded message saved to file 'output.jpg'";
            }

            #endregion
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

            iFileChars.Clear();
            iMessage.Clear();
            iMetaInfo.Clear();

            iCryptogram = null;

            for (int i = 0; i < iDecodedVariants.Length; i++)
            {
                iDecodedVariants[i].Clear();
            }

            privateKey1 = 0;
            privateKey2 = 0;
            publicKey = 0;
            isFileUsed = false;
            ConvertToBinFromFileButton.IsEnabled = true;
        }

        public string ConvertIntToBinaryString(Int32 value, int aLenght)
        {
            string tmp = Convert.ToString(value, 2);
            if (tmp.Length < aLenght)
            {
                tmp = (tmp.PadLeft(aLenght, '0'));
            }
            return tmp;
        }

        #region MathsMethods
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


        public Int64 ComputeFactors(Int64 aP, Int64 aQ, ref Int64 aA, ref Int64 aB)
        {
            if (aP == 0)
            {
                aA = 0;
                aB = 1;
                return aQ;
            }

            Int64 x1 = 1, y1 = 1;
            Int64 gcd = ComputeFactors(aQ % aP, aP, ref x1, ref y1);

            aA = y1 - (aQ / aP) * x1;
            aB = x1;

            return gcd;
        }
        #endregion
    }
}