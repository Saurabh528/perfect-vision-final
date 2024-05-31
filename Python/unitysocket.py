import os
# for TCP connection with unity
import socket
import sys
from utils import append_to_log
# init TCP connection with unity
# return the socket connected



def init_TCP(port):
    address = ('127.0.0.1', port)
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect(address)
        append_to_log("Connected to address: ")
        return s
        
    except Exception as e:
        append_to_log("Error while connecting:", e)
        sys.exit()

def send_command_to_unity(s, strarg):
    msg = 'CMD:' + strarg

    try:
        s.send(bytes(msg, "utf-8"))
    except socket.error as e:
        print("error while sending :: " + str(e))

        # quit the script if connection fails (e.g. Unity server side quits suddenly)
        sys.exit()
        
def send_message_to_unity(s, strarg):
    msg = 'MSG:' + strarg

    try:
        s.send(bytes(msg, "utf-8"))
    except socket.error as e:
        append_to_log("error while sending :: " + str(e))

        # quit the script if connection fails (e.g. Unity server side quits suddenly)
        sys.exit()
        
def send_status_to_unity(s, strarg):
    msg = 'STS:' + strarg
    try:
        s.send(bytes(msg, "utf-8"))
    except socket.error as e:
        append_to_log("error while sending :: " + str(e))

        # quit the script if connection fails (e.g. Unity server side quits suddenly)
        sys.exit()
    
    
    
import selectors
import types

sel = selectors.DefaultSelector()

def start_connection(port, callback):
    server_addr = ('127.0.0.1', port)
    print(f"Starting connection to {server_addr}")
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.setblocking(False)
    sock.connect_ex(server_addr)
    events = selectors.EVENT_READ | selectors.EVENT_WRITE
    data = types.SimpleNamespace(
        callback=callback,  # Store the callback
        messages=[b"Hello, Server!"],
        outb=b"",
    )
    sel.register(sock, events, data=data)
    return sock

def service_connection(key, mask):
    sock = key.fileobj
    data = key.data
    if mask & selectors.EVENT_READ:
        recv_data = sock.recv(1024)  # Should be ready to read
        if recv_data:
            print(f"Received {recv_data.decode('utf-8')}")
            data.callback(recv_data)  # Call the callback function
        else:
            print("Closing connection")
            sel.unregister(sock)
            sock.close()
    if mask & selectors.EVENT_WRITE:
        if not data.outb and data.messages:
            data.outb = data.messages.pop(0)
        if data.outb:
            print(f"Sending {data.outb.decode('utf-8')}")
            sent = sock.send(data.outb)  # Should be ready to write
            data.outb = data.outb[sent:]

def get_selector():
    return sel