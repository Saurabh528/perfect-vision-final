import mediapipe as mp
import cv2
import numpy as np
import math
from datetime import datetime
import time
import os

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


# In[6]:


mp_face_mesh = mp.solutions.face_mesh
connections_iris = mp_face_mesh.FACEMESH_IRISES
iris_indices = get_unique(connections_iris)

connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
left_eyes_indices = get_unique(connections_left_eyes)

connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
right_eyes_indices = get_unique(connections_right_eyes)
cap = cv2.VideoCapture(0)
fps = cap.get(cv2.CAP_PROP_FPS)
start_time = time.time()
focus = []
with mp_face_mesh.FaceMesh(
    static_image_mode=True,
    max_num_faces=2,
    refine_landmarks=True,
    min_detection_confidence=0.3) as face_mesh:
    count = 0 
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
                black = np.zeros(frame.shape).astype("uint8")
                for index in iris_indices:
                    #print(index)
                    cv2.circle(frame,(d[index][0],d[index][1]),1,(0,255,0),-1)
                text = "Sit at 50 cms from the screen and"
                text2 = "press p 10 times in still position"
                text3 = " once comfortable"
                roll, yaw, pitch = calculate_head_orientation(face_landmark.landmark)
                frame = cv2.putText(frame, f'Roll: {roll:.2f}', (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                frame = cv2.putText(frame, f'Yaw: {yaw:.2f}', (50, 250), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                frame = cv2.putText(frame, f'Pitch: {pitch:.2f}', (50, 300), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                frame = cv2.putText(frame, text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
                frame = cv2.putText(frame, text2, (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
                frame = cv2.putText(frame, text3, (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
                
                
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
                


                cv2.imshow("final", frame)
                if cv2.waitKey(1) & 0xFF == ord('p'):
                    focus.append(f)
                    #frame = cv2.putText(frame, "Sit Still and press p 10 times", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
                    cv2.imshow("final", frame)
                    if len(focus) >= 10:
                        flag = 1
                        break

        except Exception as e:
            print(e)

        if flag == 1:
            break
    count = 0

cap.release()
cv2.destroyAllWindows()


# In[12]:


final_focus = sum(focus)/len(focus)


# file_path = 'focus_value.txt'

# # Open the file in write mode ('w') and write the focus value to it
# with open(file_path, 'w') as file:
#     # Write the single value followed by a newline character
#     file.write(f"{final_focus}\n")

# print(f"Focus value has been written to {file_path}")
script_dir = os.path.dirname(__file__)
file_path = os.path.join(script_dir, 'cv.txt')
                # file_path  = r"H:\Vpower2\perfect-vision\Python\ScreenCali\cv.txt"
with open(file_path, 'w') as file:
        file.write(str(final_focus))
print(f"Focus value has been written to {file_path}")

























# import mediapipe as mp
# import cv2
# import numpy as np
# import math
# from datetime import datetime
# import time
# import os


# def calculate_head_orientation(landmarks):
#     # Assume landmarks are normalized [0, 1]l
#     # Define some key landmarks
#     nose_tip = landmarks[1]  # Tip of the nose
#     nose_bridge = landmarks[6]  # Top of the nose bridge
#     left_eye_outer = landmarks[33]  # Outer corner of the left eye
#     right_eye_outer = landmarks[263]  # Outer corner of the right eye
#     # Convert landmarks to numpy arrays
#     nose_tip = np.array([nose_tip.x, nose_tip.y, nose_tip.z])
#     nose_bridge = np.array([nose_bridge.x, nose_bridge.y, nose_bridge.z])
#     left_eye_outer = np.array([left_eye_outer.x, left_eye_outer.y, left_eye_outer.z])
#     right_eye_outer = np.array([right_eye_outer.x, right_eye_outer.y, right_eye_outer.z])
#     # Calculate the vectors
#     horizontal_vector = right_eye_outer - left_eye_outer
#     vertical_vector = nose_bridge - nose_tip
#     # Normalize the vectors
#     horizontal_vector_normalized = horizontal_vector / np.linalg.norm(horizontal_vector)
#     vertical_vector_normalized = vertical_vector / np.linalg.norm(vertical_vector)
#     # Calculate roll
#     roll = np.arctan2(horizontal_vector_normalized[1], horizontal_vector_normalized[0])
#     roll = np.degrees(roll)
#     # Calculate yaw and pitch
#     # This is a simplified approach - for more accuracy, a 3D head model or additional landmarks might be necessary
#     yaw = np.arctan2(vertical_vector_normalized[0], vertical_vector_normalized[2])
#     yaw = np.degrees(yaw)
#     pitch = np.arctan2(vertical_vector_normalized[1], vertical_vector_normalized[2])
#     pitch = np.degrees(pitch)
#     return roll, yaw, pitch

# def get_unique(c):
#     temp_list = list(c)
#     temp_set = set()
#     for t in temp_list:
#         temp_set.add(t[0])
#         temp_set.add(t[1])
#     return list(temp_set)



# mp_face_mesh = mp.solutions.face_mesh
# connections_iris = mp_face_mesh.FACEMESH_IRISES
# iris_indices = get_unique(connections_iris)

# connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
# left_eyes_indices = get_unique(connections_left_eyes)

# connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
# right_eyes_indices = get_unique(connections_right_eyes)


# iris_right_horzn = [469,471]
# iris_right_vert = [470,472]
# iris_left_horzn = [474,476]
# iris_left_vert = [475,477]



# # In[5]:


# def get_unique(c):
#     temp_list = list(c)
#     temp_set = set()
#     for t in temp_list:
#         temp_set.add(t[0])
#         temp_set.add(t[1])
#     return list(temp_set)


# # In[6]:


# mp_face_mesh = mp.solutions.face_mesh
# connections_iris = mp_face_mesh.FACEMESH_IRISES
# iris_indices = get_unique(connections_iris)

# connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
# left_eyes_indices = get_unique(connections_left_eyes)

# connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
# right_eyes_indices = get_unique(connections_right_eyes)
# cap = cv2.VideoCapture(0)
# fps = cap.get(cv2.CAP_PROP_FPS)
# start_time = time.time()
# focus = []
# with mp_face_mesh.FaceMesh(
#     static_image_mode=True,
#     max_num_faces=2,
#     refine_landmarks=True,
#     min_detection_confidence=0.3) as face_mesh:
#     count = 0 
#     while cap.isOpened():
#         flag = 0
#         ret, frame = cap.read()

#         if not ret:
#             break

#         results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))

#         try:
#             for face_landmark in results.multi_face_landmarks:
#                 lms = face_landmark.landmark
#                 d= {}
#                 for index in iris_indices:
#                     x = int(lms[index].x*frame.shape[1])
#                     y = int(lms[index].y*frame.shape[0])
#                     d[index] = (x,y)
#                 black = np.zeros(frame.shape).astype("uint8")
#                 for index in iris_indices:
#                     #print(index)
#                     cv2.circle(frame,(d[index][0],d[index][1]),1,(0,255,0),-1)
#                 text = "Sit at 50 cms from the screen and"
#                 text2 = "press p 10 times in still position"
#                 text3 = " once comfortable"
#                 roll, yaw, pitch = calculate_head_orientation(face_landmark.landmark)
#                 frame = cv2.putText(frame, f'Roll: {roll:.2f}', (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                 frame = cv2.putText(frame, f'Yaw: {yaw:.2f}', (50, 250), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                 frame = cv2.putText(frame, f'Pitch: {pitch:.2f}', (50, 300), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                 frame = cv2.putText(frame, text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
#                 frame = cv2.putText(frame, text2, (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
#                 frame = cv2.putText(frame, text3, (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
                
#                 centre_right_iris_x_1 = int((d[iris_right_horzn[0]][0] + d[iris_right_horzn[1]][0])/2)
#                 centre_right_iris_y_1 = int((d[iris_right_horzn[0]][1] + d[iris_right_horzn[1]][1])/2)
                
#                 centre_right_iris_x_2 = int((d[iris_right_vert[0]][0] + d[iris_right_vert[1]][0])/2)
#                 centre_right_iris_y_2 = int((d[iris_right_vert[0]][1] + d[iris_right_vert[1]][1])/2)
                
                    
#                 centre_left_iris_x_1 = int((d[iris_left_horzn[0]][0] + d[iris_left_horzn[1]][0])/2)
#                 centre_left_iris_y_1 = int((d[iris_left_horzn[0]][1] + d[iris_left_horzn[1]][1])/2)
                
#                 centre_left_iris_x_2 = int((d[iris_left_vert[0]][0] + d[iris_left_vert[1]][0])/2)
#                 centre_left_iris_y_2 = int((d[iris_left_vert[0]][1] + d[iris_left_vert[1]][1])/2)
                
#                 centre_left_iris_x = int((centre_left_iris_x_1 + centre_left_iris_x_2)/2)
#                 centre_left_iris_y = int((centre_left_iris_y_1 + centre_left_iris_y_2)/2)
                
#                 centre_right_iris_x = int((centre_right_iris_x_1 + centre_right_iris_x_2)/2)
#                 centre_right_iris_y = int((centre_right_iris_y_1 + centre_right_iris_y_2)/2)
                
#                 cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
#                 cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1)
                
#                 w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                
#                 W = 6.3
                
#                 d = 50
            
#                 f = (w*d)/W
                


#                 cv2.imshow("final", frame)
#                 if cv2.waitKey(1) & 0xFF == ord('p'):
#                     focus.append(f)
#                     #frame = cv2.putText(frame, "Sit Still and press p 10 times", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
#                     cv2.imshow("final", frame)
#                     if len(focus) >= 10:
#                         flag = 1
#                         break

#         except Exception as e:
#             print(e)

#         if flag == 1:
#             break
#     count = 0

# cap.release()
# cv2.destroyAllWindows()


# # In[12]:


# final_focus = sum(focus)/len(focus)


# script_dir = os.path.dirname(__file__)
# file_path = os.path.join(script_dir, 'cv.txt')
#                 # file_path  = r"H:\Vpower2\perfect-vision\Python\ScreenCali\cv.txt"
# with open(file_path, 'w') as file:
#         file.write(str(final_focus))
# print(f"Focus value has been written to {file_path}")























# import mediapipe as mp
# import cv2
# import numpy as np
# import os
# def calculate_screen_distance(landmarks, frame_width, frame_height):
#     average_inter_ocular_distance = 6.3  # in millimeters
#     left_pupil_index = 468  # Adjust as needed
#     right_pupil_index = 473  # Adjust as needed
#     left_pupil = None
#     right_pupil = None

#     for i, landmark in enumerate(landmarks.landmark):
#         if i == left_pupil_index:
#             left_pupil = landmark
#         elif i == right_pupil_index:
#             right_pupil = landmark

#     if left_pupil is None or right_pupil is None:
#         return None  # Pupils not found

#     left_pupil_x = int(left_pupil.x * frame_width)
#     left_pupil_y = int(left_pupil.y * frame_height)
#     right_pupil_x = int(right_pupil.x * frame_width)
#     right_pupil_y = int(right_pupil.y * frame_height)

#     pixel_distance = np.sqrt((right_pupil_x - left_pupil_x) ** 2 + (right_pupil_y - left_pupil_y) ** 2)
#     camera_fov = 60  # in degrees
#     perceived_width = 2 * (average_inter_ocular_distance / 2) / np.tan(np.radians(camera_fov / 2))
#     scale_factor = perceived_width / pixel_distance
#     real_face_width = 14  # in millimeters
#     screen_distance = (real_face_width * scale_factor) / 2
#     return screen_distance * 50  # Convert to centimeters

# # Globals for the rectangle drawing and calibration
# pt1 = (0, 0)
# pt2 = (0, 0)
# topLeft_clicked = False
# botRight_clicked = False

# def draw_rectangle(event, x, y, flags, param):
#     global pt1, pt2, topLeft_clicked, botRight_clicked
#     if event == cv2.EVENT_LBUTTONDOWN:
#         if topLeft_clicked and botRight_clicked:
#             pt1, pt2 = (0, 0), (0, 0)
#             topLeft_clicked, botRight_clicked = False, False
#         if not topLeft_clicked:
#             pt1 = (x, y)
#             topLeft_clicked = True
#         elif not botRight_clicked:
#             pt2 = (x, y)
#             botRight_clicked = True

# def process_video():
#     global pt1, pt2, topLeft_clicked, botRight_clicked  # Declare these variables as global

#     mp_face_detection = mp.solutions.face_detection
#     mp_face_mesh = mp.solutions.face_mesh

#     cap = cv2.VideoCapture(0)  # Use '0' for webcam. Replace with video file path if necessary.

#     cv2.namedWindow('Frame')
#     cv2.setMouseCallback('Frame', draw_rectangle)

#     with mp_face_mesh.FaceMesh(
#         static_image_mode=False,
#         max_num_faces=1,
#         refine_landmarks=True,
#         min_detection_confidence=0.5) as face_mesh:

#         while cap.isOpened():
#             ret, frame = cap.read()
#             if not ret:
#                 break

#             image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
#             h, w, _ = frame.shape
#             results = face_mesh.process(image_rgb)

#             if results.multi_face_landmarks:
#                 for face_landmarks in results.multi_face_landmarks:
#                     screen_distance = calculate_screen_distance(face_landmarks, w, h)
#                     if screen_distance:
#                         screen_distance_text = f"Screen Distance: {screen_distance:.2f} cm"
#                         cv2.putText(frame, screen_distance_text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                         cv2.putText(frame, "Place card near head & draw rectangle", (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

#             if topLeft_clicked:
#                 cv2.circle(frame, center=pt1, radius=5, color=(0, 255, 0), thickness=-1)
#             if topLeft_clicked and botRight_clicked:
#                 cv2.rectangle(frame, pt1, pt2, (0, 255, 0), 2)

#                 card_width_pixels = abs(pt2[0] - pt1[0])
#                 card_height_pixels = abs(pt2[1] - pt1[1])
#                 CARD_WIDTH_MM = 85.60
#                 CARD_HEIGHT_MM = 53.98
#                 width_conversion_rate = CARD_WIDTH_MM / card_width_pixels
#                 height_conversion_rate = CARD_HEIGHT_MM / card_height_pixels
#                 cv2.putText(frame, f"width Conversion Rate: {width_conversion_rate:.2f} mm/pixel", (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
#                 cv2.putText(frame, f"height Conversion Rate: {height_conversion_rate:.2f} mm/pixel", (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
#                 cv2.putText(frame, f"press q to exit or attempt again", (50, 250), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
#                 print("Working")
#                 script_dir = os.path.dirname(__file__)
#                 file_path = os.path.join(script_dir, 'cv.txt')
#                 # file_path  = r"H:\Vpower2\perfect-vision\Python\ScreenCali\cv.txt"
#                 with open(file_path, 'w') as file:
#                     file.write(str(width_conversion_rate))


                

#             cv2.imshow('Frame', frame)

#             key = cv2.waitKey(1) & 0xFF
#             if key == ord('q'):
#                 break
#             elif key == ord('r'):
#                 pt1, pt2 = (0, 0), (0, 0)
#                 topLeft_clicked, botRight_clicked = False, False

#         cap.release()
#         cv2.destroyAllWindows()

# process_video()