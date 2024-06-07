#!/usr/bin/env python
# coding: utf-8

# In[1]:


import cv2
import mediapipe as mp
import numpy as np
import time
import csv
import os
import math
import statistics
import cv2
import mediapipe as mp
import json
import time
import threading


def create_instruction_frame(frame, text, distance_text=''):
    # Create a white frame with the same resolution as the input frame
    instruction_frame = np.ones_like(frame) * 255
    
    # Place a red dot in the center of the frame
    height, width = instruction_frame.shape[:2]
    center = (width // 2, height // 2)
    cv2.circle(instruction_frame, center, 10, (0,0,255), -1)
    
    
    # Add the live distance text
    cv2.putText(instruction_frame, distance_text, (50, height - 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,0,0), 2)
    
    fontFace = cv2.FONT_HERSHEY_SIMPLEX
    fontScale = 1
    thickness = 2
    text_width, text_height = cv2.getTextSize(text, fontFace, fontScale, thickness)[0]
    CenterCoordinates = (int(width / 2) - int(text_width / 2), height // 2 - 30)
    # Add the instruction text
    cv2.putText(instruction_frame, text, CenterCoordinates, cv2.FONT_HERSHEY_SIMPLEX, 1, (0,0,0), 2)
    
    return instruction_frame

triggered = False
def on_data_received(data):
    global triggered
    # This function will be called when data is received by the client
    print(f"Callback function called with data: {data.decode('utf-8')}")
    # Process the received data here
    if data.decode('utf-8') == 'p':
        triggered = True
    
from argparse import ArgumentParser
from utils import get_anonymous_directory, wait_for_camera, append_to_log, speak

parser = ArgumentParser()

parser.add_argument("--connect", action="store_true",
                    help="connect to unity",
                    default=False)
                    
parser.add_argument("--quiet", action="store_true",
                    help="hide window",
                    default=False)

parser.add_argument("--port", type=int, 
                    help="specify the port of the connection to unity. Have to be the same as in Unity", 
                    default=5066)

parser.add_argument("--cameraindex", type=int, 
                    help="specify the web camera index", 
                    default=0)

parser.add_argument("--datadir", type=str, 
                    help="specify the data directory", 
                    default=get_anonymous_directory())

args = parser.parse_args()

import unitysocket
soc = []
# Initialize TCP connection
if args.connect:
    #soc = init_TCP(args.port)
    soc = unitysocket.start_connection(args.port, on_data_received)
    # Get the selector from the client module
    sel = unitysocket.get_selector()
if not os.path.exists(args.datadir):
    os.makedirs(args.datadir, exist_ok=True)


font = cv2.FONT_HERSHEY_SIMPLEX 
  
# org 
org = (50, 50) 
org1 = (50,100)
# fontScale 
fontScale = 0.7
   
# Blue color in BGR 
color = (255, 0, 0) 
  
# Line thickness of 2 px 
thickness = 2
   

def get_unique(c):
    temp_list = list(c)
    temp_set = set()
    for t in temp_list:
        temp_set.add(t[0])
        temp_set.add(t[1])
    return list(temp_set)

mp_face_mesh = mp.solutions.face_mesh
connections_iris = mp_face_mesh.FACEMESH_IRISES
iris_indices = get_unique(connections_iris)

mp_face_mesh = mp.solutions.face_mesh
connections_iris = mp_face_mesh.FACEMESH_IRISES
iris_indices = get_unique(connections_iris)
mp_face_detection = mp.solutions.face_detection



LEFT_EYE_INDICES = mp_face_mesh.FACEMESH_LEFT_EYE
RIGHT_EYE_INDICES = mp_face_mesh.FACEMESH_RIGHT_EYE
LEFT_IRIS_INDICES = mp_face_mesh.FACEMESH_LEFT_IRIS
LEFT_IRIS_INDICES = get_unique(LEFT_IRIS_INDICES)
RIGHT_IRIS_INDICES = mp_face_mesh.FACEMESH_RIGHT_IRIS
RIGHT_IRIS_INDICES = get_unique(RIGHT_IRIS_INDICES)
imp_indexes = LEFT_IRIS_INDICES + RIGHT_IRIS_INDICES
eyes_indices = [130, 133, 359, 362]
iris_right_horzn = [469,471]
iris_right_vert = [470,472]
iris_left_horzn = [474,476]
iris_left_vert = [475,477]


# In[2]:

def read_conversion_rates(filename):
    # Dictionaries to hold the conversion rates
    width_rates = {}
    height_rates = {}
    
    # Open and read the file
    with open(filename, 'r') as file:
        for line in file:
            # Remove newline and any leading/trailing whitespace
            line = line.strip()
            if line:  # Make sure the line isn't empty
                # Example line format: 'Distance 40: Width Rate = 1.4025, Height Rate = 0.4275'
                
                # Split on ': ' to isolate the distance part and the rates part
                distance_part, rates_part = line.split(': ', 1)
                
                # Extract the distance number
                distance = int(distance_part.split(' ')[1])
                
                # Further split the rates part on ', ' to separate width and height rates
                rates = rates_part.split(', ')
                
                # Extract and assign the width and height rates to their respective dictionaries
                width_rate = float(rates[0].split(' = ')[1])
                height_rate = float(rates[1].split(' = ')[1])
                
                width_rates[distance] = width_rate
                height_rates[distance] = height_rate

    return width_rates, height_rates

# Use the function to read the file
filename = os.path.join(args.datadir, 'conversion_rates.txt')  # Replace with the actual path
width_rates, height_rates = read_conversion_rates(filename)

conversion_rates = width_rates

def update_dev_pd(dist, IPD, distance_values):
    # Find the closest distance in the dictionary keys to the given dist
    closest_distance = min(distance_values.keys(), key=lambda x: abs(float(x) - dist))
    
    # Update IPD by multiplying with the corresponding value in the dictionary
    IPD *= distance_values[closest_distance]
    return IPD



def distance_between_points(point1, point2):
    """
    Calculate the Euclidean distance between two points.
    
    Args:
        point1 (tuple): Tuple containing (x, y) coordinates of the first point.
        point2 (tuple): Tuple containing (x, y) coordinates of the second point.
        
    Returns:
        float: Euclidean distance between the two points.
    """
    x1, y1 = point1
    x2, y2 = point2
    distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)
    return distance


# In[3]:


import cv2
import numpy as np
import time


wcr = 0.58

deviationsr=[]
deviationsl=[]
pdl=[]
pdr=[]
LELD =[]
LERD =[]
RERD=[]
RELD=[]
# Start video capture
calculation_started = False
if wait_for_camera(args.cameraindex):
    cap = cv2.VideoCapture(args.cameraindex)
else:
    print("Could not initialize camera.")
    exit()
distance_set = False
with mp_face_mesh.FaceMesh(
static_image_mode=True,
max_num_faces=2,
refine_landmarks=True,
min_detection_confidence=0.5) as face_mesh:
    while cap.isOpened():
        flag = 0
        ret, frame = cap.read()
        
        if not ret:
            break

        results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))

        try:
            for face_landmark in results.multi_face_landmarks:
                lms = face_landmark.landmark
                d= {}
                for index in iris_indices:
                    x = int(lms[index].x*frame.shape[1])
                    y = int(lms[index].y*frame.shape[0])
                    d[index] = (x,y)
                for index in iris_indices:
                    #print(index)
                    cv2.circle(frame,(d[index][0],d[index][1]),2,(0,255,0),-1)
                
                
                centre_right_iris_x_1 = int((d[iris_right_horzn[0]][0] + d[iris_right_horzn[1]][0])/2)
                centre_right_iris_y_1 = int((d[iris_right_horzn[0]][1] + d[iris_right_horzn[1]][1])/2)
                
                centre_right_iris_x_2 = int((d[iris_right_vert[0]][0] + d[iris_right_vert[1]][0])/2)
                centre_right_iris_y_2 = int((d[iris_right_vert[0]][1] + d[iris_right_vert[1]][1])/2)
                
                    
                centre_left_iris_x_1 = int((d[iris_left_horzn[0]][0] + d[iris_left_horzn[1]][0])/2)
                centre_left_iris_y_1 = int((d[iris_left_horzn[0]][1] + d[iris_left_horzn[1]][1])/2)
                
                centre_left_iris_x_2 = int((d[iris_left_vert[0]][0] + d[iris_left_vert[1]][0])/2)
                centre_left_iris_y_2 = int((d[iris_left_vert[0]][1] + d[iris_left_vert[1]][1])/2)
                
                centre_left_iris_x = int((centre_left_iris_x_1 + centre_left_iris_x_2)/2)
                centre_left_iris_y = int((centre_left_iris_y_1 + centre_left_iris_y_2)/2)
                
                centre_right_iris_x = int((centre_right_iris_x_1 + centre_right_iris_x_2)/2)
                centre_right_iris_y = int((centre_right_iris_y_1 + centre_right_iris_y_2)/2)
                
                cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
                cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1)
                
                w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                
                W = 6.3
                
                f = 654
            
                dist = f*W/w
                
                frame = cv2.putText(frame, f"{dist:.2f}", org, font,  
                    fontScale, color, thickness, cv2.LINE_AA)
                
                

                if not distance_set:
                    """ frame = cv2.putText(frame, f"{dist:.2f}", org, font,  
                    fontScale, color, thickness, cv2.LINE_AA)
                    text = "Sit at a distance of 40 cms and press p"
                    frame = cv2.putText(frame, text, org1, font,  
                    fontScale, color, thickness, cv2.LINE_AA)
                    cv2.imshow("final", frame) """
                    unitysocket.send_status_to_unity(soc, f"Distance: {int(dist)}cm")
                    
                    if args.connect:
                        events = sel.select(timeout=1)
                        if events:
                            for key, mask in events:
                                unitysocket.service_connection(key, mask)
                        if not sel.get_map():
                            break
                    if cv2.waitKey(1) & 0xFF == ord('p'):
                        triggered = True
                    if triggered:
                        distance_set = True
                        #if args.quiet:
                            #cv2.destroyWindow("final")
                        if args.connect:
                            if not soc:
                                print("soc is invalid.")
                            else:
                                time.sleep(0.1)
                                unitysocket.send_command_to_unity(soc, "SHOWPOINT")


                else:
                    frame_with_text = create_instruction_frame(frame, 'Starting Test')
                    h, w, _ = frame.shape
                    for round_ in range(5):
                        for eye in ['left', 'right']:
                            instruction_shown = False
                            start_time = time.time()
                                        
                            while True:
                                ret,frame = cap.read()
                                if not ret:
                                    break

                                results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
                                flag = 0 
                                for face_landmark in results.multi_face_landmarks:
                                
                                    left_pupil_center = np.array([face_landmark.landmark[468].x, face_landmark.landmark[468].y]) 
                                    right_pupil_center = np.array([face_landmark.landmark[473].x, face_landmark.landmark[473].y])
                                    #print(left_pupil_center)
                                    current_time = time.time()
                                    elapsed_time = current_time - start_time
                                    #print(elapsed_time)
                                    if eye == 'left':
                                        instruction_text = 'Right Eye'
                                    else:
                                        instruction_text = 'Left Eye'
                                    if elapsed_time > 5:
                                        flag = 1
                                        if args.connect:
                                            unitysocket.send_message_to_unity(soc, instruction_text)
                                        speak_thread = threading.Thread(target=speak, args=(args.datadir, instruction_text,))
                                        speak_thread.start()
                                        break
                                    
                                    #instruction_text = f'Close {eye} eye, gaze with the other'

                                    if elapsed_time > 3.5 and calculation_started:

                                        if eye == 'right':
                                            left_eye_point1 = np.array([face_landmark.landmark[130].x, face_landmark.landmark[130].y])
                                            left_eye_point2 = np.array([face_landmark.landmark[133].x, face_landmark.landmark[133].y])
                                            point=(left_eye_point1+left_eye_point2)/2
                                            point1=((point[0] * w), (point[1] * h))
                                            point2=((left_pupil_center[0] * w), (left_pupil_center[1] * h))
                                            LELD.append(((left_eye_point1[0]*w-point2[0]*w)**2+(left_eye_point1[1]*h-point2[1]*h)**2)**.5)
                                            LERD.append(((left_eye_point2[0]*w-point2[0]*w)**2+(left_eye_point2[1]*h-point2[1]*h)**2)**.5) 
                                            dev_pixels = abs(point1[0]-point2[0])
                                            dev_pd = dev_pixels
                                            new_dev = update_dev_pd(float(dist),dev_pd,conversion_rates)
                                            deviationsr.append(new_dev)
                                            pdr.append(new_dev*15)
                                        else: 
                                            right_eye_point1 = np.array([face_landmark.landmark[359].x, face_landmark.landmark[359].y])
                                            right_eye_point2 = np.array([face_landmark.landmark[362].x, face_landmark.landmark[362].y])
                                            point=(right_eye_point1+right_eye_point2)/2 # Index for the right eye corner.
                                            point1=((point[0] * w), (point[1] * h))
                                            point2= ((right_pupil_center[0] * w), (right_pupil_center[1] * h))
                                            RERD.append(((right_eye_point1[0]*w-point2[0]*w)**2+(right_eye_point1[1]*h-point2[1]*h)**2)**.5)
                                            RELD.append(((right_eye_point2[0]*w-point2[0]*w)**2+(right_eye_point2[1]*h-point2[1]*h)**2)**.5)
                                            dev_pixels = abs(point1[0]-point2[0])
                                            dev_pd = dev_pixels
                                            new_dev = update_dev_pd(float(dist),dev_pd,conversion_rates)
                                            deviationsl.append(new_dev)
                                            pdl.append(new_dev*15)
                                        break
                                        
                        
                                    
                                    
                                    if eye == 'right':
                                        calculation_started = True
                                    
                                    frame_with_text = create_instruction_frame(frame, instruction_text, f"{dist:.2f}")
                                    if not args.quiet:
                                        cv2.imshow('Cover Uncover Test', frame_with_text)        
                                    time.sleep(1)
                            
                                if flag == 1:
                                    break
                                    


                            
                                    
                            if cv2.waitKey(1) & 0xFF == 27: # Esc key to exit
                                cap.release()
                                cv2.destroyAllWindows()
                                exit()
                            
                            # Wait for 1 second to allow the user to shift their gaze
                            
                            
                    cap.release()
                    cv2.destroyAllWindows()

        except Exception as e:
            print(e)


# In[14]:
print("checking finished.")

# Example list of numbers

# Calculate mean
mean_value1 = round(statistics.mean(pdr),2)

# Calculate standard deviation
std_dev1 = round(statistics.stdev(pdr),2)

print("Mean:", mean_value1)
print("Standard Deviation:", std_dev1)


# In[15]:


mean_value2 = round(statistics.mean(pdl),2)

# Calculate standard deviation
std_dev2 = round(statistics.stdev(pdl),2)

print("Mean:", mean_value2)
print("Standard Deviation:", std_dev2)


# In[11]:


text_to_save = f"Eye 1: Mean Value = {mean_value1}, Standard Deviation = {std_dev1}\nEye 2: Mean Value = {mean_value2}, Standard Deviation = {std_dev2}"

# File path where the text will be saved
file_path = os.path.join(args.datadir, 'eye_statistics.txt')  # You can specify a different path or filename

# Writing to the file
with open(file_path, "w") as file:
    file.write(text_to_save)

print(f"Data has been written to {file_path}")
if args.connect:
    soc.close()