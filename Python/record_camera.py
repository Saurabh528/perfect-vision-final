import cv2
import time
import threading
import os
from utils import get_anonymous_directory, wait_for_camera
from argparse import ArgumentParser
from export_video import init_writer, write_frame, release_writer
from datetime import datetime

recording = False
saveFileName = ""

def process_metrics(cap):
    global recording, args
    while recording:
        ret, frame = cap.read()
        if not ret:
            break
        write_frame(frame)
        time.sleep(0.001)
    release_writer()
    if saveFileName != "":
         # Format the date and time as "day-month-year_hour-minute-second"
        timestamp = datetime.now().strftime("%d-%m-%Y_%H-%M-%S")
        fullFileName = os.path.join(args.datadir, f"{saveFileName} {timestamp}.avi")
        os.rename(os.path.join(args.datadir, "temp_video.avi"), fullFileName)
        os.startfile(fullFileName)

parser = ArgumentParser()
parser.add_argument("--cameraindex", type=int, 
                    help="specify the web camera index", 
                    default=0)

parser.add_argument("--datadir", type=str, 
                    help="specify the data directory", 
                    default=get_anonymous_directory())

args = parser.parse_args()

if wait_for_camera(args.cameraindex):
    cap = cv2.VideoCapture(args.cameraindex)
else:
    print("Could not initialize camera.")
    exit()
if cap.isOpened():
        init_writer(args.datadir, cap)
else:
    print("Could not initialize camera.")
    exit()
recording = True
processing_thread = threading.Thread(target=process_metrics, args=(cap,))
processing_thread.start()

while True:
    inputStr = input()
    if inputStr == 'q':
          recording = False
          time.sleep(1)
          exit()
    elif inputStr.startswith("save"):
        saveFileName = inputStr.split()[1]
        recording = False
        time.sleep(1)
        exit()
             
