using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using SnappyWinscard;
using System.Windows.Data;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Controls;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DeKast
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public bool connActive = false;
        public byte[] SendBuff = new byte[263];
        public byte[] RecvBuff = new byte[263];
        public int SendLen, RecvLen, nBytesRet, reqType, Aprotocol, dwProtocol, cbPciLength;
        public Winscard.SCARD_READERSTATE RdrState;
        public Winscard.SCARD_IO_REQUEST pioSendRequest;
        private readonly object lock1 = new object();
        public CardIo CardIo { get; set; }

        public string Status { get; set; }

        public MainWindowViewModel() 
        {
            CardIo = new CardIo();
            BindingOperations.EnableCollectionSynchronization(CardIo.Devices, lock1);
            CardIo.ReaderStateChanged += CardIo_ReaderStateChanged;
            var rest = CardIo.ConnectCard();

            Status = "";
        }

        private async void CardIo_ReaderStateChanged(CardIo.ReaderState readerState)
        {
            var rest = CardIo.ConnectCard();


            if (readerState == CardIo.ReaderState.CardReady)
            {

                var test = CardIo.ReadCardBlock(byte.Parse("1"), (byte)0x60, (byte)0x0);
                var id = int.Parse(UTF8Encoding.UTF8.GetString(test).Split("/").ElementAt(0));

                Console.WriteLine(id);

                var client = new HttpClient();

                var result = await client.PutAsync($"https://localhost:44357/CheckIn?AbonnementId={id}", null);
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(await result.Content.ReadAsStringAsync());
                Status = obj.status;
                NotifyPropertyChanged(nameof(Status));
            }
        }

        //private byte[] GetBytes()
        //{
        //    var text = "2";
        //    if (text.Length < 16)
        //    {
        //        text += new string('\0', 16 - text.Length);
        //    }
        //    else if (text.Length > 16)
        //    {
        //        text = text.Substring(0, 16);
        //    }
        //    return text
        //        .ToCharArray()
        //        .Select(c => (byte)c)
        //        .ToArray();
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }



    }

}
