using System;

namespace Net.Astropenguin.Messaging
{
	public class Message
	{
		public Type TargetType { get; private set; }
		public string Content { get; private set; }
		public object Payload { get; private set; }
		public Func<object> Dispatcher { get; private set; }

		public DateTime Timestamp { get; private set; }

		public Message( Type id, string Content, object Payload = null )
		{
			this.TargetType = id;
			this.Content = Content;
			this.Payload = Payload;
			Timestamp = DateTime.Now;		   
		}

		public Message( Type id, string Content, Func<object> Payload )
		{
			this.TargetType = id;
			this.Content = Content;
			this.Dispatcher = Payload;
			Timestamp = DateTime.Now;
		}
	}
}
