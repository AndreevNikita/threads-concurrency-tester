using System;
using ThreadsConcurrencyTester;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
			Tester.Test(new Test(), ExecutionLogOptions.All);
		}
	}
}
