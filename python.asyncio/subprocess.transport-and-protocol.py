# -*- coding: utf-8 -*-

import asyncio
import sys


class DateProtocol(asyncio.SubprocessProtocol):
    def __init__(self, exit_future):
        self.exit_future = exit_future
        self.output = bytearray()

    def pipe_data_received(self, fd, data):
        self.output.extend(data)

    def process_exited(self):
        self.exit_future.set_result(True)


async def get_date(_loop):
    code = 'import datetime; print(datetime.datetime.now())'
    exit_future = asyncio.Future(loop=_loop)
    # Create the subprocess controlled by the protocol DateProtocol,
    # redirect the standard output into a pipe
    create = _loop.subprocess_exec(lambda: DateProtocol(exit_future), sys.executable, '-c', code, stdin=None,
                                   stderr=None)
    transport, protocol = await create
    # Wait for the subprocess exit using the process_exited() method
    # of the protocol
    await exit_future
    # Close the stdout pipe
    transport.close()
    # Read the output which was collected by the pipe_data_received()
    # method of the protocol
    data = bytes(protocol.output)
    return data.decode('ascii').rstrip()


if sys.platform == "win32":
    loop = asyncio.ProactorEventLoop()
    asyncio.set_event_loop(loop)
else:
    loop = asyncio.get_event_loop()

date = loop.run_until_complete(get_date(loop))
print("Current date: %s" % date)
loop.close()
