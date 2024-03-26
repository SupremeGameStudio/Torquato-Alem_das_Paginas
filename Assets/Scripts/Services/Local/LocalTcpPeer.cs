using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Threading;

namespace Services.Local {
    public class LocalTcpPeer {

        public readonly TcpClient socket;

        private int _pos, _size, _type, _reaming = -1;
        private byte[] _buffer = new byte[1024];
        private bool _disconnected;

        private Thread _sendthread;
        private byte[] _sendSize = new byte[4];
        private List<Message> _packages = new List<Message>();
        private List<Message> _packagesSend = new List<Message>();

        public LocalTcpPeer(TcpClient socket) {
            this.socket = socket;

            _sendthread = new Thread(SendThread);
            _sendthread.Start();
        }

        public void Send(Message message) {
            if (message.Length > 0) {
                lock (_packages) {
                    _packages.Add(message);
                    Monitor.PulseAll(_packages);
                }
            }
        }

        public void Close() {
            _disconnected = true;
            lock (_packages) {
                Monitor.PulseAll(_packages);
            }
        }

        public void Wait() {
            _sendthread.Join();
        }

        private void SendThread() {
            bool dead = false;

            while (!_disconnected) {
                lock (_packages) {
                    while (!_disconnected && _packages.Count == 0) {
                        Monitor.Wait(_packages);
                    }

                    if (_disconnected) {
                        break;
                    }

                    _packagesSend.AddRange(_packages);
                    _packages.Clear();
                }

                foreach (var message in _packagesSend) {
                    if (_disconnected) {
                        break;
                    }

                    try {
                        WriteMessage(message);
                    } catch {
                        dead = true;
                        _disconnected = true;
                        break;
                    }
                }

                _packagesSend.Clear();
            }

            if (!dead) {
                try {
                    _sendSize[0] = 0;
                    _sendSize[1] = 0;
                    socket.GetStream().Write(_sendSize, 0, 2);
                } catch {
                    // ignored
                }
            }
            
            try {
                socket.Close();
            } catch {
                // ignored
            }
        }

        public bool IsDisconected() {
            return _disconnected;
        }

        public int NextMessage() {
            if (_disconnected) {
                return -1;
            }

            try {
                while (_reaming == -1 && _pos < 3 && socket.GetStream().DataAvailable) {
                    _pos = socket.GetStream().Read(_buffer, _pos, 3 - _pos);
                }

                if (_reaming == -1 && _pos >= 3) {
                    _type = _buffer[0];
                    _size = ((_buffer[1] & 0xFF) << 8) | (_buffer[2] & 0xFF);
                    _pos = 0;
                    _reaming = _size;
                    
                    if (_size <= 0 || _size > 1024) {
                        throw new Exception("Invalid message byte size");
                    }
                }

                while (_reaming > 0 && socket.GetStream().DataAvailable) {
                    int read = socket.GetStream().Read(_buffer, _pos, _reaming);
                    _pos += read;
                    _reaming -= read;
                }
            } catch {
                _disconnected = true;
                return -1;
            }

            return _reaming <= 0 && _size > 0 ? _size : -1;
        }

        public Message ReadMessage() {
            byte[] data = new byte[_size];
            for (int i = 0; i < data.Length; i++) {
                data[i] = _buffer[i];
            }
            
            _reaming = -1;
            _size = 0;
            _type = 0;
            _pos = 0;

            return new Message((Message.Type)_type, data);
        }

        public void WriteMessage(Message message) {
            _sendSize[0] = (byte)message.type;
            _sendSize[1] = (byte)(message.Length & 0xFF);
            _sendSize[2] = (byte)((message.Length >> 8) & 0xFF);
            socket.GetStream().Write(_sendSize, 0, 3);
            socket.GetStream().Write(message.data, 0, message.Length);
        }
    }
}