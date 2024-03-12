import cv2
import mediapipe as mp
import numpy as np
import pandas as pd
import socket
import sys
import time
from argparse import ArgumentParser
# Initialize MediaPipe solutions
mp_face_detection = mp.solutions.face_detection
mp_face_mesh = mp.solutions.face_mesh
mp_drawing = mp.solutions.drawing_utils
drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)



port = 5066
args = None
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

import math

def calculate_distance(x1, y1, x2, y2):
    distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)
    return int(distance)

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
LEFT_EYE_INDICES = mp_face_mesh.FACEMESH_LEFT_EYE
RIGHT_EYE_INDICES = mp_face_mesh.FACEMESH_RIGHT_EYE
LEFT_IRIS_INDICES = mp_face_mesh.FACEMESH_LEFT_IRIS
LEFT_IRIS_INDICES = get_unique(LEFT_IRIS_INDICES)
RIGHT_IRIS_INDICES = mp_face_mesh.FACEMESH_RIGHT_IRIS
RIGHT_IRIS_INDICES = get_unique(RIGHT_IRIS_INDICES)
imp_indexes = LEFT_IRIS_INDICES + RIGHT_IRIS_INDICES
eyes_indices = [130, 133, 359, 362]

pos_similarities = []
pos_similarities1 = []
pos_similarities2 = []
leld_list=[]
lerd_list=[]
rerd_list=[]
reld_list=[]
column_names = ['Time', 'Value', 'Positional_Similarity', 'LeLd', 'LeRd', 'ReLd', 'ReRd']
df = pd.DataFrame(columns=column_names)


for i in eyes_indices:
    imp_indexes.append(i)
def draw_specific_landmarks(frame, landmarks, indices):
    for connection in indices:
        if len(connection) == 2:
            start_idx, end_idx = connection
            if 0 <= start_idx < len(landmarks.landmark) and 0 <= end_idx < len(landmarks.landmark):
                start_landmark = landmarks.landmark[start_idx]
                end_landmark = landmarks.landmark[end_idx]
                cv2.line(frame, (int(start_landmark.x * frame.shape[1]), int(start_landmark.y * frame.shape[0])),
                               (int(end_landmark.x * frame.shape[1]), int(end_landmark.y * frame.shape[0])), (0, 255, 0), 1)
