using System;
using System.Text;

namespace Services {
    public class Message {
        public enum Type {
            Tcp, Udp, Connection, Disconnection, ServerDisconnected, LobbyData, Data
        }

        public Type type;
        public byte[] data;
        private int position;

        public Message(Type type, byte[] data = null) {
            this.type = type;
            if (data != null) {
                this.data = data;
                this.Length = data.Length;
            }
        }

        public int Length { get; private set; }

        private void EnsureCapacity(int requiredSpace) {
            if (data == null) {
                data = new byte[32];
            }
            Length = position + requiredSpace;
            
            int currentLength = data.Length;
            while (Length > currentLength) {
                currentLength *= 2;
            }
            if (currentLength != data.Length) {
                Array.Resize(ref data, currentLength);
            }
        }

        public Message Write(byte[] byteArray) {
            EnsureCapacity(byteArray.Length);
            byteArray.CopyTo(data, position);
            position += byteArray.Length;
            return this;
        }

        public Message Write(int value) {
            EnsureCapacity(sizeof(int));
            BitConverter.GetBytes(value).CopyTo(data, position);
            position += sizeof(int);
            return this;
        }

        public Message Write(float value) {
            EnsureCapacity(sizeof(float));
            BitConverter.GetBytes(value).CopyTo(data, position);
            position += sizeof(float);
            return this;
        }

        public Message Write(ulong value) {
            EnsureCapacity(sizeof(ulong));
            BitConverter.GetBytes(value).CopyTo(data, position);
            position += sizeof(ulong);
            return this;
        }

        public Message Write(string value) {
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);
            EnsureCapacity(sizeof(int) + stringBytes.Length);
            BitConverter.GetBytes(stringBytes.Length).CopyTo(data, position);
            position += sizeof(int);
            stringBytes.CopyTo(data, position);
            position += stringBytes.Length;
            return this;
        }
    
        public Message Write(bool value) {
            EnsureCapacity(sizeof(bool));
            BitConverter.GetBytes(value).CopyTo(data, position);
            position += sizeof(bool);
            return this;
        }

        public Message Pack() {
            Array.Resize(ref data, position);
            position = 0;
            return this;
        }

        public Message Reset() {
            position = 0;
            return this;
        }

        public Message Reuse(Type type) {
            this.type = type;
            position = 0;
            Length = 0;
            return this;
        }

        public int ReadInt() {
            int result = BitConverter.ToInt32(data, position);
            position += sizeof(int);
            return result;
        }

        public float ReadFloat() {
            float result = BitConverter.ToSingle(data, position);
            position += sizeof(float);
            return result;
        }

        public ulong ReadLong() {
            ulong result = BitConverter.ToUInt64(data, position);
            position += sizeof(ulong);
            return result;
        }

        public string ReadString() {
            int stringLength = BitConverter.ToInt32(data, position);
            position += sizeof(int);
            string result = Encoding.UTF8.GetString(data, position, stringLength);
            position += stringLength;
            return result;
        }
    
        public bool ReadBool() {
            bool result = BitConverter.ToBoolean(data, position);
            position += sizeof(bool);
            return result;
        }

        public void SetBytes(byte[] data) {
            this.data = data;
            this.Length = data.Length;
        }
    }
}