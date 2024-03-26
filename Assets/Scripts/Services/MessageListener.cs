using System.Collections.Generic;

namespace Services {
    public class MessageListener {

        private Queue<Message> messages;

        public MessageListener() {
            messages = new Queue<Message>();
        }
        
        public void EnqueueMessage(Message message) {
            messages.Enqueue(message);
        }

        public Message DequeueMessage() {
            return messages.Count == 0 ? null : messages.Dequeue();
        }
    }
}