using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadsConcurrencyTester
{
	public interface ITest {
		
		

		IEnumerable<string>[] GetThreadsCoros();



		void Reset();
		
		bool Check();

		void OnTestFailed();

		void OnTestPassed();
	}
}
