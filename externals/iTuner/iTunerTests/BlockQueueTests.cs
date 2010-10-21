//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace iTunerTests
{
	using System;
	using System.ComponentModel;
	using System.Threading;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using iTuner.iTunes;


	/// <summary>
	/// 
	/// </summary>

	[TestClass]
	public class BlockingQueueTests : TestBase
	{

		/// <summary>
		/// 
		/// </summary>

		[TestMethod]
		public void Queue ()
		{
			BlockingQueue<string> queue = new BlockingQueue<string>();

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler(DoWork);
			worker.ProgressChanged += new ProgressChangedEventHandler(DoProgressChanged);
			worker.WorkerReportsProgress = true;
			worker.RunWorkerAsync(queue);

			int count = 0;
			while (count < 5)
			{
				string s = queue.Dequeue();
				Console.WriteLine("dequeued " + count + "(" + s + ")");
				count++;
			}

			queue.Clear();
			queue.Dispose();
			queue = null;
		}


		private void DoWork (object sender, DoWorkEventArgs e)
		{
			BlockingQueue<string> queue = e.Argument as BlockingQueue<string>;

			for (int i = 0; i < 10; i++)
			{
				Thread.Sleep(1000);
				Console.WriteLine("enqueue " + i);
				queue.Enqueue("enqueue " + i);
				((BackgroundWorker)sender).ReportProgress(i + 1);
			}
		}


		private void DoProgressChanged (object sender, ProgressChangedEventArgs e)
		{
			Console.WriteLine("progress " + e.ProgressPercentage);
		}
	}
}
