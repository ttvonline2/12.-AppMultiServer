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

        private const int PORT = 100;
        IPAddress address = IPAddress.Parse("10.12.20.77");

        //

        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public MainWindow()
        {
            InitializeComponent();

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
                        tbData.Text = "Text received: " + text;
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

    }


}
