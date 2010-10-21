//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTuner.iTunes
{
	using System;
	using System.Collections.Generic;
	using System.Threading;


	/// <summary>
	/// Represents a first-in, first-out collection of objects.  This is a thread-safe
	/// collection that blocks dequeue requests until an item is available in the queue.
	/// </summary>

	internal class BlockingQueue<T> : Queue<T>, IDisposable
	{
		private object sync;
		private bool isDisposed;


		//========================================================================================
		// Constructors
		//========================================================================================

		/// <summary>
		/// Initialize a new empty instance.
		/// </summary>

		public BlockingQueue ()
			: base()
		{
			this.sync = new object();
			this.isDisposed = false;
		}


		/// <summary>
		/// Destroy the queue and free up waiting threads.
		/// </summary>

		~BlockingQueue ()
		{
			Dispose();
		}


		/// <summary>
		/// Clear the queue and free up waiting threads.
		/// </summary>

		public void Dispose ()
		{
			if (!isDisposed)
			{
				isDisposed = true;

				lock (sync)
				{
					base.Clear();
					Monitor.PulseAll(sync);
				}
			}
		}


		//========================================================================================
		// Methods
		//========================================================================================

		/// <summary>
		/// Remove all objects from the Queue.
		/// </summary>

		public new void Clear ()
		{
			lock (sync)
			{
				base.Clear();
			}
		}


		/// <summary>
		/// Removes and returns the object at the beginning of the Queue.
		/// This is a blocking operation: if the queue is empty, this method waits
		/// until a new entry is added, otherwise the top entry is returned immediately.
		/// </summary>
		/// <returns>The first T object in queue.</returns>

		public new T Dequeue ()
		{
			lock (sync)
			{
				while (!isDisposed && (base.Count == 0))
				{
					Monitor.Wait(sync);
				}

				return base.Dequeue();
			}
		}


		/// <summary>
		/// Adds an object to the end of the Queue.
		/// </summary>
		/// <param name="obj">Object to put in queue</param>

		public new void Enqueue (T obj)
		{
			lock (sync)
			{
				base.Enqueue(obj);
				Monitor.Pulse(sync);
			}
		}
	}
}
