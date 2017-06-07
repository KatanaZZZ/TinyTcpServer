﻿using System;
using System.Net.Sockets;
using System.Threading;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils
{
    public class TransportClient : IDisposable
    {
        public TransportClient(Boolean isAsync, String server, UInt16 port)
        {
            Init(server, port, isAsync);
            _client = new TcpClient();
        }

        public Boolean Open(String server, UInt16 port, Boolean isAsync = true)
        {
            Init(server, port, isAsync);
            return Open();
        }

        public Boolean Open()
        {
            if(_isAsync)
                OpenAsync();
            else OpenSync();
            return _client.Connected;
        }

        public void Close()
        { 
            _client.Close();
        }

        public void Dispose()
        {
            Close();
            _writeCompleted.Dispose();
            _readCompleted.Dispose();
            _connectCompleted.Dispose();
        }

        public Boolean Read(Byte[] data, out Int32 bytesRead)
        {
            _bytesRead = 0;
            if (_isAsync)
                return ReadAsync(data, out bytesRead);
            return ReadSync(data, out bytesRead);
        }

        public Boolean Write(Byte[] data)
        {
            if (_isAsync)
                return WriteAsync(data);
            return WriteSync(data);
        }

        private void OpenSync()
        {
            _client.Connect(_server, _port);
        }

        private void OpenAsync()
        {
            _connectCompleted.Reset();
            _client.BeginConnect(_server, _port, OpenAsyncCallback, _client);
            _connectCompleted.Wait(DefaultConnectTimeout);
        }

        private void Init(String server, UInt16 port, Boolean isAsync)
        {
            if (String.IsNullOrEmpty(server))
                throw new ArgumentNullException("server");
            _isAsync = isAsync;
            _server = server;
            _port = port;
        }

        private void OpenAsyncCallback(IAsyncResult result)
        {
            TcpClient client = (result.AsyncState as TcpClient);
            if (client == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("client");
            client.EndConnect(result);
            _connectCompleted.Set();
        }

        private Boolean WriteSync(Byte[] data)
        {
            NetworkStream stream = _client.GetStream();
            try
            {
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private Boolean WriteAsync(Byte[] data)
        {
            NetworkStream stream = _client.GetStream();
            try
            {
                _writeCompleted.Reset();
                stream.BeginWrite(data, 0, data.Length, WriteAsyncCallback, _client);
                _writeCompleted.Wait(1000);
                    //Write(data, 0, data.Length);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void WriteAsyncCallback(IAsyncResult result)
        {
            TcpClient client = (result.AsyncState as TcpClient);
            if (client == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("client");
            client.GetStream().EndWrite(result);
            _writeCompleted.Set();
        }

        private Boolean ReadSync(Byte[] data, out Int32 bytesRead)
        {
            bytesRead = 0;
            NetworkStream stream = _client.GetStream();
            try
            {
                _readCompleted.Reset();
                _bytesRead = stream.Read(data, 0, data.Length);
                bytesRead = _bytesRead;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        private Boolean ReadAsync(Byte[] data, out Int32 bytesRead)
        {
            bytesRead = 0;
            NetworkStream stream = _client.GetStream();
            try
            {
                _readCompleted.Reset();
                stream.BeginRead(data, 0, data.Length, ReadAsyncCallback, _client);
                _readCompleted.Wait(1000);
                bytesRead = _bytesRead;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void ReadAsyncCallback(IAsyncResult result)
        {
            TcpClient client = (result.AsyncState as TcpClient);
            if (client == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("client");
            _bytesRead = client.GetStream().EndRead(result);
            _readCompleted.Set();
        }

        private const Int32 DefaultConnectTimeout = 1000;

        private Boolean _isAsync;
        private String _server;
        private UInt16 _port;
        private readonly TcpClient _client;

        private Int32 _bytesRead;

        private readonly ManualResetEventSlim _connectCompleted = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _readCompleted = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _writeCompleted = new ManualResetEventSlim(false);
    }
}
