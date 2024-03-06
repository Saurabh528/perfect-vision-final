# #!/usr/bin/env python
# # coding: utf-8

# # In[1]:

# from argparse import ArgumentParser
# import mediapipe as mp
# import cv2
# import numpy as np
# import math
# from datetime import datetime
# import time
# import pandas as pd

# # In[5]:

# args = None
# def get_unique(c):
#     temp_list = list(c)
#     temp_set = set()
#     for t in temp_list:
#         temp_set.add(t[0])
#         temp_set.add(t[1])
#     return list(temp_set)

# def main():
#     with open('./Python/DPI.txt', 'r') as f:
#         lines = f.readlines()
#     patientName = lines[0].strip()
#     print('patientName: ' + patientName)

#     mp_face_mesh = mp.solutions.face_mesh
#     connections_iris = mp_face_mesh.FACEMESH_IRISES
#     iris_indices = get_unique(connections_iris)

#     connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
#     left_eyes_indices = get_unique(connections_left_eyes)

#     connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
#     right_eyes_indices = get_unique(connections_right_eyes)


#     iris_right_horzn = [469,471]
#     iris_right_vert = [470,472]
#     iris_left_horzn = [474,476]
#     iris_left_vert = [475,477]


#     mp_face_mesh = mp.solutions.face_mesh
#     connections_iris = mp_face_mesh.FACEMESH_IRISES
#     iris_indices = get_unique(connections_iris)

#     connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
#     left_eyes_indices = get_unique(connections_left_eyes)

#     connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
#     right_eyes_indices = get_unique(connections_right_eyes)


#     pixel_to_mm = 0.264583 # convert pixel to millimeter, this depends on your specific camera
#     cap = cv2.VideoCapture(args.cameraindex)
#     fps = cap.get(cv2.CAP_PROP_FPS)
#     start_time = time.time()
#     focus = []
#     with mp_face_mesh.FaceMesh(
#         static_image_mode=True,
#         max_num_faces=2,
#         refine_landmarks=True,
#         min_detection_confidence=0.5) as face_mesh:
#         count = 0 
#         while cap.isOpened():
#             flag = 0
#             ret, frame = cap.read()
#             if (time.time() - start_time) > float(5):
#                 break
#             if not ret:
#                 break

#             results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))

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
                    
#                     w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                    
#                     W = 6.3
                    
#                     d = 50
                
#                     f = (w*d)/W
                    
#                     focus.append(f)


#                     cv2.imshow("final", frame)
#                     if cv2.waitKey(1) & 0xFF == ord('q'):
#                         flag = 1
#                         break
#             except Exception as e:
#                 print(e)

#             if flag == 1:
#                 break
#         count = 0

#     cap.release()
#     cv2.destroyAllWindows()

#     final_focus = sum(focus)/len(focus)

#     df = pd.DataFrame({ 'focus' : [final_focus]
#     })
#     df.to_csv('./Python/' + patientName + '/focus_final.csv')


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


# -*- coding: utf-8 -*-



# """screendistance.ipynb

# Automatically generated by Colaboratory.

# Original file is located at
#     https://colab.research.google.com/drive/15CALirBlVTpDrT9cQAi4QpNkoJ7DSfyc
# """

# import mediapipe as mp
# import cv2
# import numpy as np
# import socket
# def calculate_screen_distance(landmarks, frame_width, frame_height):
#     # Assuming an average inter-ocular distance of 6.5 cm (65 mm)
#     average_inter_ocular_distance = 6.3  # in millimeters

#     # Landmark indices for the pupils
#     left_pupil_index = 468  # Adjust as needed
#     right_pupil_index = 473  # Adjust as needed

#     left_pupil = None
#     right_pupil = None

#     # Iterate through landmarks to find the pupils
#     for i, landmark in enumerate(landmarks.landmark):
#         if i == left_pupil_index:
#             left_pupil = landmark
#         elif i == right_pupil_index:
#             right_pupil = landmark

#     if left_pupil is None or right_pupil is None:
#         return None  # Pupils not found

