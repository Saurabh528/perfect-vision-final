#!/usr/bin/env python
# coding: utf-8

# In[3]:

from argparse import ArgumentParser

import mediapipe as mp
import cv2
import numpy as np
import time
import pandas as pd
import os
import sys
# for TCP connection with unity
import socket
# global variable
port = 5066         # have to be same as unity

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

# In[4]:


def get_unique(c):
    temp_list = list(c)
    temp_set = set()
    for t in temp_list:
        temp_set.add(t[0])
        temp_set.add(t[1])
    return list(temp_set)


# In[5]:


mp_face_mesh = mp.solutions.face_mesh
connections_iris = mp_face_mesh.FACEMESH_IRISES
iris_indices = get_unique(connections_iris)

connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
left_eyes_indices = get_unique(connections_left_eyes)

connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
right_eyes_indices = get_unique(connections_right_eyes)

# In[6]:


print(left_eyes_indices)


# In[7]:


iris_right_horzn = [469,471]
iris_right_vert = [470,472]
iris_left_horzn = [474,476]
iris_left_vert = [475,477]



# In[8]:



# In[9]:

# In[ ]:





# In[20]:

def main():
    # Initialize TCP connection
    if args.connect:
        socket = init_TCP()
    # 3 second delay
    time.sleep(0.5)
    with open('./Python/DPI.txt', 'r') as f:
        lines = f.readlines()
    patientName = lines[0].strip()
    print('patientName: ' + patientName)
    dpi = float(lines[1])
    print('DPI: ' + str(dpi))
    focus = 800.0
    try:
        with open('./Python/' + patientName + '/focus_final.csv', 'r') as f:
            lines = f.readlines()
        focus = float(lines[1].split(',')[1])
    except FileNotFoundError:
        if args.connect:
            send_message_to_unity(socket, 'Please do Screen Distance checking first')
            time.sleep(3)
            send_command_to_unity(socket, 'EXIT')
        else:
            print('Please do Screen Distance checking first')
        sys.exit()
    print('Focus: ' + str(focus))
# convert pixel to millimeter, this depends on your specific camera


    cap = cv2.VideoCapture(args.cameraindex)
    fps = cap.get(cv2.CAP_PROP_FPS)
    print('FPS: ' + str(fps))

    
    if args.connect:
        send_message_to_unity(socket, 'This test will take 5 rounds of alternate covering of eyes and\n finally will result the deviation in each eyes.')
    else:    
        print("This test will take 5 rounds of alternate covering of eyes and finally will result the deviation in each eyes.")
    time.sleep(5)    
       # Show Point
    if args.connect:
        send_command_to_unity(socket, 'SHOWPOINT')
        time.sleep(1)
    
    # Before measurement
    if args.connect:
        send_message_to_unity(socket, 'Fixate on one point in the screen')
    else:    
        print("Fixate on one point in the screen")
    time.sleep(0.5)

    dpmm = float(dpi/25.4)
    pixel_to_mm = 1/dpmm

    disp_left = []
    disp_right = []
    start_time = time.time()
    alternating_eye = True  # Start with the left eye
    switch_time = time.time() + 5
    current_time = time.time()
    cover_test_rounds = 0
    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=2,
        refine_landmarks=True,
        min_detection_confidence=0.5) as face_mesh:

        while cap.isOpened() and cover_test_rounds < 10:
            flag = 0
            #if (time.time() - start_time) > float(10):
            #    break
            ret, frame = cap.read()
            if not ret:
                break

            results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
            if results.multi_face_landmarks is None:
                break
            if current_time >= switch_time:
                alternating_eye = not alternating_eye
                switch_time = current_time + 5
                cover_test_rounds += 1
                print(cover_test_rounds)
                if alternating_eye:
                    if args.connect:
                        send_message_to_unity(socket, 'Cover your right eye now')
                    else:
                        print("Cover your right eye now")
                else:
                    if args.connect:
                        send_message_to_unity(socket, 'Cover your left eye now')
                    else:
                        print("Cover your left eye now")

            try:
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
                    
                    cv2.circle(black,(centre_right_iris_x,centre_right_iris_y),2,(0,0,255),-1)
                    cv2.circle(black,(centre_left_iris_x,centre_left_iris_y),2,(0,0,255),-1)
                    
                    w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                    
                    W = 6.3
                    distance_to_screen = (focus * W)/w
                    

                    # Existing code here...

                    e= {}
                    sum_left_eye_x, sum_left_eye_y = 0, 0
                    for index in left_eyes_indices:
                        x = int(lms[index].x*frame.shape[1])
                        y = int(lms[index].y*frame.shape[0])
                        e[index] = (x,y)

                        if index in [263, 398, 386, 374]:
                            cv2.circle(frame,(e[index][0],e[index][1]),2,(0,255,0),-1)
                            cv2.circle(black,(e[index][0],e[index][1]),2,(0,0,255),-1)
                            sum_left_eye_x += x
                            sum_left_eye_y += y

                    f= {}
                    sum_right_eye_x, sum_right_eye_y = 0, 0
                    for index in right_eyes_indices:
                        x = int(lms[index].x*frame.shape[1])
                        y = int(lms[index].y*frame.shape[0])
                        f[index] = (x,y)

                        if index in [33, 133, 145, 159]:
                            cv2.circle(frame,(f[index][0],f[index][1]),2,(0,255,0),-1)
                            cv2.circle(black,(f[index][0],f[index][1]),2,(0,0,255),-1)
                            sum_right_eye_x += x
                            sum_right_eye_y += y
                    
                    centre_left_eye_x = int(sum_left_eye_x / 4)
                    centre_left_eye_y = int(sum_left_eye_y / 4)
                    cv2.circle(frame, (centre_left_eye_x, centre_left_eye_y), 2, (0, 0, 255), -1)

                    centre_right_eye_x = int(sum_right_eye_x / 4)
                    centre_right_eye_y = int(sum_right_eye_y / 4)
                    cv2.circle(frame, (centre_right_eye_x, centre_right_eye_y), 2, (0, 0, 255), -1)
                    current_time = time.time()
                    

                    if alternating_eye: 
                        # Calculate displacement for right eye in pixels
                        displacement_right = np.sqrt((centre_right_iris_x - centre_right_eye_x)**2 + 
                                                    (centre_right_iris_y - centre_right_eye_y)**2)

                        # Convert pixel displacement to mm
                        displacement_right_mm = displacement_right * pixel_to_mm

                        # Calculate displacement in prism diopters
                        displacement_right_pd = displacement_right_mm / (distance_to_screen * 1000)
                        disp_right.append(displacement_right_pd)
                        statusStr = f"Right eye displacement: {displacement_right_pd:.2e} PD"
                        cv2.putText(frame, statusStr,
                                            (50, 80), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 0), 2)
                    
                    else:  
                        
                        displacement_left = np.sqrt((centre_left_iris_x - centre_left_eye_x)**2 + 
                                                    (centre_left_iris_y - centre_left_eye_y)**2)

                        # Convert pixel displacement to mm
                        displacement_left_mm = displacement_left * pixel_to_mm

                        # Calculate displacement in prism diopters
                        displacement_left_pd = displacement_left_mm / (distance_to_screen * 1000)
                        disp_left.append(displacement_left_pd)
                        statusStr = f"Left eye displacement: {displacement_left_pd:.2e} PD"
                        cv2.putText(frame, statusStr,
                                            (50, 80), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 0), 2)
                    send_status_to_unity(socket, statusStr)
                    if not args.quiet:
                        cv2.imshow("final", frame)
                    #print(cover_test_rounds)
                    if cv2.waitKey(1) & 0xFF == ord('q'):
                        flag = 1
                        break
            except Exception as e:
                print(e)

            if flag == 1:
                break


    cap.release()
    cv2.destroyAllWindows()

    average_disp_right = sum(disp_right) / len(disp_right)
    average_disp_left = sum(disp_left) / len(disp_left)

       
    df = pd.DataFrame({ 'displacement_right' : [average_disp_right],
                      'displacement_left' : [average_disp_left]})

    df_right = pd.DataFrame({ 'displacement_right' : disp_right})
    df_left = pd.DataFrame({ 'displacement_left' : disp_left})

    # In[23]:


    df.to_csv('./Python/' + patientName + '/alternate_test.csv')
    df.to_csv('./Python/' + patientName + '/alternate_test.csv')
    df_left.to_csv('./Python/' + patientName + '/data_left_displacement.csv')
    df_right.to_csv('./Python/' + patientName + '/data_right_displacement.csv')
     # Exit
    if args.connect:
        send_command_to_unity(socket, 'EXIT')

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
# In[ ]:




