using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dallas_Touch_Memory_Verifyer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private string _dallaskeyNumber = string.Empty;
        public string DallasKeyNumber
        {
            get { return _dallaskeyNumber; }
            set
            {
                _dallaskeyNumber = RemoveNonHEXsymbols(value);
                DallasKeyNumberChanged(_dallaskeyNumber, DallasKeyCrc);
                OnPropertyChanged(nameof(DallasKeyNumber));
            }
        }

        private string InsertSpacesToHexString(string dallaskeyNumber)
        {
            var byteArr = StringToByteArray(dallaskeyNumber);

            return BitConverter.ToString(byteArr).Replace("-", " ");
        }

        private string _dallasKeyCrc = string.Empty;
        public string DallasKeyCrc
        {
            get { return _dallasKeyCrc; }
            set
            {

                _dallasKeyCrc = RemoveNonHEXsymbols(value);
                DallasKeyNumberChanged(DallasKeyNumber, _dallasKeyCrc);
                OnPropertyChanged(nameof(DallasKeyCrc));
            }
        }

        private string _reversedKey = string.Empty;
        public string ReversedKey
        {
            get { return _reversedKey; }
            set
            {
                _reversedKey = value;
                OnPropertyChanged(nameof(ReversedKey));
            }
        }


        private string RemoveNonHEXsymbols(string value)
        {
            string goodChars = string.Empty;
            foreach (char item in value)
            {
                if (IsTextAllowed(item.ToString(), @"[^0-9aAbBcCdDeEfF]") || item == ' ')
                {
                    goodChars += item;
                }
            }
            return goodChars;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        private void DallasKeyNumberChanged(string keyValue, string dallasKeyCrc)
        {

            string keyWithoutSpaces = keyValue.Replace(" ", string.Empty);
            byte[] keyInBytes = StringToByteArray(keyWithoutSpaces);

            string keyCRCinputWithoutSpaces = dallasKeyCrc.Replace(" ", string.Empty);
            byte[] keyCRCByteInput = StringToByteArray(keyCRCinputWithoutSpaces);


            if (keyInBytes.Count() < 7)
            {
                TextBoxKeyCRC.Background = Brushes.Red;
                ReversedKey = string.Empty;
                return;
            }


            if (keyInBytes.Count() == 8)
            {
                var firstPart = new byte[] { keyInBytes[0] };
                _dallasKeyCrc = BitConverter.ToString(firstPart).Replace("-", "");

                var secondPart = keyInBytes.Skip(1).ToArray();
                _dallaskeyNumber = BitConverter.ToString(secondPart).Replace("-", "");
                OnPropertyChanged(nameof(DallasKeyCrc));
                OnPropertyChanged(nameof(DallasKeyNumber));
                DallasKeyNumberChanged(DallasKeyNumber, DallasKeyCrc);
                return;
            }


            if (keyInBytes.Count() != 7 || keyCRCByteInput.Count() != 1)
            {
                TextBoxKeyCRC.Background = Brushes.Red;
                ReversedKey = string.Empty;
                return;
            }


            keyInBytes = keyInBytes.Reverse().ToArray();
            byte keyCRCcalculated = iButtonCRC(keyInBytes);

            if (keyCRCByteInput[0] == keyCRCcalculated)
            {
                byte[] fullKey = keyInBytes.Append(keyCRCByteInput[0]).ToArray();
                ReversedKey = BitConverter.ToString(fullKey).Replace("-", "");
                TextBoxKeyCRC.Background = Brushes.Green;
                return;
            }
            else
            {
                TextBoxKeyCRC.Background = Brushes.Red;
                ReversedKey = string.Empty;
                return;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            string keyWithoutSpaces = hex.Replace(" ", string.Empty);


            if (keyWithoutSpaces.Length < 2)
                return new byte[0];

            if (keyWithoutSpaces.Length % 2 != 0)
                keyWithoutSpaces = keyWithoutSpaces.Remove(keyWithoutSpaces.Length - 1);

            return Enumerable.Range(0, keyWithoutSpaces.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(keyWithoutSpaces.Substring(x, 2), 16))
                             .ToArray();
        }

        byte[] crcTable = new byte[]{
                0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126, 32, 163, 253, 31, 65,
                157, 195, 33, 127, 252, 162, 64, 30, 95, 1, 227, 189, 62, 96, 130, 220,
                35, 125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3, 128, 222, 60, 98,
                190, 224, 2, 92, 223, 129, 99, 61, 124, 34, 192, 158, 29, 67, 161, 255,
                70, 24, 250, 164, 39, 121, 155, 197, 132, 218, 56, 102, 229, 187, 89, 7,
                219, 133, 103, 57, 186, 228, 6, 88, 25, 71, 165, 251, 120, 38, 196, 154,
                101, 59, 217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198, 152, 122, 36,
                248, 166, 68, 26, 153, 199, 37, 123, 58, 100, 134, 216, 91, 5, 231, 185,
                140, 210, 48, 110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113, 147, 205,
                17, 79, 173, 243, 112, 46, 204, 146, 211, 141, 111, 49, 178, 236, 14, 80,
                175, 241, 19, 77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82, 176, 238,
                50, 108, 142, 208, 83, 13, 239, 177, 240, 174, 76, 18, 145, 207, 45, 115,
                202, 148, 118, 40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55, 213, 139,
                87, 9, 235, 181, 54, 104, 138, 212, 149, 203, 41, 119, 244, 170, 72, 22,
                233, 183, 85, 11, 136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246, 168,
                116, 42, 200, 150, 21, 75, 169, 247, 182, 232, 10, 84, 215, 137, 107, 53
        };

        byte iButtonCRC(byte[] iButtonData)
        {
            byte i, crc, tmp;
            for (i = 0, crc = 0; i < 7; i++)
            {
                tmp = (byte)(crc ^ iButtonData[i]);
                crc = crcTable[tmp];
            }

            return crc;
        }


        private void TextBoxKeyNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text, @"[^0-9aAbBcCdDeEfF]");
        }
        private void TextBoxKeyCRC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text, @"[^0-9aAbBcCdDeEfF]");
        }

        private bool IsTextAllowed(string Text, string AllowedRegex)
        {
            try
            {
                var regex = new Regex(AllowedRegex);
                return !regex.IsMatch(Text);
            }
            catch
            {
                return true;
            }
        }




    }


}
