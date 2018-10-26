using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace Kryptografia_OTP_Paweł_Ciupka_Dawid_Gierowski_Marcin_Kwapisz
{
    public partial class MainWindow : Window
    {
        private List<UInt16> iMessage;
        private UInt16[] iKey;
        private UInt16[] iCryptogram;
        private UInt16[] iDecoded;
        private byte[] iAllBytes;
        private bool isFileUsed = false;


        public MainWindow()
        {
            iMessage = new List<UInt16>();
            InitializeComponent();
            DecodeButton.IsEnabled = false;
            EncodeButton.IsEnabled = false;
            GenerateKeyButton.IsEnabled = false;
        }


        private void ConvertFromFileEventHandler(object sender, RoutedEventArgs e)
        {
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
                        iMessage.Add((UInt16)((words[0] << 8) | words[1]));
                        Console.Write("kek");
                    }
                    else
                    {
                        iMessage.Add(words[0]);
                    }

                }
            }

            iAllBytes = allBytes;
            isFileUsed = true;
            displayBinaryTextBox.Text = ConvertDecStringToBinString(iMessage.ToArray());
            ConvertToBinButton.IsEnabled = false;
            ConvertToBinFromFileButton.IsEnabled = false;
            insertedTextBox.IsReadOnly = true;
            GenerateKeyButton.IsEnabled = true;
        }
        private void ConvertToBinEventHandler(object sender, RoutedEventArgs e)
        {
            if (insertedTextBox.Text.Length != 0)
            {
                for (UInt16 i = 0; i < insertedTextBox.Text.Length; i++)
                {
                    iMessage.Add((UInt16)insertedTextBox.Text[i]);
                }

                displayBinaryTextBox.Text = ConvertDecStringToBinString(iMessage.ToArray());
                ConvertToBinButton.IsEnabled = false;
                ConvertToBinFromFileButton.IsEnabled = false;
                insertedTextBox.IsReadOnly = true;
                GenerateKeyButton.IsEnabled = true;
            }
        }

        private void GenerateKeyEventHandler(object sender, RoutedEventArgs e)
        {
            Random randomGenerator = new Random();
            iKey = new UInt16[iMessage.Count];
            for (int i = 0; i < iKey.Length; i++)
            {
                iKey[i] = (UInt16)randomGenerator.Next(UInt16.MaxValue);
            }

            keyTextBox.Text = ConvertDecStringToBinString(iKey);
            EncodeButton.IsEnabled = true;
        }

        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iCryptogram = (UInt16[])iMessage.ToArray().Clone();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iCryptogram[i] ^= iKey[i];
            }

            cryptogramTextBox.Text = ConvertDecStringToBinString(iCryptogram);
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iDecoded = (UInt16[])iCryptogram.Clone();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iDecoded[i] ^= iKey[i];
            }

            decodedBinTextBox.Text = ConvertDecStringToBinString(iDecoded);

            
            StringBuilder stringBuilder = new StringBuilder();
            foreach (UInt16 value in iDecoded)
            {
                stringBuilder.Append((char)value);
            }

            if (!isFileUsed)
            {
                decodedTextBox.Text = stringBuilder.ToString();
            }
            else
            {
                using (FileStream fs = new FileStream("outputFile.jpg", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        for (int i = 0; i < iDecoded.Length; i++)
                        {
                            byte[] bytes = BitConverter.GetBytes(iDecoded[i]);
                            if (iAllBytes.Length % 2 == 1 && i == iDecoded.Length - 1)
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
            decodedTextBox.Clear();
            displayBinaryTextBox.Clear();
            insertedTextBox.Clear();
            keyTextBox.Clear();
            decodedBinTextBox.Clear();
        }

        public string ConvertDecStringToBinString(UInt16[] aints)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < aints.Length; i++)
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
