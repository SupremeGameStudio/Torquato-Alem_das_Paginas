using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using Services;

namespace BattleGame.Networking {
    [Serializable]
    public abstract class NetMessage {
    
        public static Message Write<T>(T obj) where T : NetMessage {
            Message message = new Message(Message.Type.Data);
        
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            byte[] ret = ms.ToArray();
            message.Write(ret);

            return message;
        }

        [CanBeNull]
        public static NetMessage Read(Message message) {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(message.data, 0, message.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            return (NetMessage)binForm.Deserialize(memStream);
        }

        public abstract void Handle(GameHandler handler);
    }
}