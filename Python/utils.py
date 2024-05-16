
from  appdirs import user_data_dir
import cv2
import time
import datetime

app_name = "PerfectVision"
app_author = "Jatin"
logfilePath = user_data_dir(app_name, app_author) + "/Python.log"
def get_anonymous_directory():
    return user_data_dir(app_name, app_author) + "/PatientData/Anonymous"

def check_camera_ready(camera_index):
    """ Attempt to open the camera and check if it can retrieve a frame. """
    cap = cv2.VideoCapture(camera_index)
    if not cap.isOpened():
        cap.release()
        return False
    
    # Try to get a frame to ensure the camera is really working
    ret, frame = cap.read()
    cap.release()
    return ret

def wait_for_camera(camera_index, timeout=10):
    """ Wait for the camera to be ready, checking every 1 second until timeout. """
    start_time = time.time()
    while time.time() - start_time < timeout:
        if check_camera_ready(camera_index):
            print("Camera is ready!")
            return True
        print("Waiting for camera...")
        time.sleep(1)  # Wait a bit before trying again
    
    print("Failed to access camera within the timeout period.")
    return False
    
def append_to_log(text):
    """
    Appends the provided text to a log file at the specified file path.
    
    Args:
    file_path (str): The path to the log file.
    text (str): The text to append to the log file.
    """
    try:
       # Get current date and time
        current_time = datetime.datetime.now()
        # Format the timestamp
        timestamp = current_time.strftime('%Y-%m-%d %H:%M:%S')
        
        # Open the log file in append mode
        with open(logfilePath, 'a') as file:
            # Append the timestamp, followed by the text and a newline
            file.write(f"{timestamp} - {text}\n")
            print("Log updated successfully.")
    except Exception as e:
        print(f"Failed to write to log file: {e}")
    
