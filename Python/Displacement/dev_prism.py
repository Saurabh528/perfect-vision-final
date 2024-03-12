# #!/usr/bin/env python
# # coding: utf-8

# # In[3]:

# from argparse import ArgumentParser

# import mediapipe as mp
# import cv2
# import numpy as np
# import time
# import pandas as pd
# import os
# import sys
# # for TCP connection with unity
# import socket
# # global variable
# port = 5066         # have to be same as unity

# # init TCP connection with unity
# # return the socket connected
# def init_TCP():
#     port = args.port

#     # '127.0.0.1' = 'localhost' = your computer internal data transmission IP
#     address = ('127.0.0.1', port)
#     # address = ('192.168.0.107', port)

#     try:
#         s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#         s.connect(address)
#         # print(socket.gethostbyname(socket.gethostname()) + "::" + str(port))
#         print("Connected to address:", socket.gethostbyname(socket.gethostname()) + ":" + str(port))
#         return s
#     except OSError as e:
#         print("Error while connecting :: %s" % e)
        
#         # quit the script if connection fails (e.g. Unity server side quits suddenly)
#         sys.exit()

# def send_command_to_unity(s, strarg):
#     msg = 'CMD:' + strarg

#     try:
#         s.send(bytes(msg, "utf-8"))
#     except socket.error as e:
#         print("error while sending :: " + str(e))

#         # quit the script if connection fails (e.g. Unity server side quits suddenly)
#         sys.exit()
# def send_message_to_unity(s, strarg):
#     msg = 'MSG:' + strarg

#     try:
#         s.send(bytes(msg, "utf-8"))
#     except socket.error as e:
#         print("error while sending :: " + str(e))

#         # quit the script if connection fails (e.g. Unity server side quits suddenly)
#         sys.exit()


# def send_status_to_unity(s, strarg):
#     msg = 'STS:' + strarg

#     try:
#         s.send(bytes(msg, "utf-8"))
#     except socket.error as e:
#         print("error while sending :: " + str(e))

#         # quit the script if connection fails (e.g. Unity server side quits suddenly)
#         sys.exit()

# # In[4]:


# def get_unique(c):
#     temp_list = list(c)
#     temp_set = set()
#     for t in temp_list:
#         temp_set.add(t[0])
#         temp_set.add(t[1])
#     return list(temp_set)


# # In[5]:


# mp_face_mesh = mp.solutions.face_mesh
# connections_iris = mp_face_mesh.FACEMESH_IRISES
# iris_indices = get_unique(connections_iris)

# connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
# left_eyes_indices = get_unique(connections_left_eyes)

# connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
# right_eyes_indices = get_unique(connections_right_eyes)

# # In[6]:


# print(left_eyes_indices)


# # In[7]:


# iris_right_horzn = [469,471]
# iris_right_vert = [470,472]
# iris_left_horzn = [474,476]
# iris_left_vert = [475,477]



# # In[8]:



# # In[9]:

# # In[ ]:





# # In[20]:

# def main():
#     # Initialize TCP connection
#     if args.connect:
#         socket = init_TCP()
#     # 3 second delay
#     time.sleep(0.5)
#     with open('./Python/DPI.txt', 'r') as f:
#         lines = f.readlines()
#     patientName = lines[0].strip()
#     print('patientName: ' + patientName)
#     dpi = float(lines[1])
#     print('DPI: ' + str(dpi))
#     focus = 800.0
#     try:
#         with open('./Python/' + patientName + '/focus_final.csv', 'r') as f:
#             lines = f.readlines()
#         focus = float(lines[1].split(',')[1])
#     except FileNotFoundError:
#         if args.connect:
#             send_message_to_unity(socket, 'Please do Screen Distance checking first')
#             time.sleep(3)
#             send_command_to_unity(socket, 'EXIT')
#         else:
#             print('Please do Screen Distance checking first')
#         sys.exit()
#     print('Focus: ' + str(focus))
# # convert pixel to millimeter, this depends on your specific camera


