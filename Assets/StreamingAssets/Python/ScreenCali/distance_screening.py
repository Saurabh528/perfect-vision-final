import mediapipe as mp
import cv2
import numpy as np
import os
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
                print("Working")
                script_dir = os.path.dirname(__file__)
                file_path = os.path.join(script_dir, 'cv.txt')
                # file_path  = r"H:\Vpower2\perfect-vision\Python\ScreenCali\cv.txt"
                with open(file_path, 'w') as file:
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