#     # Convert to pixel coordinates
#     left_pupil_x = int(left_pupil.x * frame_width)
#     left_pupil_y = int(left_pupil.y * frame_height)
#     right_pupil_x = int(right_pupil.x * frame_width)
#     right_pupil_y = int(right_pupil.y * frame_height)

#     # Calculate the pixel distance between the pupils
#     pixel_distance = ((right_pupil_x - left_pupil_x) ** 2 + (right_pupil_y - left_pupil_y) ** 2) ** 0.5

#     # Camera's horizontal FOV (assumed)
#     camera_fov = 60  # in degrees

#     # Calculate the perceived width of the face (in millimeters)
#     perceived_width = 2 * (average_inter_ocular_distance / 2) / np.tan(np.radians(camera_fov / 2))

#     # Scale factor
#     scale_factor = perceived_width / pixel_distance

#     # Real width of the face (assumed)
#     real_face_width = 14  # in millimeters

#     # Calculate the screen distance
#     screen_distance = (real_face_width * scale_factor) / 2
    
#     return screen_distance * 100  # Convert to centimeters

# mp_face_mesh = mp.solutions.face_mesh

# def process_video(video_path):
#     mp_face_detection = mp.solutions.face_detection
#     face_detection = mp_face_detection.FaceDetection()
#     face_mesh = mp_face_mesh.FaceMesh()
#     cap = cv2.VideoCapture(video_path)
#     HOST = '127.0.0.1'  # The server's hostname or IP address
#     PORT = 5055         # The port used by the server
#     with mp_face_mesh.FaceMesh(
#         static_image_mode=True,
#         max_num_faces=1,
#         refine_landmarks=True,
#         min_detection_confidence=0.3
#     ) as face_mesh:
#         while cap.isOpened():
#             success, image = cap.read()
#             if not success:
#                 break

#             # Convert the image to RGB
#             image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
#             h, w, _ = image.shape
#             results = face_detection.process(image_rgb)

#             if results.detections:
#                 for detection in results.detections:
#                     # Use face mesh to get landmarks
#                     mesh_results = face_mesh.process(image_rgb)
#                     if mesh_results.multi_face_landmarks:
#                         for face_landmarks in mesh_results.multi_face_landmarks:
#                             screen_distance = calculate_screen_distance(face_landmarks, w, h)
#                             screen_distance_text = f"Screen Distance: {screen_distance:.2f} cm"
#                             with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
#                                 # Connect to the server
#                                 s.connect((HOST, PORT))
#                                 # Send screen distance
#                                 s.sendall(str(screen_distance).encode('utf-8'))

#                             cv2.putText(image, screen_distance_text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                             cv2.imshow("Adjust Position", image)

#             if cv2.waitKey(1) & 0xFF == ord('q'):
#                 break

#         # Release the VideoCapture and close the window
#         cap.release()
#         cv2.destroyAllWindows()

# # Replace '0' with your video file path
# process_video(0)


# import mediapipe as mp
# import cv2
# import numpy as np

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

#     return screen_distance * 100  # Convert to centimeters

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
#                 cv2.putText(frame, f"Conversion Rate: {width_conversion_rate:.2f} mm/pixel", (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
#                 cv2.putText(frame, f"Conversion Rate: {height_conversion_rate:.2f} mm/pixel", (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
#                 cv2.putText(frame, f"press q to exit or attempt again", (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
                
                

#             cv2.imshow('Frame', frame)

#             key = cv2.waitKey(1) & 0xFF
#             if key == ord('q'):
#                 with open('conversion_rate.txt', 'w') as file:
#                     file.write(str(width_conversion_rate))
#                 break
#             elif key == ord('r'):
#                 pt1, pt2 = (0, 0), (0, 0)
#                 topLeft_clicked, botRight_clicked = False, False

#         cap.release()
#         cv2.destroyAllWindows()

# process_video()



import mediapipe as mp
import cv2
import numpy as np

def calculate_screen_distance(landmarks, frame_width, frame_height):
    average_inter_ocular_distance = 6.3  # in millimeters
    left_pupil_index = 468  # Adjust as needed
    right_pupil_index = 473  # Adjust as needed
    left_pupil = None
    right_pupil = None

    for i, landmark in enumerate(landmarks.landmark):
        if i == left_pupil_index:
            left_pupil = landmark
        elif i == right_pupil_index:
            right_pupil = landmark

    if left_pupil is None or right_pupil is None:
        return None  # Pupils not found

    left_pupil_x = int(left_pupil.x * frame_width)
    left_pupil_y = int(left_pupil.y * frame_height)
    right_pupil_x = int(right_pupil.x * frame_width)
    right_pupil_y = int(right_pupil.y * frame_height)

    pixel_distance = np.sqrt((right_pupil_x - left_pupil_x) ** 2 + (right_pupil_y - left_pupil_y) ** 2)
    camera_fov = 60  # in degrees
    perceived_width = 2 * (average_inter_ocular_distance / 2) / np.tan(np.radians(camera_fov / 2))
    scale_factor = perceived_width / pixel_distance
    real_face_width = 14  # in millimeters
    screen_distance = (real_face_width * scale_factor) / 2

    return screen_distance * 100  # Convert to centimeters

# Globals for the rectangle drawing and calibration
pt1 = (0, 0)
pt2 = (0, 0)
topLeft_clicked = False
botRight_clicked = False

def draw_rectangle(event, x, y, flags, param):
    global pt1, pt2, topLeft_clicked, botRight_clicked
    if event == cv2.EVENT_LBUTTONDOWN:
        if topLeft_clicked and botRight_clicked:
            pt1, pt2 = (0, 0), (0, 0)
            topLeft_clicked, botRight_clicked = False, False
        if not topLeft_clicked:
            pt1 = (x, y)
            topLeft_clicked = True
        elif not botRight_clicked:
            pt2 = (x, y)
            botRight_clicked = True

def process_video():
    global pt1, pt2, topLeft_clicked, botRight_clicked  # Declare these variables as global

    mp_face_detection = mp.solutions.face_detection
    mp_face_mesh = mp.solutions.face_mesh

    cap = cv2.VideoCapture(0)  # Use '0' for webcam. Replace with video file path if necessary.

    cv2.namedWindow('Frame')
    cv2.setMouseCallback('Frame', draw_rectangle)

    with mp_face_mesh.FaceMesh(
        static_image_mode=False,
        max_num_faces=1,
        refine_landmarks=True,
        min_detection_confidence=0.5) as face_mesh:

        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            h, w, _ = frame.shape
            results = face_mesh.process(image_rgb)

            if results.multi_face_landmarks:
                for face_landmarks in results.multi_face_landmarks:
                    screen_distance = calculate_screen_distance(face_landmarks, w, h)
                    if screen_distance:
                        screen_distance_text = f"Screen Distance: {screen_distance:.2f} cm"
                        cv2.putText(frame, screen_distance_text, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                        cv2.putText(frame, "Place card near head & draw rectangle", (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

            if topLeft_clicked:
                cv2.circle(frame, center=pt1, radius=5, color=(0, 255, 0), thickness=-1)
            if topLeft_clicked and botRight_clicked:
                cv2.rectangle(frame, pt1, pt2, (0, 255, 0), 2)

                card_width_pixels = abs(pt2[0] - pt1[0])
                card_height_pixels = abs(pt2[1] - pt1[1])
                CARD_WIDTH_MM = 85.60
                CARD_HEIGHT_MM = 53.98
                width_conversion_rate = CARD_WIDTH_MM / card_width_pixels
                height_conversion_rate = CARD_HEIGHT_MM / card_height_pixels
                cv2.putText(frame, f"width Conversion Rate: {width_conversion_rate:.2f} mm/pixel", (50, 150), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
                cv2.putText(frame, f"height Conversion Rate: {height_conversion_rate:.2f} mm/pixel", (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
                cv2.putText(frame, f"press q to exit or attempt again", (50, 250), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 0), 2)
                with open('conversion_rate.txt', 'w') as file:
                    file.write(str(width_conversion_rate))


                

            cv2.imshow('Frame', frame)

            key = cv2.waitKey(1) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('r'):
                pt1, pt2 = (0, 0), (0, 0)
                topLeft_clicked, botRight_clicked = False, False

        cap.release()
        cv2.destroyAllWindows()

process_video()