#     cap = cv2.VideoCapture(args.cameraindex)
#     fps = cap.get(cv2.CAP_PROP_FPS)
#     print('FPS: ' + str(fps))

    
#     if args.connect:
#         send_message_to_unity(socket, 'This test will take 5 rounds of alternate covering of eyes and\n finally will result the deviation in each eyes.')
#     else:    
#         print("This test will take 5 rounds of alternate covering of eyes and finally will result the deviation in each eyes.")
#     time.sleep(5)    
#        # Show Point
#     if args.connect:
#         send_command_to_unity(socket, 'SHOWPOINT')
#         time.sleep(1)
    
#     # Before measurement
#     if args.connect:
#         send_message_to_unity(socket, 'Fixate on one point in the screen')
#     else:    
#         print("Fixate on one point in the screen")
#     time.sleep(0.5)

#     dpmm = float(dpi/25.4)
#     pixel_to_mm = 1/dpmm

#     disp_left = []
#     disp_right = []
#     start_time = time.time()
#     alternating_eye = True  # Start with the left eye
#     switch_time = time.time() + 5
#     current_time = time.time()
#     cover_test_rounds = 0
#     with mp_face_mesh.FaceMesh(
#         static_image_mode=True,
#         max_num_faces=2,
#         refine_landmarks=True,
#         min_detection_confidence=0.5) as face_mesh:

#         while cap.isOpened() and cover_test_rounds < 10:
#             flag = 0
#             #if (time.time() - start_time) > float(10):
#             #    break
#             ret, frame = cap.read()
#             if not ret:
#                 break

#             results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
#             if results.multi_face_landmarks is None:
#                 break
#             if current_time >= switch_time:
#                 alternating_eye = not alternating_eye
#                 switch_time = current_time + 5
#                 cover_test_rounds += 1
#                 print(cover_test_rounds)
#                 if alternating_eye:
#                     if args.connect:
#                         send_message_to_unity(socket, 'Cover your right eye now')
#                     else:
#                         print("Cover your right eye now")
#                 else:
#                     if args.connect:
#                         send_message_to_unity(socket, 'Cover your left eye now')
#                     else:
#                         print("Cover your left eye now")

#             try:
#                 for face_landmark in results.multi_face_landmarks:
#                     lms = face_landmark.landmark
#                     d= {}
#                     for index in iris_indices:
#                         x = int(lms[index].x*frame.shape[1])
#                         y = int(lms[index].y*frame.shape[0])
#                         d[index] = (x,y)
#                     black = np.zeros(frame.shape).astype("uint8")
#                     for index in iris_indices:
#                         #print(index)
#                         cv2.circle(frame,(d[index][0],d[index][1]),2,(0,255,0),-1)
                    
                    
#                     centre_right_iris_x_1 = int((d[iris_right_horzn[0]][0] + d[iris_right_horzn[1]][0])/2)
#                     centre_right_iris_y_1 = int((d[iris_right_horzn[0]][1] + d[iris_right_horzn[1]][1])/2)
                    
#                     centre_right_iris_x_2 = int((d[iris_right_vert[0]][0] + d[iris_right_vert[1]][0])/2)
#                     centre_right_iris_y_2 = int((d[iris_right_vert[0]][1] + d[iris_right_vert[1]][1])/2)
                    
                        
#                     centre_left_iris_x_1 = int((d[iris_left_horzn[0]][0] + d[iris_left_horzn[1]][0])/2)
#                     centre_left_iris_y_1 = int((d[iris_left_horzn[0]][1] + d[iris_left_horzn[1]][1])/2)
                    
#                     centre_left_iris_x_2 = int((d[iris_left_vert[0]][0] + d[iris_left_vert[1]][0])/2)
#                     centre_left_iris_y_2 = int((d[iris_left_vert[0]][1] + d[iris_left_vert[1]][1])/2)
                    
#                     centre_left_iris_x = int((centre_left_iris_x_1 + centre_left_iris_x_2)/2)
#                     centre_left_iris_y = int((centre_left_iris_y_1 + centre_left_iris_y_2)/2)
                    
#                     centre_right_iris_x = int((centre_right_iris_x_1 + centre_right_iris_x_2)/2)
#                     centre_right_iris_y = int((centre_right_iris_y_1 + centre_right_iris_y_2)/2)
                    
                    
#                     cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
#                     cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1)
                    
