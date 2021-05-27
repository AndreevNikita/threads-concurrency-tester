from enum import Enum
from colorama import Fore, Style

class ExecutionResult(Enum):
    SUCCESS = 0
    END = 1
    FAKE = 2

EXECUTION_OPTIONS_ALL = 0xffff
EXECUTION_OPTIONS_TEST_SUCCESS = 1
EXECUTION_OPTIONS_TEST_FAIL = 2

EndOfCoro = object()

def execute(coros, last_execution_order, steps_log, max_steps):

    current_common_step = 0
    while True:
        if current_common_step == max_steps:
            return ExecutionResult.SUCCESS
        
        if current_common_step == len(last_execution_order):
            was_found = False

            for index in range(len(coros)):
                current_value = next(coros[index], EndOfCoro)
                if current_value is not EndOfCoro:
                    steps_log.append(current_value)

                    last_execution_order.append(index)
                    was_found = True
                    break
            
            if not was_found:
                return ExecutionResult.SUCCESS
            
            current_common_step += 1
            continue

        elif  current_common_step == len(last_execution_order) - 1:
            was_found = False

            for index in range(last_execution_order[-1] + 1, len(coros)):
                current_value = next(coros[index], EndOfCoro)
                if current_value is not EndOfCoro:
                    steps_log.append(current_value)

                    last_execution_order[-1] = index
                    was_found = True
                    break
            
            if not was_found:
                del last_execution_order[-1]
                if len(last_execution_order) == 0:
                    return ExecutionResult.END
                return ExecutionResult.FAKE
            
            current_common_step += 1
            continue
        else:
            current_value = next(coros[last_execution_order[current_common_step]])
            steps_log.append(current_value)
            current_common_step += 1
            continue

def test(test_object, log_options = EXECUTION_OPTIONS_ALL, max_steps = 0xffffffff):
    test_object.reset()

    coros = test_object.get_thread_coros()

    execution_order_buffer = []
    passed_tests_counter = 0
    failed_tests_counter = 0
    while True:
        steps_log = []

        execution_result = execute([coro() for coro in coros], execution_order_buffer, steps_log, max_steps)
        if execution_result == ExecutionResult.SUCCESS:

            check_result = test_object.check()

            if (check_result and ((log_options & EXECUTION_OPTIONS_TEST_SUCCESS) != 0)) or ((not check_result) and ((log_options & EXECUTION_OPTIONS_TEST_FAIL) != 0)):
                print()
                print(execution_order_buffer)
                print()
                for line in steps_log:
                    if line is not None:
                        print(line)
                print()
            
            if check_result:
                if (log_options & EXECUTION_OPTIONS_TEST_SUCCESS) != 0:
                    print(f"{Fore.GREEN}OK{Style.RESET_ALL}")
                
                on_test_passed = getattr(test_object, "on_test_passed", None)
                if callable(on_test_passed):
                    on_test_passed()
                
                passed_tests_counter += 1
            else:
                if (log_options & EXECUTION_OPTIONS_TEST_FAIL) != 0:
                    print(f"{Fore.RED}Fail!{Style.RESET_ALL}")
                
                on_test_failed = getattr(test_object, "on_test_failed", None)
                if callable(on_test_failed):
                    on_test_failed()
                
                failed_tests_counter += 1
            
        if execution_result == ExecutionResult.END:
            break

        test_object.reset()
    
    print()
    if failed_tests_counter == 0:
        print(f"{Fore.GREEN}All tests passed! ({passed_tests_counter}){Style.RESET_ALL}")
    else:
        print(f"!!! There are failed tests ({failed_tests_counter} / {passed_tests_counter + failed_tests_counter}) !!!")