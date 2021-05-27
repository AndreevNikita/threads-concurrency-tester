import tester

class Test:

    def reset(self):
        self.update_flag = False
        self.current_data = False
        self.received_data = False
    
    def thread1_coro(self):
        if self.update_flag:
            yield "1.1"
            self.update_flag = False
            yield "1.2"
            self.received_data = self.current_data

        yield "1.e"

    def thread2_coro(self):
        self.current_data = True
        yield "2.1"
        self.update_flag = True
        yield "2.e"
    


    def get_thread_coros(self):
        return [self.thread1_coro, self.thread2_coro]
    
    def check(self):
        return (self.update_flag and self.current_data) or self.received_data

    def on_test_failed(self):
        print(f"update_flag = {self.update_flag}; current_data = {self.current_data}; receive_data = {self.received_data}")
    
tester.test(Test())