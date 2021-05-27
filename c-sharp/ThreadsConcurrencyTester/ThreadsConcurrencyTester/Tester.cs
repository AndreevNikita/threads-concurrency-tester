using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreadsConcurrencyTester
{
	[Flags]
	public enum ExecutionLogOptions { 
		All = 0xffff, 
		TestSuccess = 1, 
		TestFail = 2
	}

	public static class Tester
	{
		private static void PrintList(List<int> list) {
			Console.Write("(");
			foreach(int element in list)
				Console.Write($"{element} ");
			Console.WriteLine(")");
		}


		private enum ExecutionResult { Success, End, Fake}

		private static ExecutionResult Execute(IEnumerator<string>[] coros, List<int> lastExecutionOrder, Queue<string> stepsLogQueue, int maxSteps) { 

			int currentCommonStep = 0;
			while(true) {
				if(currentCommonStep == maxSteps)
					return ExecutionResult.Success;

				if(currentCommonStep == lastExecutionOrder.Count) { //Build next path
					bool wasFound = false;
					for(int index = 0; index < coros.Length; index++) { 
						if(coros[index].MoveNext()) { 
							stepsLogQueue.Enqueue(coros[index].Current);

							lastExecutionOrder.Add(index);
							wasFound = true;
							break;
						}
					}

					if(!wasFound)
						return ExecutionResult.Success;

					currentCommonStep++;
					continue;
				} else if(currentCommonStep == lastExecutionOrder.Count - 1) { //Change the last element in the path
					bool wasFound = false;
					for(int index = lastExecutionOrder[lastExecutionOrder.Count - 1] + 1; index < coros.Length; index++) { 
						if(coros[index].MoveNext()) {
							stepsLogQueue.Enqueue(coros[index].Current);
							lastExecutionOrder[lastExecutionOrder.Count - 1] = index;
							wasFound = true;
							break;
						}
					}

					//If there is no other element, make step back
					if(!wasFound) {
						lastExecutionOrder.RemoveAt(lastExecutionOrder.Count - 1);
						if(lastExecutionOrder.Count == 0)
							return ExecutionResult.End;
						return ExecutionResult.Fake;
					}

					currentCommonStep++;
					continue;
				} else { 
					coros[lastExecutionOrder[currentCommonStep]].MoveNext();
					stepsLogQueue.Enqueue(coros[lastExecutionOrder[currentCommonStep]].Current);
					currentCommonStep++;
					continue;
				}

			}

		}

		public static void Test(ITest test, ExecutionLogOptions logOptions = ExecutionLogOptions.All, int maxSteps = int.MaxValue) {
			test.Reset();

			var coros = test.GetThreadsCoros();

			List<int> executionOrderBuffer = new List<int>();
			ExecutionResult executionResult;
			int passedTestsCounter= 0;
			int failedTestsCounter = 0;
			do {
				Queue<string> stepsLogQueue = new Queue<string>();

				executionResult = Execute(coros.Select(coro => coro.GetEnumerator()).ToArray(), executionOrderBuffer, stepsLogQueue, maxSteps);
				if(executionResult == ExecutionResult.Success) {
					
					bool checkResult = test.Check();

					if((checkResult && (logOptions.HasFlag(ExecutionLogOptions.TestSuccess))) || (!checkResult && (logOptions.HasFlag(ExecutionLogOptions.TestFail)))) {
						Console.WriteLine();
						PrintList(executionOrderBuffer);

						Console.WriteLine();
						foreach(string line in stepsLogQueue) {
							if(line != null)
								Console.WriteLine(line);
						}
						Console.WriteLine();
					}

					if(checkResult) {
						if(logOptions.HasFlag(ExecutionLogOptions.TestSuccess)) {
							Console.ForegroundColor = ConsoleColor.DarkGreen;
							Console.WriteLine("OK");
							Console.ForegroundColor = ConsoleColor.White;
						}

						test.OnTestPassed();
						passedTestsCounter++;
					} else { 
						if(logOptions.HasFlag(ExecutionLogOptions.TestFail)) {
							Console.ForegroundColor = ConsoleColor.DarkRed;
							Console.WriteLine($"Fail!");
							Console.ForegroundColor = ConsoleColor.White;
						}

						test.OnTestFailed();
						failedTestsCounter++;
						
					}

					
				}

				test.Reset();
			} while(executionResult != ExecutionResult.End);

			Console.WriteLine();
			if(failedTestsCounter == 0) { 
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine($"All tests passed! ({passedTestsCounter})");
				Console.ForegroundColor = ConsoleColor.White;
			} else { 
				Console.WriteLine($"!!! There are failed tests ({failedTestsCounter} / {passedTestsCounter + failedTestsCounter}) !!!");
			}

		}
	}
}