#                     cv2.circle(black,(centre_right_iris_x,centre_right_iris_y),2,(0,0,255),-1)
#                     cv2.circle(black,(centre_left_iris_x,centre_left_iris_y),2,(0,0,255),-1)
                    
#                     w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                    
#                     W = 6.3
#                     distance_to_screen = (focus * W)/w
                    

#                     # Existing code here...

#                     e= {}
#                     sum_left_eye_x, sum_left_eye_y = 0, 0
#                     for index in left_eyes_indices:
#                         x = int(lms[index].x*frame.shape[1])
#                         y = int(lms[index].y*frame.shape[0])
#                         e[index] = (x,y)

#                         if index in [263, 398, 386, 374]:
#                             cv2.circle(frame,(e[index][0],e[index][1]),2,(0,255,0),-1)
#                             cv2.circle(black,(e[index][0],e[index][1]),2,(0,0,255),-1)
#                             sum_left_eye_x += x
#                             sum_left_eye_y += y

#                     f= {}
#                     sum_right_eye_x, sum_right_eye_y = 0, 0
#                     for index in right_eyes_indices:
#                         x = int(lms[index].x*frame.shape[1])
#                         y = int(lms[index].y*frame.shape[0])
#                         f[index] = (x,y)

#                         if index in [33, 133, 145, 159]:
#                             cv2.circle(frame,(f[index][0],f[index][1]),2,(0,255,0),-1)
#                             cv2.circle(black,(f[index][0],f[index][1]),2,(0,0,255),-1)
#                             sum_right_eye_x += x
#                             sum_right_eye_y += y
                    
#                     centre_left_eye_x = int(sum_left_eye_x / 4)
#                     centre_left_eye_y = int(sum_left_eye_y / 4)
#                     cv2.circle(frame, (centre_left_eye_x, centre_left_eye_y), 2, (0, 0, 255), -1)

#                     centre_right_eye_x = int(sum_right_eye_x / 4)
#                     centre_right_eye_y = int(sum_right_eye_y / 4)
#                     cv2.circle(frame, (centre_right_eye_x, centre_right_eye_y), 2, (0, 0, 255), -1)
#                     current_time = time.time()
                    

#                     if alternating_eye: 
#                         # Calculate displacement for right eye in pixels
#                         displacement_right = np.sqrt((centre_right_iris_x - centre_right_eye_x)**2 + 
#                                                     (centre_right_iris_y - centre_right_eye_y)**2)

#                         # Convert pixel displacement to mm
#                         displacement_right_mm = displacement_right * pixel_to_mm

#                         # Calculate displacement in prism diopters
#                         displacement_right_pd = displacement_right_mm / (distance_to_screen * 1000)
#                         disp_right.append(displacement_right_pd)
#                         statusStr = f"Right eye displacement: {displacement_right_pd:.2e} PD"
#                         cv2.putText(frame, statusStr,
#                                             (50, 80), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 0), 2)
                    
#                     else:  
                        
#                         displacement_left = np.sqrt((centre_left_iris_x - centre_left_eye_x)**2 + 
#                                                     (centre_left_iris_y - centre_left_eye_y)**2)

#                         # Convert pixel displacement to mm
#                         displacement_left_mm = displacement_left * pixel_to_mm

#                         # Calculate displacement in prism diopters
#                         displacement_left_pd = displacement_left_mm / (distance_to_screen * 1000)
#                         disp_left.append(displacement_left_pd)
#                         statusStr = f"Left eye displacement: {displacement_left_pd:.2e} PD"
#                         cv2.putText(frame, statusStr,
#                                             (50, 80), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 0), 2)
#                     send_status_to_unity(socket, statusStr)
#                     if not args.quiet:
#                         cv2.imshow("final", frame)
#                     #print(cover_test_rounds)
#                     if cv2.waitKey(1) & 0xFF == ord('q'):
#                         flag = 1
#                         break
#             except Exception as e:
#                 print(e)

#             if flag == 1:
#                 break


#     cap.release()
#     cv2.destroyAllWindows()

#     average_disp_right = sum(disp_right) / len(disp_right)
#     average_disp_left = sum(disp_left) / len(disp_left)

       
#     df = pd.DataFrame({ 'displacement_right' : [average_disp_right],
#                       'displacement_left' : [average_disp_left]})