def draw_eye_bounding_box(frame, landmarks, indices):
    min_x, min_y = frame.shape[1], frame.shape[0]
    max_x, max_y = 0, 0

    for connection in indices:
        start_idx, end_idx = connection
        for idx in [start_idx, end_idx]:
            if idx < len(landmarks.landmark):
                landmark = landmarks.landmark[idx]
                x, y = int(landmark.x * frame.shape[1]), int(landmark.y * frame.shape[0])
                min_x, min_y = min(min_x, x), min(min_y, y)
                max_x, max_y = max(max_x, x), max(max_y, y)

    cv2.rectangle(frame, (min_x, min_y), (max_x, max_y), (0, 255, 0), 2)

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
def process_video(video_path):
    if args.connect:
        socket = init_TCP()
    face_detection = mp_face_detection.FaceDetection()
    face_mesh = mp_face_mesh.FaceMesh()
    cap = cv2.VideoCapture(video_path)

    if not cap.isOpened():
        send_message_to_unity(socket, "Failed to connect camera.")
        time.sleep(2)
        send_command_to_unity(socket, "FAIL")
        time.sleep(0.5)
        send_command_to_unity(socket, "EXIT")
        sys.exit()
        return
    send_message_to_unity(socket, "Track the red point on the screen.")
    time.sleep(1)
    send_command_to_unity(socket, 'START')
    time.sleep(3)
    send_message_to_unity(socket, "")

    with mp_face_mesh.FaceMesh(
    static_image_mode=True,
    max_num_faces=1,
    refine_landmarks=True,
    min_detection_confidence=0.3) as face_mesh:

        
        while cap.isOpened():
            success, image = cap.read()
            if not success:
                break
            # Convert the image to RGB
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = face_detection.process(image_rgb)
    
            if results.detections:
                for detection in results.detections:
                    # Use face mesh to get landmarks
                    mesh_results = face_mesh.process(image_rgb)
                    if mesh_results.multi_face_landmarks:
                        for face_landmarks in mesh_results.multi_face_landmarks:
                            # Draw landmarks on the original image, not image_rgb
                            # draw_eye_bounding_box(image, face_landmarks, LEFT_EYE_INDICES)
                            # draw_eye_bounding_box(image, face_landmarks, RIGHT_EYE_INDICES)
    
    
                            rotation_angle = calculate_rotation_angle(face_landmarks.landmark, image)
                            x_min, y_min, x_max, y_max = get_face_roi(face_landmarks.landmark, image)
                            transformed_face = perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle)
                            roll, yaw, pitch = calculate_head_orientation(face_landmarks.landmark)
                            image = cv2.putText(image, f'Roll: {roll:.2f}', (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                            image = cv2.putText(image, f'Yaw: {yaw:.2f}', (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                            image = cv2.putText(image, f'Pitch: {pitch:.2f}', (10, 90), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                             
                            transformed_face_rgb = cv2.cvtColor(transformed_face, cv2.COLOR_BGR2RGB)
                            results_transformed = face_mesh.process(transformed_face_rgb)
    
                            if results_transformed.multi_face_landmarks:
                                for face_landmarks in results_transformed.multi_face_landmarks:
                                    # draw_eye_bounding_box(transformed_face, face_landmarks, LEFT_EYE_INDICES)
                                    # draw_eye_bounding_box(transformed_face, face_landmarks, RIGHT_EYE_INDICES)

                                    left_x = 0
                                    left_y = 0
                                    right_x = 0
                                    right_y = 0
                                    
                                    for index in imp_indexes:
                                        if index < len(face_landmarks.landmark):
                                            iris_landmark = face_landmarks.landmark[index]
                                            if index == 469 or index == 470 or index == 471 or index == 472:
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                right_x+= x 
                                                right_y+= y 
                                                cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                        
                                            elif index == 474 or index == 475 or index == 476 or index == 477:  
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                left_x+= x 
                                                left_y+= y 
                                                cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
                                            else:
                                                
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
                                                
                                    right_x = int(right_x/4)
                                    right_y = int(right_y/4)
                                    left_x = int(left_x/4)
                                    left_y = int(left_y/4)
                                    cv2.circle(transformed_face, (right_x, right_y), 2, (0, 255, 0), -1)
                                    # cv2.circle(transformed_face, (left_x, left_y), 2, (0, 255, 0), -1)
                                    left_eye_left_point_index = 359
                                    left_eye_right_point_index = 362
                                    right_eye_right_point_index = 130
                                    right_eye_left_point_index = 133
                                    L1 = 0 
                                    L2 = 0 
                                    R1 = 0
                                    R2 = 0
                                    for index in eyes_indices:
                                        if index < len(face_landmarks.landmark):
                                            iris_landmark = face_landmarks.landmark[index]
                                            if index == 130:
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                R2 = calculate_distance(x,y,right_x,right_y)
                                            if index == 133:
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                R1 = calculate_distance(x,y,right_x,right_y)
                                            if index == 359:
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                L2 = calculate_distance(x,y,left_x,left_y)
                                            if index == 362:
                                                x = int(iris_landmark.x * transformed_face.shape[1])
                                                y = int(iris_landmark.y * transformed_face.shape[0])
                                                L1 = calculate_distance(x,y,left_x,left_y)
                                                
                                    # print("R1 : " + str(R1))
                                    # print("R2 : " + str(R2))
                                    # print("L1 : " + str(L1))
                                    # print("L2 : " + str(L2))

                                    
                                    leld_list.append(L2)
                                    lerd_list.append(L1)
                                    rerd_list.append(R1)
                                    reld_list.append(R2)

                                    k1= (R1/R2)/(R1+R2)
                                    k2 = (L2/L1)/(L1+L2)
                                    pos_sim = max((k1),(k2))/min((k1),(k2))
                                    pos_sim = round(pos_sim,2)
                                    pos_sim1 = abs(float(R2/R1) - float(L1/L2))
                                    pos_sim2= (R1/R2)*(L2/L1)
                                    pos_similarities.append(pos_sim)
                                    pos_similarities1.append(pos_sim1)
                                    pos_similarities2.append(pos_sim2)
                                    

                                    text = str(pos_sim)
                                    font = cv2.FONT_HERSHEY_SIMPLEX  # You can choose different fonts
                                    position = (50, 50)  # Coordinates of the bottom-left corner of the text string in the image
                                    font_scale = 1  # Font scale factor
                                    color = (255, 0, 0)  # Color in BGR (not RGB, be careful about this)
                                    thickness = 2  # Thickness of the lines used to draw the text
                                    
                                    # Using cv2.putText() method
                                    cv2.putText(transformed_face, text, position, font, font_scale, color, thickness, cv2.LINE_AA)
                                    
                                    

                                    

                                    
                                            

                                            
                                            
                                            
                                                
                                                
                                                


                                        
                                        
                                    
    
                # Display the frames
                cv2.imshow('Original Frame', image)
                cv2.imshow('Transformed Face', transformed_face)
                if cv2.waitKey(5) & 0xFF == ord('q'):
                    break
    
        cap.release()
        cv2.destroyAllWindows()

        if args.connect:
            send_command_to_unity(socket, 'EXIT')

# Replace 'video_path' with your video file path or set it to 0 for webcam
process_video(0)
from datetime import datetime


time_list = [datetime.now().strftime("%Y-%m-%d %H:%M:%S") for _ in range(len(leld_list))]

# Assuming 'Value' is a placeholder here, fill it with a default value or leave empty
value_list = ["LeLD" for _ in range(len(leld_list))]  # Replace "YourValueHere" with your actual data or logic

# Create a DataFrame from your lists
df = pd.DataFrame({
    'Time': time_list,
    'Value': value_list,
    'Positional_Similarity': pos_similarities,
    'LeLd': leld_list,
    'LeRd': lerd_list,
    'ReLd': reld_list,
    'ReRd': rerd_list
})
def normalize_array(arr):
    min_val = np.min(arr)
    max_val = np.max(arr)
    diff = max_val - min_val
    normalized_arr = (arr - min_val) / diff
    return normalized_arr
# Save the DataFrame to a CSV file
df.to_csv('output.csv', index=False)
pd.DataFrame(leld_list, columns=['LeLd']).to_csv('leld.csv', index=False)
pd.DataFrame(lerd_list, columns=['LeRd']).to_csv('lerd.csv', index=False)
pd.DataFrame(reld_list, columns=['ReLd']).to_csv('reld.csv', index=False)
pd.DataFrame(rerd_list, columns=['ReRd']).to_csv('rerd.csv', index=False)
pd.DataFrame(pos_similarities ,columns=['Positional_Similarity']).to_csv('pos_sim_values.csv', index=False)
pd.DataFrame(normalize_array(pos_similarities1) ,columns=['Positional_Similarity1']).to_csv('pos_sim_values(modulus).csv', index=False)
pd.DataFrame(normalize_array(pos_similarities2) ,columns=['Positional_Similarityexp']).to_csv('pos_sim_values_exp.csv', index=False)











































# import cv2
# import mediapipe as mp
# import numpy as np
# import pandas as pd
# # Initialize MediaPipe solutions
# mp_face_detection = mp.solutions.face_detection
# mp_face_mesh = mp.solutions.face_mesh
# mp_drawing = mp.solutions.drawing_utils
# drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)


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

# import math

# def calculate_distance(x1, y1, x2, y2):
#     distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)
#     return int(distance)




# # Initialize MediaPipe solutions
# # mp_face_detection = mp.solutions.face_detection
# # mp_face_mesh = mp.solutions.face_mesh
# # mp_drawing = mp.solutions.drawing_utils
# # drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
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
# LEFT_EYE_INDICES = mp_face_mesh.FACEMESH_LEFT_EYE
# RIGHT_EYE_INDICES = mp_face_mesh.FACEMESH_RIGHT_EYE
# LEFT_IRIS_INDICES = mp_face_mesh.FACEMESH_LEFT_IRIS
# LEFT_IRIS_INDICES = get_unique(LEFT_IRIS_INDICES)
# RIGHT_IRIS_INDICES = mp_face_mesh.FACEMESH_RIGHT_IRIS
# RIGHT_IRIS_INDICES = get_unique(RIGHT_IRIS_INDICES)
# imp_indexes = LEFT_IRIS_INDICES + RIGHT_IRIS_INDICES
# eyes_indices = [130, 133, 359, 362]

# pos_similarities = []
# pos_similarities1 = []
# pos_similarities2 = []
# leld_list=[]
# lerd_list=[]
# rerd_list=[]
# reld_list=[]
# column_names = ['Time', 'Value', 'Positional_Similarity', 'LeLd', 'LeRd', 'ReLd', 'ReRd']
# df = pd.DataFrame(columns=column_names)


# for i in eyes_indices:
#     imp_indexes.append(i)
# def draw_specific_landmarks(frame, landmarks, indices):
#     for connection in indices:
#         if len(connection) == 2:
#             start_idx, end_idx = connection
#             if 0 <= start_idx < len(landmarks.landmark) and 0 <= end_idx < len(landmarks.landmark):
#                 start_landmark = landmarks.landmark[start_idx]
#                 end_landmark = landmarks.landmark[end_idx]
#                 cv2.line(frame, (int(start_landmark.x * frame.shape[1]), int(start_landmark.y * frame.shape[0])),
#                                (int(end_landmark.x * frame.shape[1]), int(end_landmark.y * frame.shape[0])), (0, 255, 0), 1)
# def draw_eye_bounding_box(frame, landmarks, indices):
#     min_x, min_y = frame.shape[1], frame.shape[0]
#     max_x, max_y = 0, 0

#     for connection in indices:
#         start_idx, end_idx = connection
#         for idx in [start_idx, end_idx]:
#             if idx < len(landmarks.landmark):
#                 landmark = landmarks.landmark[idx]
#                 x, y = int(landmark.x * frame.shape[1]), int(landmark.y * frame.shape[0])
#                 min_x, min_y = min(min_x, x), min(min_y, y)
#                 max_x, max_y = max(max_x, x), max(max_y, y)

#     cv2.rectangle(frame, (min_x, min_y), (max_x, max_y), (0, 255, 0), 2)

# def get_face_roi(landmarks, image):
#     """
#     Determine the region of interest of the face based on landmarks.
#     """
#     # Get the bounding box coordinates
#     x_coordinates = [int(landmark.x * image.shape[1]) for landmark in landmarks]
#     y_coordinates = [int(landmark.y * image.shape[0]) for landmark in landmarks]
#     x_min, x_max = min(x_coordinates), max(x_coordinates)
#     y_min, y_max = min(y_coordinates), max(y_coordinates)
#     return x_min, y_min, x_max, y_max
# import math
# def calculate_rotation_angle(landmarks, image):
#     """
#     Calculate the rotation angle of the face based on eye landmarks.
#     """
#     # Define eye landmarks (indices may vary based on MediaPipe's output format)
#     left_eye = landmarks[33]  # Example index for left eye
#     right_eye = landmarks[263] # Example index for right eye
#     # Calculate angle
#     eye_line = [int(right_eye.x * image.shape[1]) - int(left_eye.x * image.shape[1]),
#                 int(right_eye.y * image.shape[0]) - int(left_eye.y * image.shape[0])]
#     angle = math.atan2(eye_line[1], eye_line[0])
#     return math.degrees(angle)
# def rotate_image(image, angle, center=None, scale=1.0):
#     """
#     Rotate the image by a given angle.
#     """
#     (h, w) = image.shape[:2]
#     if center is None:
#         center = (w // 2, h // 2)
#     # Perform the rotation
#     M = cv2.getRotationMatrix2D(center, angle, scale)
#     rotated = cv2.warpAffine(image, M, (w, h))
#     return rotated
# def perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle):
#     """
#     Apply perspective transform and rotation to the face region.
#     """
#     # Rotate the image first
#     rotated_image = rotate_image(image, rotation_angle)
#     # Updated points for perspective transform
#     points1 = np.float32([[x_min, y_min], [x_max, y_min], [x_min, y_max], [x_max, y_max]])
#     points2 = np.float32([[0, 0], [500, 0], [0, 500], [500, 500]])
#     matrix = cv2.getPerspectiveTransform(points1, points2)
#     return cv2.warpPerspective(rotated_image, matrix, (500, 500))
# # OpenCV code to read and process the video frame
# def process_video(video_path):
#     face_detection = mp_face_detection.FaceDetection()
#     face_mesh = mp_face_mesh.FaceMesh()
#     cap = cv2.VideoCapture(video_path)

#     with mp_face_mesh.FaceMesh(
#     static_image_mode=True,
#     max_num_faces=1,
#     refine_landmarks=True,
#     min_detection_confidence=0.3) as face_mesh:

        
#         while cap.isOpened():
#             success, image = cap.read()
#             if not success:
#                 break
#             # Convert the image to RGB
#             image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
#             results = face_detection.process(image_rgb)
    
#             if results.detections:
#                 for detection in results.detections:
#                     # Use face mesh to get landmarks
#                     mesh_results = face_mesh.process(image_rgb)
#                     if mesh_results.multi_face_landmarks:
#                         for face_landmarks in mesh_results.multi_face_landmarks:
#                             # Draw landmarks on the original image, not image_rgb
#                             # draw_eye_bounding_box(image, face_landmarks, LEFT_EYE_INDICES)
#                             # draw_eye_bounding_box(image, face_landmarks, RIGHT_EYE_INDICES)
    
    
#                             rotation_angle = calculate_rotation_angle(face_landmarks.landmark, image)
#                             x_min, y_min, x_max, y_max = get_face_roi(face_landmarks.landmark, image)
#                             transformed_face = perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle)
#                             roll, yaw, pitch = calculate_head_orientation(face_landmarks.landmark)
#                             image = cv2.putText(image, f'Roll: {roll:.2f}', (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                             image = cv2.putText(image, f'Yaw: {yaw:.2f}', (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                             image = cv2.putText(image, f'Pitch: {pitch:.2f}', (10, 90), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                             
#                             transformed_face_rgb = cv2.cvtColor(transformed_face, cv2.COLOR_BGR2RGB)
#                             results_transformed = face_mesh.process(transformed_face_rgb)
    
#                             if results_transformed.multi_face_landmarks:
#                                 for face_landmarks in results_transformed.multi_face_landmarks:
#                                     # draw_eye_bounding_box(transformed_face, face_landmarks, LEFT_EYE_INDICES)
#                                     # draw_eye_bounding_box(transformed_face, face_landmarks, RIGHT_EYE_INDICES)

#                                     left_x = 0
#                                     left_y = 0
#                                     right_x = 0
#                                     right_y = 0
                                    
#                                     for index in imp_indexes:
#                                         if index < len(face_landmarks.landmark):
#                                             iris_landmark = face_landmarks.landmark[index]
#                                             if index == 469 or index == 470 or index == 471 or index == 472:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 right_x+= x 
#                                                 right_y+= y 
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                        
#                                             elif index == 474 or index == 475 or index == 476 or index == 477:  
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 left_x+= x 
#                                                 left_y+= y 
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
#                                             else:
                                                
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
                                                
#                                     right_x = int(right_x/4)
#                                     right_y = int(right_y/4)
#                                     left_x = int(left_x/4)
#                                     left_y = int(left_y/4)
#                                     cv2.circle(transformed_face, (right_x, right_y), 2, (0, 255, 0), -1)
#                                     # cv2.circle(transformed_face, (left_x, left_y), 2, (0, 255, 0), -1)
#                                     left_eye_left_point_index = 359
#                                     left_eye_right_point_index = 362
#                                     right_eye_right_point_index = 130
#                                     right_eye_left_point_index = 133
#                                     L1 = 0 
#                                     L2 = 0 
#                                     R1 = 0
#                                     R2 = 0
#                                     for index in eyes_indices:
#                                         if index < len(face_landmarks.landmark):
#                                             iris_landmark = face_landmarks.landmark[index]
#                                             if index == 130:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 R2 = calculate_distance(x,y,right_x,right_y)
#                                             if index == 133:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 R1 = calculate_distance(x,y,right_x,right_y)
#                                             if index == 359:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 L2 = calculate_distance(x,y,left_x,left_y)
#                                             if index == 362:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 L1 = calculate_distance(x,y,left_x,left_y)
                                                
#                                     # print("R1 : " + str(R1))
#                                     # print("R2 : " + str(R2))
#                                     # print("L1 : " + str(L1))
#                                     # print("L2 : " + str(L2))

                                    
#                                     leld_list.append(L2)
#                                     lerd_list.append(L1)
#                                     rerd_list.append(R1)
#                                     reld_list.append(R2)

#                                     k1= (R1/R2)/(R1+R2)
#                                     k2 = (L2/L1)/(L1+L2)
#                                     pos_sim = max((k1),(k2))/min((k1),(k2))
#                                     pos_sim = round(pos_sim,2)
#                                     pos_sim1 = abs(float(R2/R1) - float(L1/L2))
#                                     pos_sim2= (R1/R2)*(L2/L1)
#                                     pos_similarities.append(pos_sim)
#                                     pos_similarities1.append(pos_sim1)
#                                     pos_similarities2.append(pos_sim2)
                                    

#                                     text = str(pos_sim)
#                                     font = cv2.FONT_HERSHEY_SIMPLEX  # You can choose different fonts
#                                     position = (50, 50)  # Coordinates of the bottom-left corner of the text string in the image
#                                     font_scale = 1  # Font scale factor
#                                     color = (255, 0, 0)  # Color in BGR (not RGB, be careful about this)
#                                     thickness = 2  # Thickness of the lines used to draw the text
                                    
#                                     # Using cv2.putText() method
#                                     cv2.putText(transformed_face, text, position, font, font_scale, color, thickness, cv2.LINE_AA)
                                    
                                    

                                    

                                    
                                            

                                            
                                            
                                            
                                                
                                                
                                                


                                        
                                        
                                    
    
#                 # Display the frames
#                 cv2.imshow('Original Frame', image)
#                 cv2.imshow('Transformed Face', transformed_face)
#                 if cv2.waitKey(5) & 0xFF == ord('q'):
#                     break
    
#         cap.release()
#         cv2.destroyAllWindows()

# # Replace 'video_path' with your video file path or set it to 0 for webcam
# process_video(0)
# from datetime import datetime


# time_list = [datetime.now().strftime("%Y-%m-%d %H:%M:%S") for _ in range(len(leld_list))]

# # Assuming 'Value' is a placeholder here, fill it with a default value or leave empty
# value_list = ["YourValueHere" for _ in range(len(leld_list))]  # Replace "YourValueHere" with your actual data or logic

# # Create a DataFrame from your lists
# df = pd.DataFrame({
#     'Time': time_list,
#     'Value': value_list,
#     'Positional_Similarity': pos_similarities,
#     'LeLd': leld_list,
#     'LeRd': lerd_list,
#     'ReLd': reld_list,
#     'ReRd': rerd_list
# })
# def normalize_array(arr):
#     min_val = np.min(arr)
#     max_val = np.max(arr)
#     diff = max_val - min_val
#     normalized_arr = (arr - min_val) / diff
#     return normalized_arr
# # Save the DataFrame to a CSV file
# df.to_csv('output.csv', index=False)
# pd.DataFrame(leld_list, columns=['LeLd']).to_csv('leld.csv', index=False)
# pd.DataFrame(lerd_list, columns=['LeRd']).to_csv('lerd.csv', index=False)
# pd.DataFrame(reld_list, columns=['ReLd']).to_csv('reld.csv', index=False)
# pd.DataFrame(rerd_list, columns=['ReRd']).to_csv('rerd.csv', index=False)
# pd.DataFrame(pos_similarities ,columns=['Positional_Similarity']).to_csv('pos_sim_values.csv', index=False)
# pd.DataFrame(normalize_array(pos_similarities1) ,columns=['Positional_Similarity1']).to_csv('pos_sim_values(modulus).csv', index=False)
# pd.DataFrame(normalize_array(pos_similarities2) ,columns=['Positional_Similarityexp']).to_csv('pos_sim_values_exp.csv', index=False)









































# # #!/usr/bin/env python
# # # coding: utf-8
# # import sys
# # # In[4]:

# # from argparse import ArgumentParser
# # import mediapipe as mp
# # import cv2
# # import numpy as np
# # import math
# # from datetime import datetime
# # import time
# # import pandas as pd
# # import os
# # import queue

# # # for TCP connection with unity
# # import socket
# # # global variable
# # port = 5066         # have to be same as unity
# # args = None
# # # init TCP connection with unity
# # # return the socket connected
# # def init_TCP():
# #     port = args.port

# #     # '127.0.0.1' = 'localhost' = your computer internal data transmission IP
# #     address = ('127.0.0.1', port)
# #     # address = ('192.168.0.107', port)

# #     try:
# #         s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# #         s.connect(address)
# #         # print(socket.gethostbyname(socket.gethostname()) + "::" + str(port))
# #         print("Connected to address:", socket.gethostbyname(socket.gethostname()) + ":" + str(port))
# #         return s
# #     except OSError as e:
# #         print("Error while connecting :: %s" % e)
        
# #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# #         sys.exit()

# # def send_command_to_unity(s, strarg):
# #     msg = 'CMD:' + strarg

# #     try:
# #         s.send(bytes(msg, "utf-8"))
# #     except socket.error as e:
# #         print("error while sending :: " + str(e))

# #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# #         sys.exit()
# # def send_message_to_unity(s, strarg):
# #     msg = 'MSG:' + strarg

# #     try:
# #         s.send(bytes(msg, "utf-8"))
# #     except socket.error as e:
# #         print("error while sending :: " + str(e))

# #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# #         sys.exit()

# # def send_status_to_unity(s, strarg):
# #     msg = 'STS:' + strarg

# #     try:
# #         s.send(bytes(msg, "utf-8"))
# #     except socket.error as e:
# #         print("error while sending :: " + str(e))

# #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# #         sys.exit()

# # font = cv2.FONT_HERSHEY_SIMPLEX

# # # org
# # org = (50, 50)
# # org1 = (50,100)

# # # fontScale
# # fontScale = 1

# # # Blue color in BGR
# # color = (255, 0, 0)

# # # Line thickness of 2 px
# # thickness = 1
# # """focus_df = pd.read_csv('./Python/Alignment/dis_cal.csv')
# # focus_values = focus_df.values.tolist()
# # focus_values = [item for sublist in focus_values for item in sublist]
# # focus = int(sum(focus_values)/len(focus_values))
# # focus = round(focus,2)"""
# # patientName = "Anonymous"
# # with open('./Python/DPI.txt', 'r') as f:
# #     lines = f.readlines()
# #     patientName = lines[0].strip()
# #     print('patientName: ' + patientName)
# # focus = 800.0
# # try:
# #     with open('./Python/' + patientName + '/focus_final.csv', 'r') as f:
# #         lines = f.readlines()
# #     focus = float(lines[1].split(',')[1])
# # except FileNotFoundError:
# #     if args.connect:
# #         send_message_to_unity(socket, 'Please do Screen Distance checking first')
# #         time.sleep(3)
# #         send_command_to_unity(socket, 'EXIT')
# #     else:
# #         print('Please do Screen Distance checking first')
# #     sys.exit()
# # print('Focus: ' + str(focus))


# # def get_unique(c):
# #     temp_list = list(c)
# #     temp_set = set()
# #     for t in temp_list:
# #         temp_set.add(t[0])
# #         temp_set.add(t[1])
# #     return list(temp_set)
    
# # mp_face_mesh = mp.solutions.face_mesh
# # connections_iris = mp_face_mesh.FACEMESH_IRISES
# # iris_indices = get_unique(connections_iris)

# # connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
# # left_eyes_indices = get_unique(connections_left_eyes)

# # connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
# # right_eyes_indices = get_unique(connections_right_eyes)

# # iris_right_horzn = [469,471]
# # iris_right_vert = [470,472]
# # iris_left_horzn = [474,476]
# # iris_left_vert = [475,477]

# # def slope(x1, y1, x2, y2): # Line slope given two points:
# #     return (y2-y1)/(x2-x1)
# # def angle(s1, s2): 
# #     return math.degrees(math.atan((s2-s1)/(1+(s2*s1))))
# # def distance(x1, y1, x2, y2):
# #     return (((x2 - x1)**2 +(y2 - y1)**2)**0.5)


# # def process_value(value, last_value, start_timestamp, change_timestamp):
# #     if value != last_value:
# #         if change_timestamp is None:
# #             change_timestamp = time.time()
# #         elif time.time() - change_timestamp > 2:
# #             end_timestamp = time.time()
# #             time_range = (start_timestamp, end_timestamp)
# #             return time_range, change_timestamp
# #     else:
# #         change_timestamp = None

# #     return None, change_timestamp

# # leld = []
# # lerd = []
# # reld = []
# # rerd = []

# # pos_sim_values = []
# # temp_leld = 0
# # temp_lerd = 0
# # temp_reld = 0
# # temp_rerd = 0

# # last_value = None
# # start_timestamp = None
# # change_timestamp = None


# # csv_filename = "./Python/Alignment/report_strabisums.csv"
# # if os.path.isfile(csv_filename):
# #     df = pd.read_csv(csv_filename)
# # else:
# #     df = pd.DataFrame(columns=['Start', 'End' , 'Duration' , 'Value'])

# # result_queue = queue.Queue()

# # column_names = ['Time', 'Value', 'Positional_Similarity']

# # # Create an empty DataFrame
# # # df = pd.DataFrame(columns=column_names)

# # # df.to_csv('out_data.csv', index=False)

# # def main():
# #     # Initialize TCP connection
# #     if args.connect:
# #         socket = init_TCP()
# #     # 3 second delay
# #     time.sleep(0.5)
# #     cap = cv2.VideoCapture(args.cameraindex)
# #     if not cap.isOpened():
# #         send_message_to_unity(socket, "Failed to connect camera.")
# #         time.sleep(2)
# #         send_command_to_unity(socket, "FAIL")
# #         time.sleep(0.5)
# #         send_command_to_unity(socket, "EXIT")
# #         #sys.exit()
# #         #return
    
# #     fps = cap.get(cv2.CAP_PROP_FPS)
# #     print(fps)
# #     send_message_to_unity(socket, "Track the red point on the screen.")
# #     time.sleep(1)
# #     send_command_to_unity(socket, 'START')
# #     time.sleep(3)
# #     send_message_to_unity(socket, "")
# #     last_value = None
# #     start_timestamp = None
# #     change_timestamp = None
# #     leld = []
# #     lerd = []
# #     reld = []
# #     rerd = []

# #     pos_sim_values = []
# #     temp_leld = 0
# #     temp_lerd = 0
# #     temp_reld = 0
# #     temp_rerd = 0
# #     column_names = ['Time','Value','Positional_Similarity','LeLd','LeRd','ReLd','ReRd']

# #     df = pd.DataFrame(columns=column_names)
# #     df.to_csv('./Python/' + patientName + '/out_data.csv', index=False)
# #     start_time = time.time()
# #     with mp_face_mesh.FaceMesh(
# #         static_image_mode=True,
# #         max_num_faces=2,
# #         refine_landmarks=True,
# #         min_detection_confidence=0.5) as face_mesh:

# #         while cap.isOpened():
# #             flag = 0
            
# #             ret, frame = cap.read()
# #             if not ret:
# #                 break
                
# #             if time.time() - start_time > (60):
# #                 break

# #             results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))


# #             try:
# #                 for face_landmark in results.multi_face_landmarks:
# #                     lms = face_landmark.landmark
# #                     d= {}
# #                     for index in iris_indices:
# #                         x = int(lms[index].x*frame.shape[1])
# #                         y = int(lms[index].y*frame.shape[0])
# #                         d[index] = (x,y)
# #                     black = np.zeros(frame.shape).astype("uint8")
# #                     for index in iris_indices:
# #                         #print(index)
# #                         cv2.circle(frame,(d[index][0],d[index][1]),2,(0,255,0),-1)
                    
                    
# #                     centre_right_iris_x_1 = int((d[iris_right_horzn[0]][0] + d[iris_right_horzn[1]][0])/2)
# #                     centre_right_iris_y_1 = int((d[iris_right_horzn[0]][1] + d[iris_right_horzn[1]][1])/2)
                    
# #                     centre_right_iris_x_2 = int((d[iris_right_vert[0]][0] + d[iris_right_vert[1]][0])/2)
# #                     centre_right_iris_y_2 = int((d[iris_right_vert[0]][1] + d[iris_right_vert[1]][1])/2)
                    
                        
# #                     centre_left_iris_x_1 = int((d[iris_left_horzn[0]][0] + d[iris_left_horzn[1]][0])/2)
# #                     centre_left_iris_y_1 = int((d[iris_left_horzn[0]][1] + d[iris_left_horzn[1]][1])/2)
                    
# #                     centre_left_iris_x_2 = int((d[iris_left_vert[0]][0] + d[iris_left_vert[1]][0])/2)
# #                     centre_left_iris_y_2 = int((d[iris_left_vert[0]][1] + d[iris_left_vert[1]][1])/2)
                    
# #                     centre_left_iris_x = int((centre_left_iris_x_1 + centre_left_iris_x_2)/2)
# #                     centre_left_iris_y = int((centre_left_iris_y_1 + centre_left_iris_y_2)/2)
                    
# #                     centre_right_iris_x = int((centre_right_iris_x_1 + centre_right_iris_x_2)/2)
# #                     centre_right_iris_y = int((centre_right_iris_y_1 + centre_right_iris_y_2)/2)
                    
                    
# #                     cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
# #                     cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1)
                    
# #                     cv2.circle(black,(centre_right_iris_x,centre_right_iris_y),2,(0,0,255),-1)
# #                     cv2.circle(black,(centre_left_iris_x,centre_left_iris_y),2,(0,0,255),-1)
                    
# #                     w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                    
# #                     W = 6.3
                   
# #                     screen_distance = (W*focus)/w
# #                     screen_distance = int(screen_distance)
# #                     send_status_to_unity(socket, "Distance: " + str(screen_distance))
# #                     frame = cv2.putText(frame, " Distance : " + str(screen_distance), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
                    
# #                     start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

# #                     new_data = {"Time": start, "Distance": str(screen_distance)}
                    
# #                     df = pd.read_csv('./Python/Alignment/screen_face_distance.csv')
# #                     if len(df)>500:
# #                         df = df.iloc[250:]
# #                         df.reset_index(drop=True, inplace=True)
                    
# #                     df = df._concat(new_data, ignore_index=True)
# #                     df.to_csv('./Python/Alignment/screen_face_distance.csv', index=False)

# #                     e= {}
# #                     for index in left_eyes_indices:
# #                         x = int(lms[index].x*frame.shape[1])
# #                         y = int(lms[index].y*frame.shape[0])
# #                         e[index] = (x,y)
# #                     for index in left_eyes_indices:
# #                         #print(index)
# #                         cv2.circle(frame,(e[index][0],e[index][1]),2,(0,255,0),-1)
# #                         cv2.circle(black,(e[index][0],e[index][1]),2,(0,0,255),-1)
# #                         if index == 263 or index == 362:
# #                             cv2.line(black,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
# #                             cv2.line(frame,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
# #                     for conn in list(connections_left_eyes):
# #                         cv2.line(black,(e[conn[0]][0],e[conn[0]][1]),(e[conn[1]][0],e[conn[1]][1]),(0,0,255),1)

# #                     f= {}
# #                     for index in right_eyes_indices:
# #                         x = int(lms[index].x*frame.shape[1])
# #                         y = int(lms[index].y*frame.shape[0])
# #                         f[index] = (x,y)

# #                     for index in right_eyes_indices:
# #                         #print(index)
# #                         cv2.circle(frame,(f[index][0],f[index][1]),2,(0,255,0),-1)
# #                         cv2.circle(black,(f[index][0],f[index][1]),2,(0,0,255),-1)
# #                         if index == 33 or index == 133:
# #                             cv2.line(black,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
# #                             cv2.line(frame,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
# #                     for conn in list(connections_right_eyes):
# #                         cv2.line(black,(f[conn[0]][0],f[conn[0]][1]),(f[conn[1]][0],f[conn[1]][1]),(0,0,255),1)
                    

# #                     left_eye_left_point_index = 263
# #                     left_eye_right_point_index = 398
# #                     right_eye_right_point_index = 33
# #                     right_eye_left_point_index = 133
# #                     df = pd.DataFrame()
# #                     df.to_csv('./Python/Alignment/working1.csv')
# #                     #st.write("I am Screening your eyes")
                    

# #                     le_lp_d = int((((e[left_eye_left_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_left_point_index][1] - centre_left_iris_y)**2)**0.5))
# #                     le_rp_d = int((((e[left_eye_right_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_right_point_index][1] - centre_left_iris_y)**2)**0.5))

# #                     re_lp_d = int((((f[right_eye_left_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_left_point_index][1] - centre_right_iris_y)**2)**0.5))
# #                     re_rp_d = int((((f[right_eye_right_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_right_point_index][1] - centre_right_iris_y)**2)**0.5))
                     
                    
# #                     #frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(latest_data.values[0]), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
               
# #                     df_leld = pd.read_csv('./Python/Alignment/leld.csv')
# #                     data_list_leld = []
# #                     df_lerd = pd.read_csv('./Python/Alignment/lerd.csv')
# #                     data_list_lerd = []
# #                     df_reld = pd.read_csv('./Python/Alignment/reld.csv')
# #                     data_list_reld = []
# #                     df_rerd = pd.read_csv('./Python/Alignment/rerd.csv')
# #                     data_list_rerd = []
                    
                    
# #                     if len(df_leld) < 30:     
# #                         new_data = {'vals': le_lp_d}
# #                         df_leld = df_leld._concat(new_data, ignore_index=True)
# #                         df_leld.to_csv('./Python/Alignment/leld.csv', index=False)
                        
# #                         new_data = {'vals': le_rp_d}
# #                         df_lerd = df_lerd._concat(new_data, ignore_index=True)
# #                         df_lerd.to_csv('./Python/Alignment/lerd.csv', index=False)
                        
# #                         new_data = {'vals': re_lp_d}
# #                         df_reld = df_reld._concat(new_data, ignore_index=True)
# #                         df_reld.to_csv('./Python/Alignment/reld.csv', index=False)
                        
# #                         new_data = {'vals': re_rp_d}
# #                         df_rerd = df_rerd._concat(new_data, ignore_index=True)
# #                         df_rerd.to_csv('./Python/Alignment/rerd.csv', index=False)

                        
                        
# #                     else:
       
# #                         data_list_leld = df_leld.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_leld.columns)
# #                         empty_df.to_csv('./Python/Alignment/leld.csv', index=False)

# #                         data_list_lerd = df_lerd.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_lerd.columns)
# #                         empty_df.to_csv('./Python/Alignment/lerd.csv', index=False)
                        
# #                         data_list_reld = df_reld.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_reld.columns)
# #                         empty_df.to_csv('./Python/Alignment/reld.csv', index=False)
                        
                           
# #                         data_list_rerd = df_rerd.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_rerd.columns)
# #                         empty_df.to_csv('./Python/Alignment/rerd.csv', index=False)

                        
                        
                        
# #     #                 frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(len(data_list_leld)), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)  
# #     #                 frame = cv2.putText(img, "right Eye right Point Distance : " + str(len(data_list_reld)), (50,200), font, fontScale, color, thickness, cv2.LINE_AA)
# #     #                 frame = cv2.putText(img, "right Eye Left Point Distance : " + str(len(data_list_lerd)), (50,250), font, fontScale, color, thickness, cv2.LINE_AA)
# #     #                 frame = cv2.putText(img, "Left Eye right Point Distance : " + str(len(data_list_rerd)), (50,300), font, fontScale, color, thickness, cv2.LINE_AA)
                        
                    

# #                     if len(data_list_leld) == 30:
                        

                        
# #     #                     leld = [item for sublist in data_list_leld for item in sublist]
# #     #                     #lerd = data_list_lerd
# #     #                     lerd = [item for sublist in data_list_lerd for item in sublist]
# #     #                     #reld = data_list_reld
# #     #                     reld = [item for sublist in data_list_reld for item in sublist]
# #     #                     #rerd = data_list_rerd
# #     #                     rerd = [item for sublist in data_list_rerd for item in sublist]
                
# #                         data_list_leld = [item for sublist in data_list_leld for item in sublist]
# #                         temp_leld = int(sum(data_list_leld)/len(data_list_leld))

# #                         data_list_reld = [item for sublist in data_list_reld for item in sublist]
# #                         temp_reld = int(sum(data_list_reld)/len(data_list_reld))
                        

# #                         data_list_lerd = [item for sublist in data_list_lerd for item in sublist]
# #                         temp_lerd = int(sum(data_list_lerd)/len(data_list_lerd))
# #                         data_list_rerd = [item for sublist in data_list_rerd for item in sublist]
# #                         temp_rerd = int(sum(data_list_rerd)/len(data_list_rerd))

# #                         L2 = temp_leld
# #                         L1 = temp_lerd
# #                         R1 = temp_reld
# #                         R2 = temp_rerd

# #                         #pos_sim = max((R1/R2),(L1/L2))/min((R1/R2),(L1/L2))
# #                         pos_sim = R1 * L1 / R2 / L2
# #                         pos_sim_values.concat(pos_sim)

# #                         value = "Normal"
# #                         if pos_sim > 1.42:
# #                             value = "Strabismus"
# #                         else:
# #                             value = "Normal"
                        
                     

# #                         start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

# #                         new_data = {"Time": start, "Value": str(value),"Positional_Similarity":str(round(pos_sim,2)),
# #                                    "LeLd":L2,"LeRd":L1,"ReLd":R1,"ReRd":R2}
                        
                        
# #                         df = pd.read_csv('./Python/' + patientName + '/out_data.csv')
               
# #                         #df = df.concat(data, ignore_index=True)
                          
# #                         df = df._concat(new_data, ignore_index=True)
# #                         df.to_csv('./Python/' + patientName + '/out_data.csv', index=False)
                        
# #                     if not args.quiet:
# #                         cv2.imshow("final", frame)
# #                     if cv2.waitKey(1) & 0xFF == ord('q'):
# #                         flag = 1
# #                         break

# #             except Exception as e:
# #                 print(e)
            
# #             if flag == 1:
# #                 break
# #     cap.release()
# #     cv2.destroyAllWindows()
# #     # In[2]:
# #     # In[3]:
    
# #     def custom_date_parser(date_string):
# #         return pd.to_datetime(date_string, format="%d/%m/%y %H:%M:%S")
# #     csv_file = './Python/' + patientName + '/out_data.csv'
# #     df = pd.read_csv(csv_file, parse_dates=['Time'], date_parser=custom_date_parser)

# #             # Create bins for the 'Positional_Similarity' column with 0.25 intervals
# #     bins = [i * 0.25 for i in range(41)]  # 41 since we want to include 10 (0.25 * 40 = 10)
# #     labels = [f'{i * 0.25}-{(i + 1) * 0.25}' for i in range(40)]  # 40 intervals in total
# #     df['pos_similarity_interval'] = pd.cut(df['Positional_Similarity'], bins=bins, labels=labels)

# #     # Calculate the time duration for each row
# #     df['duration'] = df['Time'].diff()

# #     # Group the DataFrame by the binned 'pos_similarity_interval' column
# #     grouped_df = df.groupby('pos_similarity_interval')['duration'].sum().reset_index()

# #     # Convert the time intervals to seconds and remove bins with no or None values
# #     grouped_df['duration_seconds'] = grouped_df['duration'].dt.total_seconds()
# #     grouped_df.dropna(subset=['duration_seconds'], inplace=True)
# #     grouped_df.reset_index(drop=True, inplace=True)

# #     grouped_df = grouped_df[['pos_similarity_interval', 'duration_seconds']]


# #     # In[5]:


# #     grouped_df.to_csv('./Python/' + patientName + '/grouped_output.csv')
# #     if args.connect:
# #         send_command_to_unity(socket, 'EXIT')

# #     # In[ ]:






# #     # In[5]:


# #     grouped_df


# #     # In[ ]:
# # if __name__ == "__main__":

# #     parser = ArgumentParser()

# #     parser.add_argument("--connect", action="store_true",
# #                         help="connect to unity",
# #                         default=False)
                        
# #     parser.add_argument("--quiet", action="store_true",
# #                         help="hide window",
# #                         default=False)

# #     parser.add_argument("--port", type=int, 
# #                         help="specify the port of the connection to unity. Have to be the same as in Unity", 
# #                         default=5066)
# #     parser.add_argument("--cameraindex", type=int, 
# #                         help="specify the web camera index", 
# #                         default=0)
    
# #     args = parser.parse_args()

# #     # demo code
# #     main()





# #!/usr/bin/env python
# # coding: utf-8

# # In[1]:

# # from argparse import ArgumentParser
# # import mediapipe as mp
# # import cv2
# # import numpy as np
# # import math
# # from datetime import datetime
# # import time
# # import pandas as pd
# # import os
# # import queue
# # import socket
# # import sys
# # # port = 5066
# # # args = None
# # # def init_TCP():
# # #     port = args.port

# # #     # '127.0.0.1' = 'localhost' = your computer internal data transmission IP
# # #     address = ('127.0.0.1', port)
# # #     # address = ('192.168.0.107', port)

# # #     try:
# # #         s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# # #         s.connect(address)
# # #         # print(socket.gethostbyname(socket.gethostname()) + "::" + str(port))
# # #         print("Connected to address:", socket.gethostbyname(socket.gethostname()) + ":" + str(port))
# # #         return s
# # #     except OSError as e:
# # #         print("Error while connecting :: %s" % e)
        
# # #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# # #         sys.exit()

# # # def send_command_to_unity(s, strarg):
# # #     msg = 'CMD:' + strarg

# # #     try:
# # #         s.send(bytes(msg, "utf-8"))
# # #     except socket.error as e:
# # #         print("error while sending :: " + str(e))

# # #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# # #         sys.exit()
# # # def send_message_to_unity(s, strarg):
# # #     msg = 'MSG:' + strarg

# # #     try:
# # #         s.send(bytes(msg, "utf-8"))
# # #     except socket.error as e:
# # #         print("error while sending :: " + str(e))

# # #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# # #         sys.exit()

# # # def send_status_to_unity(s, strarg):
# # #     msg = 'STS:' + strarg

# # #     try:
# # #         s.send(bytes(msg, "utf-8"))
# # #     except socket.error as e:
# # #         print("error while sending :: " + str(e))

# # #         # quit the script if connection fails (e.g. Unity server side quits suddenly)
# # #         sys.exit()


# # font = cv2.FONT_HERSHEY_SIMPLEX

# # # org
# # org = (50, 50)
# # org1 = (50,100)

# # # fontScale
# # fontScale = 1

# # # Blue color in BGR
# # color = (255, 0, 0)

# # # Line thickness of 2 px
# # thickness = 1
# # focus_df = pd.read_csv('H:\Vpower2\perfect-vision\Python\Alignment\dis_cal.csv') # focal length of camera shayd
# # focus_values = focus_df.values.tolist()
# # focus_values = [item for sublist in focus_values for item in sublist]
# # focus = int(sum(focus_values)/len(focus_values))
# # focus = round(focus,2)


# # def get_unique(c):
# #     temp_list = list(c)
# #     temp_set = set()
# #     for t in temp_list:
# #         temp_set.add(t[0])
# #         temp_set.add(t[1])
# #     return list(temp_set)
    
# # mp_face_mesh = mp.solutions.face_mesh
# # connections_iris = mp_face_mesh.FACEMESH_IRISES
# # iris_indices = get_unique(connections_iris)

# # connections_left_eyes =  mp_face_mesh.FACEMESH_LEFT_EYE
# # left_eyes_indices = get_unique(connections_left_eyes)

# # connections_right_eyes =  mp_face_mesh.FACEMESH_RIGHT_EYE
# # right_eyes_indices = get_unique(connections_right_eyes)

# # iris_right_horzn = [469,471]
# # iris_right_vert = [470,472]
# # iris_left_horzn = [474,476]
# # iris_left_vert = [475,477]

# # def slope(x1, y1, x2, y2): # Line slope given two points:
# #     return (y2-y1)/(x2-x1)
# # def angle(s1, s2): 
# #     return math.degrees(math.atan((s2-s1)/(1+(s2*s1))))
# # def distance(x1, y1, x2, y2):
# #     return (((x2 - x1)**2 +(y2 - y1)**2)**0.5)


# # def process_value(value, last_value, start_timestamp, change_timestamp):
# #     if value != last_value:
# #         if change_timestamp is None:
# #             change_timestamp = time.time()
# #         elif time.time() - change_timestamp > 2:
# #             end_timestamp = time.time()
# #             time_range = (start_timestamp, end_timestamp)
# #             return time_range, change_timestamp
# #     else:
# #         change_timestamp = None

# #     return None, change_timestamp

# # leld = []
# # lerd = []
# # reld = []
# # rerd = []

# # pos_sim_values = []
# # temp_leld = 0
# # temp_lerd = 0
# # temp_reld = 0
# # temp_rerd = 0

# # last_value = None
# # start_timestamp = None
# # change_timestamp = None


# # csv_filename = "./Python/Alignment/report_strabisums.csv"
# # if os.path.isfile(csv_filename):
# #     df = pd.read_csv(csv_filename)
# # else:
# #     df = pd.DataFrame(columns=['Start', 'End' , 'Duration' , 'Value'])

# # result_queue = queue.Queue()

# # column_names = ['Time', 'Value', 'Positional_Similarity']

# # # Create an empty DataFrame
# # # df = pd.DataFrame(columns=column_names)

# # # df.to_csv('out_data.csv', index=False)
# # # parser = ArgumentParser()

# # # parser.add_argument("--connect", action="store_true",
# # #                     help="connect to unity",
# # #                     default=False)
                    
# # # parser.add_argument("--quiet", action="store_true",
# # #                     help="hide window",
# # #                     default=False)

# # # parser.add_argument("--port", type=int, 
# # #                     help="specify the port of the connection to unity. Have to be the same as in Unity", 
# # #                     default=5066)
# # # parser.add_argument("--cameraindex", type=int, 
# # #                     help="specify the web camera index", 
# # #                     default=0)

# # # args = parser.parse_args()
# # # if args.connect:
# # #         socket = init_TCP()
# # cap = cv2.VideoCapture(0)
# # fps = cap.get(cv2.CAP_PROP_FPS)
# # print(fps)


# # last_value = None
# # start_timestamp = None
# # change_timestamp = None
# # leld = []
# # lerd = []
# # reld = []
# # rerd = []

# # pos_sim_values = []
# # temp_leld = 0
# # temp_lerd = 0
# # temp_reld = 0
# # temp_rerd = 0
# # column_names = ['Time','Value','Positional_Similarity','LeLd','LeRd','ReLd','ReRd']

# # df = pd.DataFrame(columns=column_names)
# # df.to_csv('./Python/' + '/out_data.csv', index=False)# use in unity

# # with mp_face_mesh.FaceMesh(
# #     static_image_mode=True,
# #     max_num_faces=2,
# #     refine_landmarks=True,
# #     min_detection_confidence=0.5) as face_mesh:

# #     while cap.isOpened():
# #         flag = 0

# #         ret, frame = cap.read()
# #         if not ret:
# #             break

# #         results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))


# #         try:
# #             for face_landmark in results.multi_face_landmarks:
# #                 lms = face_landmark.landmark
# #                 d= {}
# #                 for index in iris_indices:
# #                     x = int(lms[index].x*frame.shape[1])
# #                     y = int(lms[index].y*frame.shape[0])
# #                     d[index] = (x,y)
# #                 black = np.zeros(frame.shape).astype("uint8")
# #                 for index in iris_indices:
# #                     #print(index)
# #                     cv2.circle(frame,(d[index][0],d[index][1]),2,(0,255,0),-1)
                
                
# #                 centre_right_iris_x_1 = int((d[iris_right_horzn[0]][0] + d[iris_right_horzn[1]][0])/2)
# #                 centre_right_iris_y_1 = int((d[iris_right_horzn[0]][1] + d[iris_right_horzn[1]][1])/2)
                
# #                 centre_right_iris_x_2 = int((d[iris_right_vert[0]][0] + d[iris_right_vert[1]][0])/2)
# #                 centre_right_iris_y_2 = int((d[iris_right_vert[0]][1] + d[iris_right_vert[1]][1])/2)
                
                    
# #                 centre_left_iris_x_1 = int((d[iris_left_horzn[0]][0] + d[iris_left_horzn[1]][0])/2)
# #                 centre_left_iris_y_1 = int((d[iris_left_horzn[0]][1] + d[iris_left_horzn[1]][1])/2)
                
# #                 centre_left_iris_x_2 = int((d[iris_left_vert[0]][0] + d[iris_left_vert[1]][0])/2)
# #                 centre_left_iris_y_2 = int((d[iris_left_vert[0]][1] + d[iris_left_vert[1]][1])/2)
                
# #                 centre_left_iris_x = int((centre_left_iris_x_1 + centre_left_iris_x_2)/2)
# #                 centre_left_iris_y = int((centre_left_iris_y_1 + centre_left_iris_y_2)/2)
                
# #                 centre_right_iris_x = int((centre_right_iris_x_1 + centre_right_iris_x_2)/2)
# #                 centre_right_iris_y = int((centre_right_iris_y_1 + centre_right_iris_y_2)/2)
                
                
# #                 cv2.circle(frame,(centre_right_iris_x,centre_right_iris_y),2,(0,255,0),-1)
# #                 cv2.circle(frame,(centre_left_iris_x,centre_left_iris_y),2,(0,255,0),-1)
                
# #                 cv2.circle(black,(centre_right_iris_x,centre_right_iris_y),2,(0,0,255),-1)
# #                 cv2.circle(black,(centre_left_iris_x,centre_left_iris_y),2,(0,0,255),-1)
                
# #                 w = ((centre_right_iris_x - centre_left_iris_x)**2 + (centre_right_iris_y - centre_left_iris_y)**2)**0.5
                
# #                 W = 6.3
               
# #                 screen_distance = (W*focus)/w
# #                 screen_distance = int(screen_distance)
                
# #                 frame = cv2.putText(frame, " Distance : " + str(screen_distance), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
                
# #                 start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

# #                 new_data = {"Time": start, "Distance": str(screen_distance)}
                
# #                 df = pd.read_csv('./Python/Alignment/screen_face_distance.csv')
# #                 if len(df)>500:
# #                     df = df.iloc[250:]
# #                     df.reset_index(drop=True, inplace=True)
                
# #                 # df = df.concat(new_data, ignore_index=True)
# #                 df = pd.concat([df, pd.DataFrame([new_data])], ignore_index=True)
# #                 df.to_csv('./Python/Alignment/screen_face_distance.csv', index=False)

# #                 e= {}
# #                 for index in left_eyes_indices:
# #                     x = int(lms[index].x*frame.shape[1])
# #                     y = int(lms[index].y*frame.shape[0])
# #                     e[index] = (x,y)
# #                 for index in left_eyes_indices:
# #                     #print(index)
# #                     cv2.circle(frame,(e[index][0],e[index][1]),2,(0,255,0),-1)
# #                     cv2.circle(black,(e[index][0],e[index][1]),2,(0,0,255),-1)
# #                     if index == 263 or index == 362:
# #                         cv2.line(black,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
# #                         cv2.line(frame,(e[index][0],e[index][1]),(centre_left_iris_x,centre_left_iris_y),(0,0,255),1)
# #                 for conn in list(connections_left_eyes):
# #                     cv2.line(black,(e[conn[0]][0],e[conn[0]][1]),(e[conn[1]][0],e[conn[1]][1]),(0,0,255),1)

# #                 f= {}
# #                 for index in right_eyes_indices:
# #                     x = int(lms[index].x*frame.shape[1])
# #                     y = int(lms[index].y*frame.shape[0])
# #                     f[index] = (x,y)

# #                 for index in right_eyes_indices:
# #                     #print(index)
# #                     cv2.circle(frame,(f[index][0],f[index][1]),2,(0,255,0),-1)
# #                     cv2.circle(black,(f[index][0],f[index][1]),2,(0,0,255),-1)
# #                     if index == 33 or index == 133:
# #                         cv2.line(black,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
# #                         cv2.line(frame,(f[index][0],f[index][1]),(centre_right_iris_x,centre_right_iris_y),(0,0,255),1)
# #                 for conn in list(connections_right_eyes):
# #                     cv2.line(black,(f[conn[0]][0],f[conn[0]][1]),(f[conn[1]][0],f[conn[1]][1]),(0,0,255),1)
                

# #                 left_eye_left_point_index = 263
# #                 left_eye_right_point_index = 398
# #                 right_eye_right_point_index = 33
# #                 right_eye_left_point_index = 133
# #                 df = pd.DataFrame()
# #                 df.to_csv('./Python/Alignment/working1.csv')
# #                 #st.write("I am Screening your eyes")
                

# #                 le_lp_d = int((((e[left_eye_left_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_left_point_index][1] - centre_left_iris_y)**2)**0.5))
# #                 le_rp_d = int((((e[left_eye_right_point_index][0] - centre_left_iris_x)**2 +(e[left_eye_right_point_index][1] - centre_left_iris_y)**2)**0.5))

# #                 re_lp_d = int((((f[right_eye_left_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_left_point_index][1] - centre_right_iris_y)**2)**0.5))
# #                 re_rp_d = int((((f[right_eye_right_point_index][0] - centre_right_iris_x)**2 +(f[right_eye_right_point_index][1] - centre_right_iris_y)**2)**0.5))
                 
                
# #                 #frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(latest_data.values[0]), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)
# #                 df_leld = pd.read_csv('./Python/Alignment/leld.csv')
# #                 data_list_leld = []
# #                 df_lerd = pd.read_csv('./Python/Alignment/lerd.csv')
# #                 data_list_lerd = []
# #                 df_reld = pd.read_csv('./Python/Alignment/reld.csv')
# #                 data_list_reld = []
# #                 df_rerd = pd.read_csv('./Python/Alignment/rerd.csv')
# #                 data_list_rerd = []
                   
                
# #                 if len(df_leld) < 30:     
# #                     new_data = {'vals': le_lp_d}
# #                     df_leld = pd.concat([df_leld, pd.DataFrame([new_data])], ignore_index=True)
# #                     df_leld.to_csv('./Python/Alignment/leld.csv', index=False)
                    
# #                     new_data = {'vals': le_rp_d}
# #                     df_lerd = pd.concat([df_lerd, pd.DataFrame([new_data])], ignore_index=True)
# #                     df_lerd.to_csv('./Python/Alignment/lerd.csv', index=False)
                    
# #                     new_data = {'vals': re_lp_d}
# #                     df_reld = pd.concat([df_reld, pd.DataFrame([new_data])], ignore_index=True)
# #                     df_reld.to_csv('./Python/Alignment/reld.csv', index=False)
                    
# #                     new_data = {'vals': re_rp_d}
# #                     df_rerd = pd.concat([df_rerd, pd.DataFrame([new_data])], ignore_index=True)
# #                     df_rerd.to_csv('./Python/Alignment/rerd.csv', index=False)
                    
                    
# #                 else:
       
# #                         data_list_leld = df_leld.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_leld.columns)
# #                         empty_df.to_csv('./Python/Alignment/leld.csv', index=False)

# #                         data_list_lerd = df_lerd.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_lerd.columns)
# #                         empty_df.to_csv('./Python/Alignment/lerd.csv', index=False)
                        
# #                         data_list_reld = df_reld.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_reld.columns)
# #                         empty_df.to_csv('./Python/Alignment/reld.csv', index=False)
                        
                           
# #                         data_list_rerd = df_rerd.values.tolist()
# #                         empty_df = pd.DataFrame(columns=df_rerd.columns)
# #                         empty_df.to_csv('./Python/Alignment/rerd.csv', index=False)

                    
                    
                    
# # #                 frame = cv2.putText(img, "Left Eye Left Point Distance : " + str(len(data_list_leld)), (50,150), font, fontScale, color, thickness, cv2.LINE_AA)  
# # #                 frame = cv2.putText(img, "right Eye right Point Distance : " + str(len(data_list_reld)), (50,200), font, fontScale, color, thickness, cv2.LINE_AA)
# # #                 frame = cv2.putText(img, "right Eye Left Point Distance : " + str(len(data_list_lerd)), (50,250), font, fontScale, color, thickness, cv2.LINE_AA)
# # #                 frame = cv2.putText(img, "Left Eye right Point Distance : " + str(len(data_list_rerd)), (50,300), font, fontScale, color, thickness, cv2.LINE_AA)
                    
                

# #                 if len(data_list_leld) == 30:
                    

                    
# # #                     leld = [item for sublist in data_list_leld for item in sublist]
# # #                     #lerd = data_list_lerd
# # #                     lerd = [item for sublist in data_list_lerd for item in sublist]
# # #                     #reld = data_list_reld
# # #                     reld = [item for sublist in data_list_reld for item in sublist]
# # #                     #rerd = data_list_rerd
# # #                     rerd = [item for sublist in data_list_rerd for item in sublist]
            
# #                     data_list_leld = [item for sublist in data_list_leld for item in sublist]
# #                     temp_leld = int(sum(data_list_leld)/len(data_list_leld))

# #                     data_list_reld = [item for sublist in data_list_reld for item in sublist]
# #                     temp_reld = int(sum(data_list_reld)/len(data_list_reld))
                    

# #                     data_list_lerd = [item for sublist in data_list_lerd for item in sublist]
# #                     temp_lerd = int(sum(data_list_lerd)/len(data_list_lerd))
# #                     data_list_rerd = [item for sublist in data_list_rerd for item in sublist]
# #                     temp_rerd = int(sum(data_list_rerd)/len(data_list_rerd))

# #                     L2 = temp_leld
# #                     L1 = temp_lerd
# #                     R1 = temp_reld
# #                     R2 = temp_rerd

# #                     pos_sim = max((R1/R2),(L1/L2))/min((R1/R2),(L1/L2))
# #                     pos_sim_values.append(pos_sim)

# #                     value = "Normal"
# #                     if pos_sim > 1.42:
# #                         value = "Strabismus"
# #                     else:
# #                         value = "Normal"
                    
                 

# #                     start = datetime.now().strftime("%d/%m/%y %H:%M:%S")

# #                     new_data = {"Time": start, "Value": str(value),"Positional_Similarity":str(round(pos_sim,2)),
# #                                "LeLd":L2,"LeRd":L1,"ReLd":R1,"ReRd":R2}
                    
# #                     df = pd.read_csv('./Python/' + '/out_data.csv')
           
# #                     #df = df.concat(data, ignore_index=True)
                      
# #                     df = pd.concat([df, pd.DataFrame([new_data])], ignore_index=True)
# #                     df.to_csv('./Python/out_data.csv', index=False)
                    
                    
# #                 cv2.imshow("final", frame)
# #                 if cv2.waitKey(1) & 0xFF == ord('q'):
# #                     flag = 1
# #                     break

# #         except Exception as e:
# #             print(e)
        
# #         if flag == 1:
# #             break
# # cap.release()
# # cv2.destroyAllWindows()


# import cv2
# import mediapipe as mp
# import numpy as np
# import pandas as pd
# # Initialize MediaPipe solutions
# mp_face_detection = mp.solutions.face_detection
# mp_face_mesh = mp.solutions.face_mesh
# mp_drawing = mp.solutions.drawing_utils
# drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)


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

# import math

# def calculate_distance(x1, y1, x2, y2):
#     distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)
#     return int(distance)


# import cv2
# import mediapipe as mp
# import numpy as np
# # Initialize MediaPipe solutions
# mp_face_detection = mp.solutions.face_detection
# mp_face_mesh = mp.solutions.face_mesh
# mp_drawing = mp.solutions.drawing_utils
# drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
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
# LEFT_EYE_INDICES = mp_face_mesh.FACEMESH_LEFT_EYE
# RIGHT_EYE_INDICES = mp_face_mesh.FACEMESH_RIGHT_EYE
# LEFT_IRIS_INDICES = mp_face_mesh.FACEMESH_LEFT_IRIS
# LEFT_IRIS_INDICES = get_unique(LEFT_IRIS_INDICES)
# RIGHT_IRIS_INDICES = mp_face_mesh.FACEMESH_RIGHT_IRIS
# RIGHT_IRIS_INDICES = get_unique(RIGHT_IRIS_INDICES)
# imp_indexes = LEFT_IRIS_INDICES + RIGHT_IRIS_INDICES
# eyes_indices = [130, 133, 359, 362]

# pos_similarities = []
# for i in eyes_indices:
#     imp_indexes.append(i)
# def draw_specific_landmarks(frame, landmarks, indices):
#     for connection in indices:
#         if len(connection) == 2:
#             start_idx, end_idx = connection
#             if 0 <= start_idx < len(landmarks.landmark) and 0 <= end_idx < len(landmarks.landmark):
#                 start_landmark = landmarks.landmark[start_idx]
#                 end_landmark = landmarks.landmark[end_idx]
#                 cv2.line(frame, (int(start_landmark.x * frame.shape[1]), int(start_landmark.y * frame.shape[0])),
#                                (int(end_landmark.x * frame.shape[1]), int(end_landmark.y * frame.shape[0])), (0, 255, 0), 1)
# def draw_eye_bounding_box(frame, landmarks, indices):
#     min_x, min_y = frame.shape[1], frame.shape[0]
#     max_x, max_y = 0, 0

#     for connection in indices:
#         start_idx, end_idx = connection
#         for idx in [start_idx, end_idx]:
#             if idx < len(landmarks.landmark):
#                 landmark = landmarks.landmark[idx]
#                 x, y = int(landmark.x * frame.shape[1]), int(landmark.y * frame.shape[0])
#                 min_x, min_y = min(min_x, x), min(min_y, y)
#                 max_x, max_y = max(max_x, x), max(max_y, y)

#     cv2.rectangle(frame, (min_x, min_y), (max_x, max_y), (0, 255, 0), 2)

# def get_face_roi(landmarks, image):
#     """
#     Determine the region of interest of the face based on landmarks.
#     """
#     # Get the bounding box coordinates
#     x_coordinates = [int(landmark.x * image.shape[1]) for landmark in landmarks]
#     y_coordinates = [int(landmark.y * image.shape[0]) for landmark in landmarks]
#     x_min, x_max = min(x_coordinates), max(x_coordinates)
#     y_min, y_max = min(y_coordinates), max(y_coordinates)
#     return x_min, y_min, x_max, y_max
# import math
# def calculate_rotation_angle(landmarks, image):
#     """
#     Calculate the rotation angle of the face based on eye landmarks.
#     """
#     # Define eye landmarks (indices may vary based on MediaPipe's output format)
#     left_eye = landmarks[33]  # Example index for left eye
#     right_eye = landmarks[263] # Example index for right eye
#     # Calculate angle
#     eye_line = [int(right_eye.x * image.shape[1]) - int(left_eye.x * image.shape[1]),
#                 int(right_eye.y * image.shape[0]) - int(left_eye.y * image.shape[0])]
#     angle = math.atan2(eye_line[1], eye_line[0])
#     return math.degrees(angle)
# def rotate_image(image, angle, center=None, scale=1.0):
#     """
#     Rotate the image by a given angle.
#     """
#     (h, w) = image.shape[:2]
#     if center is None:
#         center = (w // 2, h // 2)
#     # Perform the rotation
#     M = cv2.getRotationMatrix2D(center, angle, scale)
#     rotated = cv2.warpAffine(image, M, (w, h))
#     return rotated
# def perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle):
#     """
#     Apply perspective transform and rotation to the face region.
#     """
#     # Rotate the image first
#     rotated_image = rotate_image(image, rotation_angle)
#     # Updated points for perspective transform
#     points1 = np.float32([[x_min, y_min], [x_max, y_min], [x_min, y_max], [x_max, y_max]])
#     points2 = np.float32([[0, 0], [500, 0], [0, 500], [500, 500]])
#     matrix = cv2.getPerspectiveTransform(points1, points2)
#     return cv2.warpPerspective(rotated_image, matrix, (500, 500))
# # OpenCV code to read and process the video frame
# def process_video(video_path):
#     face_detection = mp_face_detection.FaceDetection()
#     face_mesh = mp_face_mesh.FaceMesh()
#     cap = cv2.VideoCapture(video_path)

#     with mp_face_mesh.FaceMesh(
#     static_image_mode=True,
#     max_num_faces=1,
#     refine_landmarks=True,
#     min_detection_confidence=0.3) as face_mesh:

        
#         while cap.isOpened():
#             success, image = cap.read()
#             if not success:
#                 break
#             # Convert the image to RGB
#             image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
#             results = face_detection.process(image_rgb)
    
#             if results.detections:
#                 for detection in results.detections:
#                     # Use face mesh to get landmarks
#                     mesh_results = face_mesh.process(image_rgb)
#                     if mesh_results.multi_face_landmarks:
#                         for face_landmarks in mesh_results.multi_face_landmarks:
#                             # Draw landmarks on the original image, not image_rgb
#                             # draw_eye_bounding_box(image, face_landmarks, LEFT_EYE_INDICES)
#                             # draw_eye_bounding_box(image, face_landmarks, RIGHT_EYE_INDICES)
    
    
#                             rotation_angle = calculate_rotation_angle(face_landmarks.landmark, image)
#                             x_min, y_min, x_max, y_max = get_face_roi(face_landmarks.landmark, image)
#                             transformed_face = perspective_transform(image, x_min, y_min, x_max, y_max, rotation_angle)
#                             roll, yaw, pitch = calculate_head_orientation(face_landmarks.landmark)
#                             image = cv2.putText(image, f'Roll: {roll:.2f}', (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                             image = cv2.putText(image, f'Yaw: {yaw:.2f}', (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
#                             image = cv2.putText(image, f'Pitch: {pitch:.2f}', (10, 90), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                             
#                             transformed_face_rgb = cv2.cvtColor(transformed_face, cv2.COLOR_BGR2RGB)
#                             results_transformed = face_mesh.process(transformed_face_rgb)
    
#                             if results_transformed.multi_face_landmarks:
#                                 for face_landmarks in results_transformed.multi_face_landmarks:
#                                     # draw_eye_bounding_box(transformed_face, face_landmarks, LEFT_EYE_INDICES)
#                                     # draw_eye_bounding_box(transformed_face, face_landmarks, RIGHT_EYE_INDICES)

#                                     left_x = 0
#                                     left_y = 0
#                                     right_x = 0
#                                     right_y = 0
                                    
#                                     for index in imp_indexes:
#                                         if index < len(face_landmarks.landmark):
#                                             iris_landmark = face_landmarks.landmark[index]
#                                             if index == 469 or index == 470 or index == 471 or index == 472:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 right_x+= x 
#                                                 right_y+= y 
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                        
#                                             elif index == 474 or index == 475 or index == 476 or index == 477:  
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 left_x+= x 
#                                                 left_y+= y 
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
#                                             else:
                                                
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 cv2.circle(transformed_face, (x, y), 2, (0, 255, 0), -1)
                                                
                                                
#                                     right_x = int(right_x/4)
#                                     right_y = int(right_y/4)
#                                     left_x = int(left_x/4)
#                                     left_y = int(left_y/4)
#                                     cv2.circle(transformed_face, (right_x, right_y), 2, (0, 255, 0), -1)
#                                     # cv2.circle(transformed_face, (left_x, left_y), 2, (0, 255, 0), -1)
#                                     left_eye_left_point_index = 359
#                                     left_eye_right_point_index = 362
#                                     right_eye_right_point_index = 130
#                                     right_eye_left_point_index = 133
#                                     L1 = 0 
#                                     L2 = 0 
#                                     R1 = 0
#                                     R2 = 0
#                                     for index in eyes_indices:
#                                         if index < len(face_landmarks.landmark):
#                                             iris_landmark = face_landmarks.landmark[index]
#                                             if index == 130:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 R2 = calculate_distance(x,y,right_x,right_y)
#                                             if index == 133:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 R1 = calculate_distance(x,y,right_x,right_y)
#                                             if index == 359:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 L2 = calculate_distance(x,y,left_x,left_y)
#                                             if index == 362:
#                                                 x = int(iris_landmark.x * transformed_face.shape[1])
#                                                 y = int(iris_landmark.y * transformed_face.shape[0])
#                                                 L1 = calculate_distance(x,y,left_x,left_y)
                                                
#                                     # print("R1 : " + str(R1))
#                                     # print("R2 : " + str(R2))
#                                     # print("L1 : " + str(L1))
#                                     # print("L2 : " + str(L2))

#                                     k1= (R1/R2)/(R1+R2)
#                                     k2 = (L2/L1)/(L1+L2)
#                                     pos_sim = max((k1),(k2))/min((k1),(k2))
#                                     pos_sim = round(pos_sim,2)
#                                     # pos_sim = abs(float(R2/R1) - float(L1/L2))
#                                     # pos_sim = normalized_pos_sim(R1,R2,L1,L2)
#                                     pos_similarities.append(pos_sim)

#                                     text = str(pos_sim)
#                                     font = cv2.FONT_HERSHEY_SIMPLEX  # You can choose different fonts
#                                     position = (50, 50)  # Coordinates of the bottom-left corner of the text string in the image
#                                     font_scale = 1  # Font scale factor
#                                     color = (255, 0, 0)  # Color in BGR (not RGB, be careful about this)
#                                     thickness = 2  # Thickness of the lines used to draw the text
                                    
#                                     # Using cv2.putText() method
#                                     cv2.putText(transformed_face, text, position, font, font_scale, color, thickness, cv2.LINE_AA)
                                    
                                    

                                    

                                    
                                            

                                            
                                            
                                            
                                                
                                                
                                                


                                        
                                        
                                    
    
#                 # Display the frames
#                 cv2.imshow('Original Frame', image)
#                 cv2.imshow('Transformed Face', transformed_face)
#                 if cv2.waitKey(5) & 0xFF == ord('q'):
#                     break
    
#         cap.release()
#         cv2.destroyAllWindows()

# # Replace 'video_path' with your video file path or set it to 0 for webcam
# process_video(0)
            





    

    



# # In[2]:


# def custom_date_parser(date_string):
#     return pd.to_datetime(date_string, format="%d/%m/%y %H:%M:%S")


# # In[3]:


# csv_file = './Python/out_data.csv'
# df = pd.read_csv(csv_file, parse_dates=['Time'], date_parser=custom_date_parser)

#         # Create bins for the 'Positional_Similarity' column with 0.25 intervals
# bins = [i * 0.25 for i in range(41)]  # 41 since we want to include 10 (0.25 * 40 = 10)
# labels = [f'{i * 0.25}-{(i + 1) * 0.25}' for i in range(40)]  # 40 intervals in total
# df['pos_similarity_interval'] = pd.cut(df['Positional_Similarity'], bins=bins, labels=labels)

# # Calculate the time duration for each row
# df['duration'] = df['Time'].diff()

# # Group the DataFrame by the binned 'pos_similarity_interval' column
# grouped_df = df.groupby('pos_similarity_interval')['duration'].sum().reset_index()

# # Convert the time intervals to seconds and remove bins with no or None values
# grouped_df['duration_seconds'] = grouped_df['duration'].dt.total_seconds()
# grouped_df.dropna(subset=['duration_seconds'], inplace=True)
# grouped_df.reset_index(drop=True, inplace=True)

# grouped_df = grouped_df[['pos_similarity_interval', 'duration_seconds']]
# grouped_df.to_csv('./Python/grouped_output.csv')

# # In[5]:



# # if args.connect:
# #         send_command_to_unity(socket, 'EXIT')

# # In[ ]:




