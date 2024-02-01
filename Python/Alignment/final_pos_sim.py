#!/usr/bin/env python
# coding: utf-8
import sys
# In[4]:

from argparse import ArgumentParser
import mediapipe as mp
import cv2
import numpy as np
import math
from datetime import datetime
import time
import pandas as pd
import os
import queue

# for TCP connection with unity
import socket
# global variable
port = 5066         # have to be same as unity
args = None
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

font = cv2.FONT_HERSHEY_SIMPLEX

# org
org = (50, 50)
org1 = (50,100)

# fontScale
fontScale = 1

# Blue color in BGR
color = (255, 0, 0)

# Line thickness of 2 px
thickness = 1
"""focus_df = pd.read_csv('./Python/Alignment/dis_cal.csv')
focus_values = focus_df.values.tolist()
focus_values = [item for sublist in focus_values for item in sublist]
focus = int(sum(focus_values)/len(focus_values))
focus = round(focus,2)"""
patientName = "Anonymous"
with open('./Python/DPI.txt', 'r') as f:
    lines = f.readlines()
    patientName = lines[0].strip()
    print('patientName: ' + patientName)
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

iris_right_horzn = [469,471]
iris_right_vert = [470,472]
iris_left_horzn = [474,476]
iris_left_vert = [475,477]

def slope(x1, y1, x2, y2): # Line slope given two points:
    return (y2-y1)/(x2-x1)
def angle(s1, s2): 
    return math.degrees(math.atan((s2-s1)/(1+(s2*s1))))
def distance(x1, y1, x2, y2):
    return (((x2 - x1)**2 +(y2 - y1)**2)**0.5)


def process_value(value, last_value, start_timestamp, change_timestamp):
    if value != last_value:
        if change_timestamp is None:
            change_timestamp = time.time()
        elif time.time() - change_timestamp > 2:
            end_timestamp = time.time()
            time_range = (start_timestamp, end_timestamp)
            return time_range, change_timestamp
    else:
        change_timestamp = None

    return None, change_timestamp

leld = []
lerd = []
reld = []
rerd = []

pos_sim_values = []
temp_leld = 0
temp_lerd = 0
temp_reld = 0
temp_rerd = 0

last_value = None
start_timestamp = None
change_timestamp = None


csv_filename = "./Python/Alignment/report_strabisums.csv"
if os.path.isfile(csv_filename):
    df = pd.read_csv(csv_filename)
else:
    df = pd.DataFrame(columns=['Start', 'End' , 'Duration' , 'Value'])

result_queue = queue.Queue()

column_names = ['Time', 'Value', 'Positional_Similarity']

# Create an empty DataFrame
# df = pd.DataFrame(columns=column_names)

# df.to_csv('out_data.csv', index=False)