#     df_right = pd.DataFrame({ 'displacement_right' : disp_right})
#     df_left = pd.DataFrame({ 'displacement_left' : disp_left})

#     # In[23]:


#     df.to_csv('./Python/' + patientName + '/alternate_test.csv')
#     df.to_csv('./Python/' + patientName + '/alternate_test.csv')
#     df_left.to_csv('./Python/' + patientName + '/data_left_displacement.csv')
#     df_right.to_csv('./Python/' + patientName + '/data_right_displacement.csv')
#      # Exit
#     if args.connect:
#         send_command_to_unity(socket, 'EXIT')

# if __name__ == "__main__":

#     parser = ArgumentParser()

#     parser.add_argument("--connect", action="store_true",
#                         help="connect to unity",
#                         default=False)
                        
#     parser.add_argument("--quiet", action="store_true",
#                         help="hide window",
#                         default=False)

#     parser.add_argument("--port", type=int, 
#                         help="specify the port of the connection to unity. Have to be the same as in Unity", 
#                         default=5066)
    
#     parser.add_argument("--cameraindex", type=int, 
#                         help="specify the web camera index", 
#                         default=0)

#     args = parser.parse_args()

#     # demo code
#     main()
# # In[ ]:




# -*- coding: utf-8 -*-
"""papercalculation.ipynb

Automatically generated by Colaboratory.

Original file is located at
    https://colab.research.google.com/drive/1JeSJLCYgN6Bku7qwvkk61bq_ZYLixay0
"""

from argparse import ArgumentParser
import os
import sys

# for TCP connection with unity
import socket
# global variable
port = 5066         # have to be same as unity
args = None
sock = None
# init TCP connection with unity
# return the socket connected
def init_TCP():
    port = args.port

    # '127.0.0.1' = 'localhost' = your computer internal data transmission IP
    address = ('127.0.0.1', port)
    # address = ('192.168.0.107', port)

    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect(address)
        # print(socket.gethostbyname(socket.gethostname()) + "::" + str(port))
        print("Connected to address:", socket.gethostbyname(socket.gethostname()) + ":" + str(port))
        return s
    except OSError as e:
        print("Error while connecting :: %s" % e)
        
        # quit the script if connection fails (e.g. Unity server side quits suddenly)
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
        print("error while sending :: " + str(e))

        # quit the script if connection fails (e.g. Unity server side quits suddenly)
        sys.exit()

def send_status_to_unity(s, strarg):
    msg = 'STS:' + strarg

    try:
        s.send(bytes(msg, "utf-8"))
    except socket.error as e:
        print("error while sending :: " + str(e))

        # quit the script if connection fails (e.g. Unity server side quits suddenly)
        sys.exit()





def get_face_roi(landmarks, image):
    """
    Determine the region of interest of the face based on landmarks.
    """
    # Get the bounding box coordinates
    x_coordinates = [int(landmark.x * image.shape[1]) for landmark in landmarks]
    y_coordinates = [int(landmark.y * image.shape[0]) for landmark in landmarks]
    x_min, x_max = min(x_coordinates), max(x_coordinates)
    y_min, y_max = min(y_coordinates), max(y_coordinates)
    return x_min, y_min, x_max, y_max
import math
def calculate_rotation_angle(landmarks, image):
    """
    Calculate the rotation angle of the face based on eye landmarks.
    """
    # Define eye landmarks (indices may vary based on MediaPipe's output format)
    left_eye = landmarks[33]  # Example index for left eye
    right_eye = landmarks[263] # Example index for right eye
    # Calculate angle
    eye_line = [int(right_eye.x * image.shape[1]) - int(left_eye.x * image.shape[1]),
                int(right_eye.y * image.shape[0]) - int(left_eye.y * image.shape[0])]
    angle = math.atan2(eye_line[1], eye_line[0])
    return math.degrees(angle)
