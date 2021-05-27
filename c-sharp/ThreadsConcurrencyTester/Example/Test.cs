using System;
using System.Collections.Generic;
using System.Text;
using ThreadsConcurrencyTester;

namespace Example
{
	//Test changed data source and sending data only when updated
	//When first thread iteration finished, the updateFlag must have True value (that shows that data must be sended on the next iteration),
	//or receivedData must have changedData value (the value sended)
	//
	//Finally this test show, that the first thread can't change the updateFlag without sending data
	public class Test : ITest {
		bool updateFlag;
		bool currentData;

		bool receivedData;

		public Test() {
			Reset();
		}

		public IEnumerable<string> Thread1Coro() { 
			if(updateFlag) {
				yield return "1.1";
				updateFlag = false;
				yield return "1.2";
				receivedData = currentData;
			}

			yield return "1.e";
		}

		public IEnumerable<string> Thread2Coro() {
			currentData = true;
			yield return "2.1";
			updateFlag = true;

			yield return "2.e";
		}

		public void Reset() {
			updateFlag = false;
			currentData = false;
			receivedData = false;
		}

		public IEnumerable<string>[] GetThreadsCoros() {
			return new IEnumerable<string>[] { 
				Thread1Coro(), 
				Thread2Coro() 
			};
		}

		public bool Check()
		{
			return (updateFlag && currentData) || receivedData;
		}

		public void OnTestFailed() {
			Console.WriteLine($"updateFlag = {updateFlag}; currentData = {currentData}; receivedData = {receivedData}");
		}

		public void OnTestPassed()
		{
		}
	}
}