def main():
    # Initialize TCP connection
    if args.connect:
        socket = init_TCP()
    # 3 second delay
    time.sleep(0.5)
    cap = cv2.VideoCapture(args.cameraindex)
    if not cap.isOpened():
        send_message_to_unity(socket, "Failed to connect camera.")
        time.sleep(2)
        send_command_to_unity(socket, "FAIL")
        time.sleep(0.5)
        send_command_to_unity(socket, "EXIT")
        #sys.exit()
        #return
    
    fps = cap.get(cv2.CAP_PROP_FPS)
    print(fps)
    send_message_to_unity(socket, "Track the red point on the screen.")
    time.sleep(1)
    send_command_to_unity(socket, 'START')
    time.sleep(3)
    send_message_to_unity(socket, "")
    last_value = None
    start_timestamp = None
    change_timestamp = None
    leld = []
    lerd = []
    reld = []
    rerd = []

    pos_sim_values = []
    temp_leld = 0
    temp_lerd = 0
    temp_reld = 0
    temp_rerd = 0
    column_names = ['Time','Value','Positional_Similarity','LeLd','LeRd','ReLd','ReRd']

    df = pd.DataFrame(columns=column_names)
    df.to_csv('./Python/' + patientName + '/out_data.csv', index=False)
    start_time = time.time()
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
                
            if time.time() - start_time > (60):
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
                   
                    screen_distance = (W*focus)/w
                    screen_distance = int(screen_distance)
                    send_status_to_unity(socket, "Distance: " + str(screen_distance))
                    frame = cv2.putText(frame, " Distance : " + str(screen_distance), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
                    
                    start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

                    new_data = {"Time": start, "Distance": str(screen_distance)}
                    
                    df = pd.read_csv('./Python/Alignment/screen_face_distance.csv')
                    if len(df)>500:
                        df = df.iloc[250:]
                        df.reset_index(drop=True, inplace=True)
                    
                    df = df._append(new_data, ignore_index=True)
                    df.to_csv('./Python/Alignment/screen_face_distance.csv', index=False)

                    e= {}
                    for index in left_eyes_indices:
                        x = int(lms[index].x*frame.shape[1])
                        y = int(lms[index].y*frame.shape[0])
                        e[index] = (x,y)
                    for index in left_eyes_indices:
                        #print(index)
                        cv2.circle(frame,(e[index][0],e[index][1]),2,(0,255,0),-1)
                        cv2.circle(black,(e[index][0],e[index][1]),2,(0,0,255),-1)
                        if index == 263 or index == 362:
                            cv2.line(black,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
                            cv2.line(frame,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
                    for conn in list(connections_left_eyes):
                        cv2.line(black,(e[conn[0]][0],e[conn[0]][1]),(e[conn[1]][0],e[conn[1]][1]),(0,0,255),1)

                    f= {}
                    for index in right_eyes_indices:
                        x = int(lms[index].x*frame.shape[1])
                        y = int(lms[index].y*frame.shape[0])
                        f[index] = (x,y)

                    for index in right_eyes_indices:
                        #print(index)
                        cv2.circle(frame,(f[index][0],f[index][1]),2,(0,255,0),-1)
                        cv2.circle(black,(f[index][0],f[index][1]),2,(0,0,255),-1)
                        if index == 33 or index == 133:
                            cv2.line(black,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
                            cv2.line(frame,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
                    for conn in list(connections_right_eyes):
                        cv2.line(black,(f[conn[0]][0],f[conn[0]][1]),(f[conn[1]][0],f[conn[1]][1]),(0,0,255),1)
                    

                    left_eye_left_point_index = 263
                    left_eye_right_point_index = 398
                    right_eye_right_point_index = 33
                    right_eye_left_point_index = 133
                    df = pd.DataFrame()
                    df.to_csv('./Python/Alignment/working1.csv')
                    #st.write("I am Screening your eyes")
                    

                    le_lp_d = int((((e[left_eye_left_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_left_point_index][1] - centre_left_iris_y)**2)**0.5))
                    le_rp_d = int((((e[left_eye_right_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_right_point_index][1] - centre_left_iris_y)**2)**0.5))

                    re_lp_d = int((((f[right_eye_left_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_left_point_index][1] - centre_right_iris_y)**2)**0.5))
                    re_rp_d = int((((f[right_eye_right_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_right_point_index][1] - centre_right_iris_y)**2)**0.5))
                     
                    
                    #frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(latest_data.values[0]), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
               
                    df_leld = pd.read_csv('./Python/Alignment/leld.csv')
                    data_list_leld = []
                    df_lerd = pd.read_csv('./Python/Alignment/lerd.csv')
                    data_list_lerd = []
                    df_reld = pd.read_csv('./Python/Alignment/reld.csv')
                    data_list_reld = []
                    df_rerd = pd.read_csv('./Python/Alignment/rerd.csv')
                    data_list_rerd = []
                    
                    
                    if len(df_leld) < 30:     
                        new_data = {'vals': le_lp_d}
                        df_leld = df_leld._append(new_data, ignore_index=True)
                        df_leld.to_csv('./Python/Alignment/leld.csv', index=False)
                        
                        new_data = {'vals': le_rp_d}
                        df_lerd = df_lerd._append(new_data, ignore_index=True)
                        df_lerd.to_csv('./Python/Alignment/lerd.csv', index=False)
                        
                        new_data = {'vals': re_lp_d}
                        df_reld = df_reld._append(new_data, ignore_index=True)
                        df_reld.to_csv('./Python/Alignment/reld.csv', index=False)
                        
                        new_data = {'vals': re_rp_d}
                        df_rerd = df_rerd._append(new_data, ignore_index=True)
                        df_rerd.to_csv('./Python/Alignment/rerd.csv', index=False)

                        
                        
                    else:
       
                        data_list_leld = df_leld.values.tolist()
                        empty_df = pd.DataFrame(columns=df_leld.columns)
                        empty_df.to_csv('./Python/Alignment/leld.csv', index=False)

                        data_list_lerd = df_lerd.values.tolist()
                        empty_df = pd.DataFrame(columns=df_lerd.columns)
                        empty_df.to_csv('./Python/Alignment/lerd.csv', index=False)
                        
                        data_list_reld = df_reld.values.tolist()
                        empty_df = pd.DataFrame(columns=df_reld.columns)
                        empty_df.to_csv('./Python/Alignment/reld.csv', index=False)
                        
                           
                        data_list_rerd = df_rerd.values.tolist()
                        empty_df = pd.DataFrame(columns=df_rerd.columns)
                        empty_df.to_csv('./Python/Alignment/rerd.csv', index=False)

                        
                        
                        
    #                 frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(len(data_list_leld)), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)  
    #                 frame = cv2.putText(img, "right Eye right Point Distance : " + str(len(data_list_reld)), (50,200), font, fontScale, color, thickness, cv2.LINE_AA)
    #                 frame = cv2.putText(img, "right Eye Left Point Distance : " + str(len(data_list_lerd)), (50,250), font, fontScale, color, thickness, cv2.LINE_AA)
    #                 frame = cv2.putText(img, "Left Eye right Point Distance : " + str(len(data_list_rerd)), (50,300), font, fontScale, color, thickness, cv2.LINE_AA)
                        
                    

                    if len(data_list_leld) == 30:
                        

                        
    #                     leld = [item for sublist in data_list_leld for item in sublist]
    #                     #lerd = data_list_lerd
    #                     lerd = [item for sublist in data_list_lerd for item in sublist]
    #                     #reld = data_list_reld
    #                     reld = [item for sublist in data_list_reld for item in sublist]
    #                     #rerd = data_list_rerd
    #                     rerd = [item for sublist in data_list_rerd for item in sublist]
                
                        data_list_leld = [item for sublist in data_list_leld for item in sublist]
                        temp_leld = int(sum(data_list_leld)/len(data_list_leld))

                        data_list_reld = [item for sublist in data_list_reld for item in sublist]
                        temp_reld = int(sum(data_list_reld)/len(data_list_reld))
                        

                        data_list_lerd = [item for sublist in data_list_lerd for item in sublist]
                        temp_lerd = int(sum(data_list_lerd)/len(data_list_lerd))
                        data_list_rerd = [item for sublist in data_list_rerd for item in sublist]
                        temp_rerd = int(sum(data_list_rerd)/len(data_list_rerd))

                        L2 = temp_leld
                        L1 = temp_lerd
                        R1 = temp_reld
                        R2 = temp_rerd

                        #pos_sim = max((R1/R2),(L1/L2))/min((R1/R2),(L1/L2))
                        pos_sim = R1 * L1 / R2 / L2;
                        pos_sim_values.append(pos_sim)

                        value = "Normal"
                        if pos_sim > 1.42:
                            value = "Strabismus"
                        else:
                            value = "Normal"
                        
                     

                        start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

                        new_data = {"Time": start, "Value": str(value),"Positional_Similarity":str(round(pos_sim,2)),
                                   "LeLd":L2,"LeRd":L1,"ReLd":R1,"ReRd":R2}
                        
                        
                        df = pd.read_csv('./Python/' + patientName + '/out_data.csv')
               
                        #df = df.append(data, ignore_index=True)
                          
                        df = df._append(new_data, ignore_index=True)
                        df.to_csv('./Python/' + patientName + '/out_data.csv', index=False)
                        
                    if not args.quiet:
                        cv2.imshow("final", frame)
                    if cv2.waitKey(1) & 0xFF == ord('q'):
                        flag = 1
                        break

            except Exception as e:
                print(e)
            
            if flag == 1:
                break
    cap.release()
    cv2.destroyAllWindows()
    # In[2]:
    # In[3]:
    
    def custom_date_parser(date_string):
        return pd.to_datetime(date_string, format="%d/%m/%y %H:%M:%S")
    csv_file = './Python/' + patientName + '/out_data.csv'
    df = pd.read_csv(csv_file, parse_dates=['Time'], date_parser=custom_date_parser)

            # Create bins for the 'Positional_Similarity' column with 0.25 intervals
    bins = [i * 0.25 for i in range(41)]  # 41 since we want to include 10 (0.25 * 40 = 10)
    labels = [f'{i * 0.25}-{(i + 1) * 0.25}' for i in range(40)]  # 40 intervals in total
    df['pos_similarity_interval'] = pd.cut(df['Positional_Similarity'], bins=bins, labels=labels)

    # Calculate the time duration for each row
    df['duration'] = df['Time'].diff()

    # Group the DataFrame by the binned 'pos_similarity_interval' column
    grouped_df = df.groupby('pos_similarity_interval')['duration'].sum().reset_index()

    # Convert the time intervals to seconds and remove bins with no or None values
    grouped_df['duration_seconds'] = grouped_df['duration'].dt.total_seconds()
    grouped_df.dropna(subset=['duration_seconds'], inplace=True)
    grouped_df.reset_index(drop=True, inplace=True)

    grouped_df = grouped_df[['pos_similarity_interval', 'duration_seconds']]


    # In[5]:


    grouped_df.to_csv('./Python/' + patientName + '/grouped_output.csv')
    if args.connect:
        send_command_to_unity(socket, 'EXIT')

    # In[ ]:






    # In[5]:


    grouped_df


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



