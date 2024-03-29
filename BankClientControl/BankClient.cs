﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliServLib;
using TcpLib;
using System.Net.Sockets;
using System.Windows.Threading;

namespace BankClientControl
{
    public class BankClient
    {
        ClientConnectAsync conn;
        MessageData data = new MessageData();
        Client tcpClient;
        TxDataGetter dataGetter = new TxDataGetter();
        Socket _clientSocket;
        private const int MaxEmptyRcv = 100;
        private int currentNumEmptyRcv = 0;
        string _ip = String.Empty;
        int _port = 0;

        public BankClient()
        {
            conn = new ClientConnectAsync();
            IsConnected = false;
            ClientConnectAsync.OnConnect += ClientConnectAsync_OnConnect;
        }

        private void Receiver_ClientDataReceived(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ReceiveData data = e.UserState as ReceiveData;
                if (data != null)
                {
                    MessageData msg = data.clientData as MessageData;
                    if ((msg != null) && (msg.id > 0))
                    {
                        Console.WriteLine("<<< Client: {0} -- Received Msg from Server >>>", msg.handle);
                        if (!Dispatcher.CheckAccess())
                        {
                            Dispatcher.Invoke(BankClientControl.dataAction, msg);
                        }
                        else
                        {
                            BankClientControl.dataAction.Invoke(msg);
                        }
                        currentNumEmptyRcv = 0;
                    }
                    else
                    {
                        ++currentNumEmptyRcv;
                        if (currentNumEmptyRcv == MaxEmptyRcv)
                        {
                            Close();
                        }
                    }
                }
            }
        }

        public BankClient(AddressFamily addressFamily, SocketType socketType, ProtocolType protoType)
        {
            conn = new ClientConnectAsync(addressFamily, socketType, protoType);
            IsConnected = false;
            ClientConnectAsync.OnConnect += ClientConnectAsync_OnConnect;
        }

        public bool IsConnected
        {
            get;
            private set;
        }

        public Socket ClientSocket
        {
            get { return _clientSocket; }
            private set { _clientSocket = value; }
        }

        public string IP
        {
            get { return _ip; }
            private set { _ip = value; }
        }

        public int Port
        {
            get { return _port; }
            private set { _port = value; }
        }

        public Dispatcher Dispatcher
        {
            get;
            set;
        }

        public Client TcpClient()
        {
            return tcpClient;
        }

        public bool Connect(string ip, int port, int delay, int cycles)
        {
            _ip = ip;
            _port = port;
            var connectResult = conn.ConnectAsync(_port, _ip, delay, cycles);
            if (connectResult.Result.Failure)
            {
                return false;
            }
            _clientSocket = connectResult.Result.Value;
            tcpClient = new Client(_clientSocket, 1024, dataGetter);
            tcpClient.Receiver.ClientDataReceived += Receiver_ClientDataReceived;

            return _clientSocket.Connected;
        }

        public void Close()
        {
            TcpClient().Stop();
        }

        private void ClientConnectAsync_OnConnect(Socket socket)
        {
            System.Diagnostics.Debug.WriteLine("Connect Event from Socket " + socket.Handle);

            IsConnected = true;
        }

    }
}