def rotate_image(image, angle, center=None, scale=1.0):
    """
    Rotate the image by a given angle.
    """
    (h, w) = image.shape[:2]
    if center is None:
        center = (w // 2, h // 2)
    # Perform the rotation
    M = cv2.getRotationMatrix2D(center, angle, scale)
    rotated = cv2.warpAffine(image, M, (w, h))
    return rotated

def perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle):
    """
    Apply perspective transform and rotation to the face region.
    """
    # Rotate the image first
    rotated_image = rotate_image(image, rotation_angle)
    # Updated points for perspective transform
    points1 = np.float32([[x_min, y_min], [x_max, y_min], [x_min, y_max], [x_max, y_max]])
    points2 = np.float32([[0, 0], [500, 0], [0, 500], [500, 500]])
    matrix = cv2.getPerspectiveTransform(points1, points2)
    return cv2.warpPerspective(rotated_image, matrix, (500, 500))
# OpenCV code to read and process the video frame

import cv2
import mediapipe as mp
import numpy as np
import time
import csv



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

pos_similarities = []
for i in eyes_indices:
    imp_indexes.append(i)

# Function to calculate distance.
def calculate_distance(p1, p2):
    return np.linalg.norm(np.array(p1) - np.array(p2))

# Function to calculate prism diopters from pixel distance.
def calculate_prism_diopters(distance_pixels):
    adult_limb_diameter_mm = 11  # Adult limb diameter in millimeters.
    pixelMM = wcr   #for 500 mm distance
    DHmm = distance_pixels * pixelMM  # Convert pixel distance to mm.
    DP = 15  # Constant to convert to prism diopters.
    prism_diopters = DP * DHmm
    return prism_diopters

def calculate_screen_distance(landmarks, frame_width, frame_height):
    # Assuming an average inter-ocular distance of 6.5 cm (65 mm)
    average_inter_ocular_distance = 6.3  # in millimeters

    # Landmark indices for the pupils
    left_pupil_index = 468  # Adjust as needed
    right_pupil_index = 473  # Adjust as needed

    left_pupil = None
    right_pupil = None

    # Iterate through landmarks to find the pupils
    for i, landmark in enumerate(landmarks.landmark):
        if i == left_pupil_index:
            left_pupil = landmark
        elif i == right_pupil_index:
            right_pupil = landmark

    if left_pupil is None or right_pupil is None:
        return None  # Pupils not found

    # Convert to pixel coordinates
    left_pupil_x = int(left_pupil.x * frame_width)
    left_pupil_y = int(left_pupil.y * frame_height)
    right_pupil_x = int(right_pupil.x * frame_width)
    right_pupil_y = int(right_pupil.y * frame_height)

    # Calculate the pixel distance between the pupils
    pixel_distance = ((right_pupil_x - left_pupil_x) ** 2 + (right_pupil_y - left_pupil_y) ** 2) ** 0.5

    # Camera's horizontal FOV (assumed)
    camera_fov = 60  # in degrees

    # Calculate the perceived width of the face (in millimeters)
    perceived_width = 2 * (average_inter_ocular_distance / 2) / np.tan(np.radians(camera_fov / 2))

    # Scale factor
    scale_factor = perceived_width / pixel_distance

    # Real width of the face (assumed)
    real_face_width = 14  # in millimeters

    # Calculate the screen distance
    screen_distance = (real_face_width * scale_factor) / 2

    return screen_distance * 100  # Convert to centimeters

deviationsr=[]
deviationsl=[]
pdl=[]
pdr=[]
LELD =[]
LERD =[]
RERD=[]
RELD=[]

patientName = "Anonymous"
with open('./Python/DPI.txt', 'r') as f:
    lines = f.readlines()
    patientName = lines[0].strip()
    print('patientName: ' + patientName)


