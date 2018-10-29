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
        private List<Int16> iMetaInfo;
        private List<UInt16> iFileData;
        private List<Int32>[] iDecodedVariants;

        private Int32[] iCryptogram;
        private UInt16[] iResult;
        
        private bool isFileUsed = false;
        private Int64 privateKey1;
        private Int64 privateKey2;
        private Int64 publicKey;

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

            iFileData = new List<UInt16>();
            iMessage = new List<UInt16>();
            iMetaInfo = new List<Int16>();
        }

        #region FileLoading
        private void FileDataEventHandler(object sender, RoutedEventArgs e)
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
                if (allBytes.Length % 2 == 0)
                {
                    for (int i = 0; i < allBytes.Length; i += 2)
                    {
                        iFileData.Add(BitConverter.ToUInt16(new byte[] { allBytes[i], allBytes[i + 1] }, 0));
                    }
                }
                else
                {
                    for (int i = 0; i < allBytes.Length; i += 2)
                    {
                        if (i < allBytes.Length - 2)
                        {
                            iFileData.Add(BitConverter.ToUInt16(new byte[] { allBytes[i], allBytes[i + 1] }, 0));
                        }
                        else
                        {
                            iFileData.Add((UInt16)(allBytes[i]));
                        }
                    }
                }
            }

            isFileUsed = true;
            ConvertToBinButton.IsEnabled = false;
            ConvertToBinFromFileButton.IsEnabled = false;
            insertedTextBox.IsReadOnly = true;
            GenerateKeyButton.IsEnabled = true;

            LoadDataEventHandler(sender, e);
        }
        #endregion

        private void LoadDataEventHandler(object sender, RoutedEventArgs e)
        {

            if (insertedTextBox.Text.Length != 0 || ConvertToBinButton.IsEnabled == false)
            {

                if(isFileUsed == false)
                {
                    iFileData.Clear();
                    for(int i = 0; i < insertedTextBox.Text.Length; i++)
                    {
                        iFileData.Add((UInt16)insertedTextBox.Text[i]);
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();

                for (Int32 i = 0; i < iFileData.Count; i++)
                {
                    stringBuilder.Append(iFileData[i]);
                    stringBuilder.Append(" -> ");

                    int numOfSignificantBits;

                    byte[] bytes = BitConverter.GetBytes(iFileData[i]);

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
                            Int16 lengthDiffrence = (Int16)(binaryString.Length - result.Length);
                            Int16 info = (Int16)(1000 * lengthDiffrence);
                            for (int j = 0; j < binaryString.Length - numOfSignificantBits; j++)
                            {
                                result = result.Insert(0, binaryString[binaryString.Length - 1 - j].ToString());
                            }
                            string nextByteString = result.Substring(lengthDiffrence, 8);
                            info += Convert.ToByte(nextByteString, 2);
                            iMetaInfo.Add(info);
                        }
                    }
                    iMessage.Add(Convert.ToUInt16(result, 2));
                    stringBuilder.Append(iMessage[i] + "\n");

                }
                ConvertToBinFromFileButton.IsEnabled = false;
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
            for (int i = 0; i < iDecodedVariants.Length; i++)
            {
                iDecodedVariants[i].Clear();
            }

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
                    string parsed = Convert.ToString(iDecodedVariants[j][i], 2).PadLeft(16, '0');
                    if (parsed.Length <= 16)
                    {
                        decodedStrings.Add(parsed);
                    }
                }
                if(decodedStrings.Count == 0)
                {
                    iResult[i] = (UInt16)r;
                }
                else if (iMetaInfo[i] == -1)
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
                else
                {
                    for (int j = 0; j < decodedStrings.Count; j++)
                    {
                        int numberOfCoppiedBits = iMetaInfo[i] / 1000;
                        string coppiedBits = decodedStrings[j].Substring(0, numberOfCoppiedBits);
                        string toCompare = decodedStrings[j].Substring(decodedStrings[j].Length - numberOfCoppiedBits, numberOfCoppiedBits);
                        if (toCompare.Equals(coppiedBits))
                        {
                            string byteToCheck = decodedStrings[j].Substring(numberOfCoppiedBits, 8);
                            if (iMetaInfo[i] % 1000 == Convert.ToByte(byteToCheck, 2))
                            {
                                string actualValue = decodedStrings[j].Substring(numberOfCoppiedBits);
                                iResult[i] = Convert.ToUInt16(actualValue, 2);
                            }
                        }
                    }
                }
            }

            #region OutputProduction
            if (!isFileUsed)
            {
                decryptionVariantsTextBox.Text = stringBuilder.ToString();

                StringBuilder chosenResult = new StringBuilder();

                for (Int32 i = 0; i < iResult.Length; i++)
                {
                    chosenResult.Append((char)iResult[i]);
                }


                chosenDecryptionTextBox.Text = chosenResult.ToString();
                decryptionVariantsTextBox.Text = stringBuilder.ToString();
            }
            else
            {
                using (FileStream fs = new FileStream("outputFile.jpg", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        for (int i = 0; i < iResult.Length; i++)
                        {
                            bw.Write(iResult[i]);
                        }
                    }
                }
                decryptionVariantsTextBox.Text = stringBuilder.ToString();
                chosenDecryptionTextBox.Text = "Decoded message saved to file 'output.jpg'";
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
            ConvertToBinFromFileButton.IsEnabled = true;

            cryptogramTextBox.Clear();
            chosenDecryptionTextBox.Clear();
            displayBinaryTextBox.Clear();
            insertedTextBox.Clear();
            keyTextBox.Clear();
            decryptionVariantsTextBox.Clear();

            iFileData.Clear();
            iMessage.Clear();
            iMetaInfo.Clear();

            iCryptogram = null;

            for (int i = 0; i < iDecodedVariants.Length; i++)
            {
                iDecodedVariants[i].Clear();
            }

            isFileUsed = false;
            iResult = null;
            privateKey1 = 0;
            privateKey2 = 0;
            publicKey = 0;
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