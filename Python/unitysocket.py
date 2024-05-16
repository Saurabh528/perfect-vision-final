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
    