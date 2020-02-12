using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Net;
using System.Net.Sockets;
//using System.Windows.Forms;

namespace AppMultiServer
{
    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
    
    public partial class MainWindow : Window
    {
        string[] Acc = new string[100];
        string[] Pass = new string[100];
        int[]  Pos= new int[100];

        string[] Items_Name = new string[10000];
        string[] Items_Order = new string[10000];
        string[] Items_Drawing = new string[10000];
        string[] Items_Qty = new string[10000];
        string[] Items_High = new string[10000];
        string[] Items_Length = new string[10000];
        string[] Items_Weight = new string[10000];
        string[] Items_Total = new string[10000];

        string[] Items_User = new string[10000];
        int[] Items_Pos = new int[10000];
        string[] Items_Done = new string[10000];

        int DiaChi;
        String DuLieu;

        private const int PORT = 100;
        IPAddress address = IPAddress.Parse("10.12.20.77");
        String filePath;


        //

        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public MainWindow()
        {
            InitializeComponent();
            ScanData_User();
            ScanData_InforItems();

            // CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }
        void SetupServer()
        {
            lbStatus.Content += ("Setting up server...\r\n");
            _serverSocket.Bind(new IPEndPoint(address, PORT));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            lbStatus.Content += ("Server setup complete\r\n");

        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);

            __ClientSockets.Add(new SocketT2h(socket));

            var dispatcher = this.Dispatcher;
            // Or use this.Dispatcher if this method is in a window class.

            dispatcher.BeginInvoke(new Action(() =>
            {
                listbox_Client.Items.Add(socket.RemoteEndPoint.ToString());
                
                lbStatus.Content = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
            }));

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
               
        }