def process_video(video_path, num_rounds=5):
    global sock, wcr
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        send_message_to_unity(sock, "Failed to connect camera.")
        time.sleep(2)
        send_command_to_unity(sock, "FAIL")
        time.sleep(0.5)
        send_command_to_unity(sock, "EXIT")
    # send_message_to_unity(sock, "Track the red point on the screen.")
        send_message_to_unity(sock, "FOCUS ON GREEN DOT")
    time.sleep(1)
    send_message_to_unity(sock, "")
    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True,
        min_detection_confidence=0.3) as face_mesh:
        start_time = time.time()
        while time.time() - start_time < 10:
            success, image = cap.read()
            if not success:
                break

            h, w, _ = image.shape
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = face_mesh.process(image_rgb)

            screen_distance_text = "Adjusting..."
            if results.multi_face_landmarks:
                for face_landmarks in results.multi_face_landmarks:
                    rotation_angle = calculate_rotation_angle(face_landmarks.landmark, image)
                    x_min, y_min, x_max, y_max = get_face_roi(face_landmarks.landmark, image)
                    image = perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle)
                    screen_distance = calculate_screen_distance(face_landmarks, w, h)
                    screen_distance_text = f"Screen Distance: {screen_distance:.2f} cm"
            if args.connect:
                send_status_to_unity(sock, screen_distance_text)
            else:
                cv2.putText(image, screen_distance_text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            if not args.quiet:
                cv2.imshow("Adjust Position", image)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
        
        send_command_to_unity(sock, 'START')
        time.sleep(3)

        for _ in range(num_rounds):
            for eye in ['left', 'right']:

                instruction = f"Close{eye}"
                instructionText = f"Close your {eye} eye and focus with the other"
                send_command_to_unity(sock, instruction)
                # Draw a fixed point on the screen center
                # Assuming the screen resolution is known (e.g., 1280x720)
                  # Replace with actual screen center coordinates

                time.sleep(5)  # Wait for the user to fix their head
                for t in [0, 3]:
                    time.sleep(t)
                    success, image = cap.read()
                    if not success:
                        break

                    h, w, _ = image.shape
                    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                    results = face_mesh.process(image_rgb)

                    if results.multi_face_landmarks:
                        for face_landmarks in results.multi_face_landmarks:
                            rotation_angle = calculate_rotation_angle(face_landmarks.landmark, image)
                            x_min, y_min, x_max, y_max = get_face_roi(face_landmarks.landmark, image)

                            image = perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle)
                            screen_distance = calculate_screen_distance(face_landmarks, w, h)
                            screen_distance_text = f"Screen Distance: {screen_distance:.2f} cm"
                            if args.connect:
                                send_status_to_unity(sock, screen_distance_text)
                            else:
                                cv2.putText(image, screen_distance_text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
                            # Example indices, adjust as needed
                            imp_indexes = [469, 471]
                            points = []
                            left_pupil_center = np.array([face_landmarks.landmark[468].x, face_landmarks.landmark[468].y])
                            right_pupil_center = np.array([face_landmarks.landmark[473].x, face_landmarks.landmark[473].y])
                            left_eye_w = calculate_distance((int(face_landmarks.landmark[133].x * w), int(face_landmarks.landmark[133].y * h)), (int(face_landmarks.landmark[130].x * w), int(face_landmarks.landmark[130].y * h)))
                            right_eye_w = calculate_distance((int(face_landmarks.landmark[362].x * w), int(face_landmarks.landmark[362].y * h)), (int(face_landmarks.landmark[359].x * w), int(face_landmarks.landmark[359].y * h)))

                            # Extracting the landmarks for the eye that is open (right or left).
                            if eye == 'right':
                                right_eye_point1 = np.array([face_landmarks.landmark[359].x, face_landmarks.landmark[359].y])
                                right_eye_point2 = np.array([face_landmarks.landmark[362].x, face_landmarks.landmark[362].y])
                                point=(right_eye_point1+right_eye_point2)/2 # Index for the right eye corner.
                                point1=((point[0] * w), (point[1] * h))
                                point2= ((right_pupil_center[0] * w), (right_pupil_center[1] * h))
                                RERD.append(((right_eye_point1[0]*w-point2[0]*w)**2+(right_eye_point1[1]*h-point2[1]*h)**2)**.5)
                                RELD.append(((right_eye_point2[0]*w-point2[0]*w)**2+(right_eye_point2[1]*h-point2[1]*h)**2)**.5)
                            
                            else:
                                left_eye_point1 = np.array([face_landmarks.landmark[130].x, face_landmarks.landmark[130].y])
                                left_eye_point2 = np.array([face_landmarks.landmark[133].x, face_landmarks.landmark[133].y])
                                point=(left_eye_point1+left_eye_point2)/2
                                point1=((point[0] * w), (point[1] * h))
                                point2=((left_pupil_center[0] * w), (left_pupil_center[1] * h))
                                LELD.append(((left_eye_point1[0]*w-point2[0]*w)**2+(left_eye_point1[1]*h-point2[1]*h)**2)**.5)
                                LERD.append(((left_eye_point2[0]*w-point2[0]*w)**2+(left_eye_point2[1]*h-point2[1]*h)**2)**.5) 

                            # Calculate the deviation (distance in pixels from fixed point).
                            deviation_pixels = abs(point1[0]-point2[0])
                            if eye == 'right':
                                deviationsr.append(deviation_pixels*wcr)
                            else:
                                deviationsl.append(deviation_pixels*wcr)

                            print(f"deviation in pixels {deviation_pixels}")
                            p=calculate_prism_diopters(deviation_pixels)
                            if eye == 'right':
                                pdl.append(p)
                            else:
                                pdr.append(p)
                            print(f"p: {p}")
                    else:
                        if eye == 'right':
                            if len(RERD) > 0:
                                RERD.append(RERD[-1])
                            else:
                                RERD.append(np.array([0, 0]))
                            if len(RELD) > 0:
                                RELD.append(RELD[-1])
                            else:
                                RELD.append(np.array([0, 0]))
                        else:
                            if len(LERD) > 0:
                                LERD.append(LERD[-1])
                            else:
                                LERD.append(np.array([0, 0]))
                            if len(LELD) > 0:
                                LELD.append(LELD[-1])
                            else:
                                LELD.append(np.array([0, 0]))
                    h, w, _ = image.shape
                    screen_center = (w // 2, h // 2)
                    
                    cv2.putText(image, instructionText, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
                    cv2.circle(image, screen_center, 5, (0, 255, 0), -1)
                    if not args.quiet:
                        cv2.imshow("Processed Video", image)

                    if cv2.waitKey(5) & 0xFF == ord('q'):
                        break

        cap.release()
        cv2.destroyAllWindows()
        # connect to unity. pdr[] pdl[]
        # Calculate the mean of deviations after the rounds
        mean_deviationr = np.mean(deviationsr)
        mean_deviationl = np.mean(deviationsl)
        print(f"Mean Deviation of right eye: {mean_deviationr}")
        print(f"Mean Deviation of left eye: {mean_deviationl}")
        return mean_deviationr,mean_deviationl


def main():
    global sock, wcr
    # Initialize TCP connection
    if args.connect:
        sock = init_TCP()
    try:
        script_dir = os.path.dirname(__file__)
        file_path = os.path.join(script_dir, ".." , "ScreenCali" , 'cv.txt')
        with open(file_path, 'r') as file:
            wcr = float(file.readline())
    except FileNotFoundError:
        if args.connect:
            send_message_to_unity(sock, 'Please do Screen Distance checking first')
            time.sleep(3)
            send_command_to_unity(sock, 'EXIT')
        else:
            print('Please do Screen Distance checking first')
    time.sleep(0.5)
    # Example usage with webcam
    process_video(args.cameraindex)

    # np.savetxt('H:\Vpower2\perfect-vision\Python\ScreenCali\' + patientName' + 'RELD.txt', RELD)
    # np.savetxt('H:\Vpower2\perfect-vision\Python\ScreenCali\' + patientName' + 'RERD.txt', RERD)
    # np.savetxt('H:\Vpower2\perfect-vision\Python\ScreenCali\' + patientName' + 'LELD.txt', LELD)
    # np.savetxt('H:\Vpower2\perfect-vision\Python\ScreenCali\' + patientName' + 'LERD.txt', LERD)
    file_path = os.path.join(script_dir, ".." , "ScreenCali" )
    np.savetxt(file_path + 'RELD.txt', RELD)
    np.savetxt(file_path + 'RERD.txt', RERD)
    np.savetxt(file_path + 'LELD.txt', LELD)
    np.savetxt(file_path + 'LERD.txt', LERD)

    """ np.savetxt('./Python/' + patientName + '/righteyedev.txt', pdr)
    np.savetxt('./Python/' + patientName + '/lefteyedev.txt', pdr)
    np.savetxt('./Python/' + patientName + '/righteyedevinmm.txt', deviationsr)
    np.savetxt('./Python/' + patientName + '/lefteyedevinmm.txt', deviationsl) """

    # In[ ]:
if __name__ == "__main__":

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
    
    args = parser.parse_args()

    # demo code
    main()

