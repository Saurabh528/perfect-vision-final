import mediapipe as mp

import cv2
import numpy as np
import math
from datetime import datetime
import time


import os
import sys

from argparse import ArgumentParser
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

parser.add_argument("--patientname", type=str, 
                    help="specify the patient name", 
                    default="")

args = parser.parse_args()
from unitysocket import init_TCP, send_command_to_unity, send_message_to_unity
    
# Initialize TCP connection
if args.connect:
    socket = init_TCP(args.port)
cur_dir = os.path.dirname(os.path.abspath(__file__))

data_dir = cur_dir
if args.patientname != "":
    data_dir = os.path.join(cur_dir, "PatientData/" + args.patientname)
if not os.path.exists(data_dir):
    os.makedirs(data_dir, exist_ok=True)

font = cv2.FONT_HERSHEY_SIMPLEX 
  
# org 
org = (50, 50) 
  
# fontScale 
fontScale = 1
   
# Blue color in BGR 
color = (255, 0, 0) 
  
# Line thickness of 2 px 
thickness = 2
# Open the file 'focus_value.txt', read its content, and print the value it contains
file_path = os.path.join(data_dir, 'focus_value.txt')  # Specify the file path
if not (os.path.exists(file_path) and os.path.isfile(file_path)):
    print(file_path + " does not exist.")
    if args.connect:
        send_message_to_unity("Please do screen distance callibration first.")
    sys.exit()
with open(file_path, 'r') as file:  # Open the file in read mode
    value = file.read().strip()  # Read the content and remove any leading/trailing whitespace

focus = value # Print the value
focus = int(float(focus))

iris_right_horzn = [469,471]
iris_right_vert = [470,472]
iris_left_horzn = [474,476]
iris_left_vert = [475,477]


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

connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
left_eyes_indices = get_unique(connections_left_eyes)

connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
right_eyes_indices = get_unique(connections_right_eyes)
cap = cv2.VideoCapture(args.cameraindex)
fps = cap.get(cv2.CAP_PROP_FPS)



def live_distance(results):
    for face_landmark in results.multi_face_landmarks:
        lms = face_landmark.landmark
        d= {}
        for index in iris_indices:
            x = int(lms[index].x*frame.shape[1])
            y = int(lms[index].y*frame.shape[0])
            d[index] = (x,y)
        black = np.zeros(frame.shape).astype("uint8")
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
        
        f = focus
        
        d = f*W/w
    return d


def calculate_conversion_rates(calibration_data):
    """
    Calculates width and height conversion rates for given calibration data.

    Parameters:
    - calibration_data: Dictionary with distances as keys and lists of points (tuples) as values.

    Returns:
    - A dictionary with distances as keys and tuples of (width_conversion_rate, height_conversion_rate) as values.
    """
    CARD_WIDTH_MM = 85.60
    CARD_HEIGHT_MM = 53.98

    def distance_between_points(point1, point2):
        """Calculate the Euclidean distance between two points."""
        x1, y1 = point1
        x2, y2 = point2
        distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)
        return distance

    def conversion(points):
        """Calculate width and height conversion rates based on the provided points."""
        # Assuming points are [(top-left), (top-right), (bottom-left), (bottom-right)]
        live_width = distance_between_points(points[0], points[1])
        live_height = distance_between_points(points[0], points[2])
        width_conversion_rate = CARD_WIDTH_MM / live_width
        height_conversion_rate = CARD_HEIGHT_MM / live_height
        return width_conversion_rate, height_conversion_rate

    # Apply the conversion function for each set of points stored in the calibration data
    conversion_rates = {distance: conversion(points) for distance, points in calibration_data.items()}

    return conversion_rates



points = []  # To store points selected by the user
distances = [40, 45, 50, 55]  # Specified distances for calibration
calibration_data = {}  # To store calibration points for each distance
current_distance_index = 0  # Index to track the current calibration step

# Function to handle mouse clicks
def select_point(event, x, y, flags, param):
    global points, frame
    if event == cv2.EVENT_LBUTTONDOWN:  # Double left click
        points.append((x, y))
        cv2.circle(frame, (x, y), 5, (0, 255, 0), -1)  # Show selected point
        cv2.imshow("Frame", frame)

# Function to display instructions
def display_instructions(img, distance):
    instructions = f"Sit at {distance}cm from the screen."
    cv2.putText(img, instructions, (20, 20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2, cv2.LINE_AA)
    cv2.putText(img, "Press 'p' when ready, then double-click the 4 corners of the card.", (20, 50), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2, cv2.LINE_AA)

# Main function
def run_callib():
    global points, current_distance_index, frame

    # Open the video camera
    cap = cv2.VideoCapture(args.cameraindex)

    cv2.namedWindow("Frame")
    cv2.setMouseCallback("Frame", select_point)

    while True:
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame")
            break

        display_instructions(frame, distances[current_distance_index])
        with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=2,
        refine_landmarks=True,
        min_detection_confidence=0.3) as face_mesh:
            results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
            try:
                distance = live_distance(results)
                frame = cv2.putText(frame, str(distance), (50,100), font,  
                   fontScale, color, thickness, cv2.LINE_AA)
            except Exception as e:
                print(e)
            

            

        cv2.imshow("Frame", frame)

        key = cv2.waitKey(1)

        if key & 0xFF == ord('p') and len(points) < 4:
            # Pause and show instructions to select points
            print("Select the 4 corners of the card...")
            while len(points) < 4:
                cv2.waitKey(50)

        if len(points) == 4:
            print(f"Points for distance {distances[current_distance_index]}cm: {points}")
            calibration_data[distances[current_distance_index]] = points.copy()
            cv2.imshow("Frame", frame)  # Show frame with points one last time
            cv2.waitKey(3000)  # Wait for 3 seconds to let the user see the points
            points = []  # Reset points for next distance
            current_distance_index += 1

            if current_distance_index == len(distances):
                break  # Exit after last distance

    cap.release()
    cv2.destroyAllWindows()

    # Optionally, save or process the calibration_data here
    print("Calibration Data:", calibration_data)

run_callib()

conv_rates = calculate_conversion_rates(calibration_data) 

filename = os.path.join(data_dir, 'conversion_rates.txt')
with open(filename, "w") as file:
    for distance, rates in conv_rates.items():
        line = f"Distance {distance}: Width Rate = {rates[0]}, Height Rate = {rates[1]}\n"
        file.write(line)
print(f"Conversion rates have been stored in {filename}")
