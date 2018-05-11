using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Helpers
{
	public class AsyncLocks<HashType, YieldType>
	{
		public class QueueToken
		{
			private TaskCompletionSource<YieldType> TCS;
			public Task<YieldType> Task => TCS.Task;

			public QueueToken()
			{
				TCS = new TaskCompletionSource<YieldType>();
			}

			public bool TrySetResult( YieldType Value )
			{
				return TCS.TrySetResult( Value );
			}
		}

		private ConcurrentDictionary<HashType, QueueToken> PDict = new ConcurrentDictionary<HashType, QueueToken>();

		public bool AcquireLock( HashType HT, out QueueToken QT )
		{
			RetryLock:
			if( PDict.TryGetValue( HT, out QT ) )
			{
				return false;
			}
			else
			{
				QT = new QueueToken();

				if( PDict.TryAdd( HT, QT ) )
				{
					AutoRelease( HT, QT.Task );
					return true;
				}

				goto RetryLock;
			}
		}

		private async void AutoRelease( HashType HT, Task<YieldType> Task )
		{
			await Task;
			PDict.TryRemove( HT, out QueueToken QT );
		}

	}
}