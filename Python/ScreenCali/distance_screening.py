#!/usr/bin/env python
# coding: utf-8

# In[1]:

from argparse import ArgumentParser
import mediapipe as mp
import cv2
import numpy as np
import math
from datetime import datetime
import time
import pandas as pd

# In[5]:

args = None
def get_unique(c):
    temp_list = list(c)
    temp_set = set()
    for t in temp_list:
        temp_set.add(t[0])
        temp_set.add(t[1])
    return list(temp_set)

def main():
    with open('./Python/DPI.txt', 'r') as f:
        lines = f.readlines()
    patientName = lines[0].strip()
    print('patientName: ' + patientName)

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


    mp_face_mesh = mp.solutions.face_mesh
    connections_iris = mp_face_mesh.FACEMESH_IRISES
    iris_indices = get_unique(connections_iris)

    connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
    left_eyes_indices = get_unique(connections_left_eyes)

    connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
    right_eyes_indices = get_unique(connections_right_eyes)


    pixel_to_mm = 0.264583 # convert pixel to millimeter, this depends on your specific camera
    cap = cv2.VideoCapture(args.cameraindex)
    fps = cap.get(cv2.CAP_PROP_FPS)
    start_time = time.time()
    focus = []
    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=2,
        refine_landmarks=True,
        min_detection_confidence=0.5) as face_mesh:
        count = 0 
        while cap.isOpened():
            flag = 0
            ret, frame = cap.read()
            if (time.time() - start_time) > float(5):
                break
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
                    
                    d = 50
                
                    f = (w*d)/W
                    
                    focus.append(f)


                    cv2.imshow("final", frame)
                    if cv2.waitKey(1) & 0xFF == ord('q'):
                        flag = 1
                        break
            except Exception as e:
                print(e)

            if flag == 1:
                break
        count = 0

    cap.release()
    cv2.destroyAllWindows()

    final_focus = sum(focus)/len(focus)

    df = pd.DataFrame({ 'focus' : [final_focus]
    })
    df.to_csv('./Python/' + patientName + '/focus_final.csv')


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
