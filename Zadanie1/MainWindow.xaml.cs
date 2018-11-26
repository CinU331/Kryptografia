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

            StringBuilder stringBuilder = new StringBuilder();

            if (allBytes != null)
            {
                if (allBytes.Length % 2 == 0)
                {
                    for (int i = 0; i < allBytes.Length; i += 2)
                    {
                        iMessage.Add(BitConverter.ToUInt16(new byte[] { allBytes[i], allBytes[i + 1] }, 0));
                    }
                }
                else
                {
                    for (int i = 0; i < allBytes.Length; i += 2)
                    {
                        if (i < allBytes.Length - 2)
                        {
                            iMessage.Add(BitConverter.ToUInt16(new byte[] { allBytes[i], allBytes[i + 1] }, 0));
                        }
                        else
                        {
                            iMessage.Add((UInt16)(allBytes[i]));
                        }
                    }
                }
            }

            for (int i = 0; i < iMessage.Count; i++)
            {
                stringBuilder.Append(Convert.ToString(iMessage[i], 2).PadLeft(16, '0'));
                stringBuilder.Append("\n");
            }

            isFileUsed = true;
            displayBinaryTextBox.Text = stringBuilder.ToString();
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

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < iMessage.Count; i++)
                {
                    stringBuilder.Append(Convert.ToString(iMessage[i], 2).PadLeft(16, '0'));
                    stringBuilder.Append("\n");
                }

                displayBinaryTextBox.Text = stringBuilder.ToString();
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

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < iKey.Length; i++)
            {
                stringBuilder.Append(Convert.ToString(iKey[i], 2).PadLeft(16, '0'));
                stringBuilder.Append("\n");
            }


            using (FileStream fs = new FileStream("key", FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    for (int i = 0; i < iKey.Length; i++)
                    {
                        bw.Write(iKey[i]);
                    }
                }
            }

            keyTextBox.Text = stringBuilder.ToString();
            EncodeButton.IsEnabled = true;
        }

        private void LoadKeyEventHandler(object sender, RoutedEventArgs e)
        {
            iKey = new UInt16[iMessage.Count];
            OpenFileDialog openFileDialog = new OpenFileDialog();

            byte[] allBytes = null;

            if (openFileDialog.ShowDialog() == true)
            {
                allBytes = File.ReadAllBytes(openFileDialog.FileName);
            }

            for (int i = 0; i < iMessage.Count; i += 2)
            {
                iKey[i] = BitConverter.ToUInt16(new byte[] { allBytes[i], allBytes[i + 1] }, 0);
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < iKey.Length; i++)
            {
                stringBuilder.Append(Convert.ToString(iKey[i], 2).PadLeft(16, '0'));
                stringBuilder.Append("\n");
            }
            keyTextBox.Text = stringBuilder.ToString();
            EncodeButton.IsEnabled = true;
        }
        private void EncodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iCryptogram = (UInt16[])iMessage.ToArray();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iCryptogram[i] ^= iKey[i];
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < iCryptogram.Length; i++)
            {
                stringBuilder.Append(Convert.ToString(iCryptogram[i], 2).PadLeft(16, '0'));
                stringBuilder.Append("\n");
            }

            cryptogramTextBox.Text = stringBuilder.ToString();
            DecodeButton.IsEnabled = true;
        }

        private void DecodeMessageEventHandler(object sender, RoutedEventArgs e)
        {
            iDecoded = (UInt16[])iCryptogram.Clone();

            for (int i = 0; i < iCryptogram.Length; i++)
            {
                iDecoded[i] ^= iKey[i];
            }


            if (isFileUsed)
            {
                using (FileStream fs = new FileStream("outputFile", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        for (int i = 0; i < iDecoded.Length; i++)
                        {
                            bw.Write(iDecoded[i]);
                        }
                    }
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder messageBuilder = new StringBuilder();
            for (int i = 0; i < iDecoded.Length; i++)
            {
                stringBuilder.Append(Convert.ToString(iDecoded[i], 2).PadLeft(16, '0'));
                messageBuilder.Append((char)iDecoded[i]);
                stringBuilder.Append("\n");
            }

            decodedBinTextBox.Text = stringBuilder.ToString();
            if (!isFileUsed)
            {
                decodedTextBox.Text = messageBuilder.ToString();
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

            iMessage.Clear();
            iKey = null;
            iCryptogram = null;
            iDecoded = null;
            isFileUsed = false;
        }
    }
}
