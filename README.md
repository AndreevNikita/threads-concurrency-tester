# Threads concurrency tester

The project that helps to test multithread code parts that may cause race conditions. Provides method, that executes several methods step by step with all available combinations. There are two test projects:
* c# - as main
* python - for more lightweight tests

## Getting started

A method that simulates a thread is written like this

```c#
public IEnumerable<string> Thread1Coro() { 
    if(updateFlag) {
        yield return "1.1";
        updateFlag = false;
        yield return "1.2";
        receivedData = currentData;
    }

    yield return "1.e";
}
```

**yield return** gives ability to pause this method and start or continue other method. Returned string values are comments, that will be logged in the order of execution.  
*You can read the sections of code between **yield return** atomic operation*

It is convenient to write a test as a class, that implements the **ITest** interface.  

Description of the **ITest** interface methods:
- IEnumerable<string>[] GetThreadsCoros() - must return an array of thread simulation methods.
- void Reset() - resets variables (the test object's condition to initial state) after each test for the net test.
- bool Check() - checking for correct state after executing threads
- void OnTestFailed() - on test failed callback
- void OnTestPassed() - on test passed callback

To run the test, you need to call the **Tester.Test** method and pass your **ITest** object as the first argument.

```c#
static void Main(string[] args) {
    Tester.Test(new Test());
}
```

You can also pass **ExecutionLogOptions** flags as the second argument to hide or show log messages of some types and the max steps count (**int maxSteps**) as the third aurgument of the **Tester.Test** method.

## Python project
There is also the [python](/python/) fully ported tester  (names are translated to the snake case style). It's more lightweight and simple for test writing.

