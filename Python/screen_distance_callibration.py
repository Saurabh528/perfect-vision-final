import mediapipe as mp
import cv2
import numpy as np
import math
from datetime import datetime
import time


def calculate_head_orientation(landmarks):
    # Assume landmarks are normalized [0, 1]l
    # Define some key landmarks
    nose_tip = landmarks[1]  # Tip of the nose
    nose_bridge = landmarks[6]  # Top of the nose bridge
    left_eye_outer = landmarks[33]  # Outer corner of the left eye
    right_eye_outer = landmarks[263]  # Outer corner of the right eye
    # Convert landmarks to numpy arrays
    nose_tip = np.array([nose_tip.x, nose_tip.y, nose_tip.z])
    nose_bridge = np.array([nose_bridge.x, nose_bridge.y, nose_bridge.z])
    left_eye_outer = np.array([left_eye_outer.x, left_eye_outer.y, left_eye_outer.z])
    right_eye_outer = np.array([right_eye_outer.x, right_eye_outer.y, right_eye_outer.z])
    # Calculate the vectors
    horizontal_vector = right_eye_outer - left_eye_outer
    vertical_vector = nose_bridge - nose_tip
    # Normalize the vectors
    horizontal_vector_normalized = horizontal_vector / np.linalg.norm(horizontal_vector)
    vertical_vector_normalized = vertical_vector / np.linalg.norm(vertical_vector)
    # Calculate roll
    roll = np.arctan2(horizontal_vector_normalized[1], horizontal_vector_normalized[0])
    roll = np.degrees(roll)
    # Calculate yaw and pitch
    # This is a simplified approach - for more accuracy, a 3D head model or additional landmarks might be necessary
    yaw = np.arctan2(vertical_vector_normalized[0], vertical_vector_normalized[2])
    yaw = np.degrees(yaw)
    pitch = np.arctan2(vertical_vector_normalized[1], vertical_vector_normalized[2])
    pitch = np.degrees(pitch)
    return roll, yaw, pitch

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



# In[5]:


def get_unique(c):
    temp_list = list(c)
    temp_set = set()
    for t in temp_list:
        temp_set.add(t[0])
        temp_set.add(t[1])
    return list(temp_set)


import os   

from argparse import ArgumentParser
from utils import get_anonymous_directory, wait_for_camera

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


from unitysocket import init_TCP, send_command_to_unity, send_message_to_unity

# Initialize TCP connection
if args.connect:
    socket = init_TCP(args.port)
if not os.path.exists(args.datadir):
    os.makedirs(args.datadir, exist_ok=True)
print(args.datadir)

focus = []
with mp_face_mesh.FaceMesh(
    static_image_mode=True,
    max_num_faces=2,
    refine_landmarks=True,
    min_detection_confidence=0.3) as face_mesh:
    for i in range(10):
        flag = 0
        filename = os.path.join(args.datadir, f'grab_screen{i}.png')
        frame = cv2.imread(filename)
        if frame is None:
            break
        os.remove(filename)
        results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))

        try:
            for face_landmark in results.multi_face_landmarks:
                lms = face_landmark.landmark
                d= {}
                for index in iris_indices:
                    x = int(lms[index].x*frame.shape[1])
                    y = int(lms[index].y*frame.shape[0])
                    d[index] = (x,y)
                """ black = np.zeros(frame.shape).astype("uint8")
                for index in iris_indices:
                    #print(index)
                    cv2.circle(frame,(d[index][0],d[index][1]),1,(0,255,0),-1)
                text = "Sit at 50 cms from the screen. press p 10 times in still position once comfortable"
                roll, yaw, pitch = calculate_head_orientation(face_landmark.landmark)
                fontScale = 0.7
                thickness = 2
                frame = cv2.putText(frame, f'Roll: {roll:.2f}', (50, 100), cv2.FONT_HERSHEY_SIMPLEX, fontScale, (0, 255, 0), thickness)
                frame = cv2.putText(frame, f'Yaw: {yaw:.2f}', (50, 150), cv2.FONT_HERSHEY_SIMPLEX, fontScale, (0, 255, 0), thickness)
                frame = cv2.putText(frame, f'Pitch: {pitch:.2f}', (50, 200), cv2.FONT_HERSHEY_SIMPLEX, fontScale, (0, 255, 0), thickness)
                frame = cv2.putText(frame, "Sit at 50 cms from the screen.", (50, 20), cv2.FONT_HERSHEY_SIMPLEX, fontScale, (0, 255, 0), thickness)
                frame = cv2.putText(frame, "Press p 10 times in still position once comfortable.", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, fontScale, (0, 255, 0), thickness) """
                
                
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
                
                """ cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
                cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1) """
                
                w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                
                W = 6.3
                
                d = 50
            
                f = (w*d)/W
                


                #cv2.imshow("final", frame)
                if 1: #cv2.waitKey(1) & 0xFF == ord('p'):
                    focus.append(f)
                    #cv2.imshow("final", frame)
                    if len(focus) >= 10:
                        flag = 1
                        break

        except Exception as e:
            print(e)

        """ if flag == 1:
            break """
    count = 0

#cap.release()
#cv2.destroyAllWindows()


    # In[12]:

if len(focus) == 0:
    final_focus = 0
    print("Can not get focus value.")
else:
    final_focus = sum(focus)/len(focus)


file_path = os.path.join(args.datadir, 'focus_value.txt')
# Open the file in write mode ('w') and write the focus value to it
with open(file_path, 'w') as file:
    # Write the single value followed by a newline character
    file.write(f"{final_focus}\n")

print(f"Focus value has been written to {file_path}")
    