        private  void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(AR);
                }
                catch (Exception)
                {
                    // client đóng kết nối
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            
                            

                            var dispatcher = this.Dispatcher;
                            dispatcher.BeginInvoke(new Action(() =>
                            {
                                lbStatus.Content = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
                                listbox_Client.Items.RemoveAt(i-1);
                            }));
                            
                            __ClientSockets.RemoveAt(i);

                        }
                    }
                    // xóa trong list
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text2 = Encoding.ASCII.GetString(dataBuf);
                    string text = text2.Substring(2);

                    var dispatcher = this.Dispatcher;
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        string temp = socket.RemoteEndPoint.ToString() + "/" + text;
                        tbData.Text = temp;
                        XuLyText(temp);
                    }));

                    string reponse = string.Empty;

                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            dispatcher.BeginInvoke(new Action(() =>
                            {
                               //lbStatus2.Content = ("\n" + __ClientSockets[i]._Name + ": " + text);
                            }));
                            
                        }
                    }
                    if (text == "bye")
                    {
                        return;
                    }
                    reponse = text;
                    //Sendata(socket, reponse);
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            
                            
                            var dispatcher = this.Dispatcher;
                            dispatcher.BeginInvoke(new Action(() =>
                            {
                                lbStatus.Content = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
                                listbox_Client.Items.RemoveAt(i-1);
                            }));
                            __ClientSockets.RemoveAt(i);
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        void Sendata(Socket socket, string noidung)
        {
            byte[] data = Encoding.ASCII.GetBytes(noidung);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        void Save_Data(string strPath, String text)
        {
            FileStream Stream = new FileStream(strPath, FileMode.Append, FileAccess.Write);
            StreamWriter StreamWriter = new StreamWriter(Stream);
            StreamWriter.Write(text);
            StreamWriter.Close();
            Stream.Close();
        }

        void Send_Ard (IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            byte[] data = Encoding.ASCII.GetBytes("Khong hop le");
            current.Send(data);
        }

        private void BtStart_Click(object sender, RoutedEventArgs e)
        {
            SetupServer();
        }

        private void BtSend_Click(object sender, RoutedEventArgs e)
        {

            foreach(var item in listbox_Client.SelectedItems)
            {
                for(int i  = 0; i < listbox_Client.Items.Count; i ++)
                {
                    if(item.ToString() == listbox_Client.Items[i].ToString())
                    {
                        Sendata(__ClientSockets[i]._Socket, tbDataSend.Text + "\r\n");
                    }
                }
            }
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void BtExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        int Search_Index_Adress(string address_now)
        {
            for (int i = 0; i < listbox_Client.Items.Count; i++)
            {
                if (address_now == listbox_Client.Items[i].ToString())
                {
                    return i;
                }
            }
            return -1;
            
        }

        // Data
        void AutoSend(int Client_adress_index, String mess)
        {
            Sendata(__ClientSockets[Client_adress_index]._Socket, mess + "\r\n");
            tb_test.Text += mess + "\r\n";
        }

        void ScanData_User()
        {
            filePath = @"D:\Data\Acc_Pass_Pos\Acc_Pass_Pos.txt";
            string line;
            TextReader reader = new StreamReader(filePath);
            for (int m = 0; m < 100; m++)
            {
                if ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split('\t');
                    {
                        Acc[m] = a[0];
                        Pass[m] = a[1];
                        Pos[m] = Convert.ToInt16(a[2]);
                    }

                }
                else
                {
                    break;
                }
            }

        }

        void ScanData_InforItems()
        {
            filePath = @"D:\Data\Infor\DataItems.txt";
            string line;
            TextReader reader = new StreamReader(filePath);
            for (int m = 0; m < 10000; m++)
            {
                if ((line = reader.ReadLine()) != null)
                {
                    string[] aL = line.Split('\t');
                    {
                        Items_Name[m] = aL[0];
                        Items_Order[m] = aL[1];
                        Items_Drawing[m] = aL[2];
                        Items_Qty[m] = aL[3];
                        Items_High[m] = aL[4];
                        Items_Length[m] = aL[5];
                        Items_Weight[m] = aL[6];
                        Items_Total[m] = aL[7];

                    }

                }
                else
                {
                    break;
                }
            }
        }

        String Check_Login(string mess)
        {
            string[] a = mess.Split('|');
            if (a[0] != null && a[1] != null) 
            {
                for (int i = 0; i < 100; i++)
                {
                    if (a[0] == Acc[i]&&a[1] == Pass[i]+"'")
                    {
                        return "?";
                    }
                }
            }
            return "!";
        }

        int Check_Login_Pos(string mess)
        {
            string[] a = mess.Split('|');
            if (a[0] != null)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (a[0] == Acc[i])
                    {
                        return Pos[i];
                    }
                }
            }
            return -1;
        }


        int Check_InforName (string mess)
        {
            for (int i = 0; i < 10000; i++)
            {
                if (mess == Items_Name[i] + ";")
                {
                    return i;
                }
            }
            return -1;
        }

        int Check_Completed(string mess)
        {
            string[] temp = mess.Split('>');
            string[] aL = temp[0].Split('|');
            int code = 9999;
            for(int i = 0; i < 10000; i ++)
            {
                if (aL[1] == Items_Name[i])
                {
                    code = i;
                    break;
                }
            }
            for (int i = 0; i < 100; i++)
            {
                if (aL[0] == Acc[i])
                {
                   Items_User[code] = Acc[i];
                   Items_Pos[code] = Pos[i];
                   Items_Done[code] = aL[2];
                   break;
                }
            }
            return code;
        }
   
        void XuLyText (string mess)
        {
            DiaChi = -1;
            DuLieu = "";
            string[] aL = mess.Split('/');
            if(aL[0] != null)
            {
                int num = Search_Index_Adress(aL[0]);
                if(num != -1)
                {
                    if (aL[1] != null)
                    {
                        if (aL[1].Substring(aL[1].Length - 1, 1) == "'")      // " ' " la yeu cau dang nhap
                        {
                            DuLieu = Check_Login(aL[1]);
                            int int_temp = Check_Login_Pos(aL[1]);
                            if (int_temp != -1)
                            {
                                AutoSend(num, DuLieu);
                                AutoSend(num, Convert.ToString(int_temp));
                            }

                        }
                        else if (aL[1].Substring(aL[1].Length - 1, 1) == ";")  // " ; " gui du lieu trong list data cua items
                        {
                            // add them 1 ham tim du lieu tu QR
                            int ttDulieu = Check_InforName(aL[1]);
                            if (ttDulieu != -1)
                            {
                                string status = "*";
                                if(Items_Done[ttDulieu] == "1")
                                {
                                    status = "Submited!";
                                }
                                else
                                {
                                    status = "Working";
                                }
                                DuLieu = "Code:\t" + Items_Name[ttDulieu] + "|" +
                                    "Job Order:\t" + Items_Order[ttDulieu] + "|" +
                                    "Drawing:\t" + Items_Drawing[ttDulieu] + "|" +
                                    "Qty:\t" + Items_Qty[ttDulieu] + "|" +
                                    "High:\t" + Items_High[ttDulieu] + "|" +
                                    "Length:\t" + Items_Length[ttDulieu] + "|" +
                                    "Weight:\t" + Items_Weight[ttDulieu] + "|" +
                                    "Total:\t" + Items_Total[ttDulieu]+"|";
                                DuLieu += "Status:\t" + status;
                            }
                            else
                            {
                                DuLieu = "no data";
                            }
                            AutoSend(num, DuLieu);
                        }
                        else if (aL[1].Substring(aL[1].Length - 1, 1) == ">")  // " ; " gui du lieu trong list data cua items
                        {
                            int code = Check_Completed(aL[1]);
                            AutoSend(num, "Submit Successfully<");
                        }
                    }
                }
            }

        }
    }
}