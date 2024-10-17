import cv2
import os
import time

out = None
prevtime = 0
previous_frame = None
frame_duration = 0
start_time = 0

def init_writer(datadir, cap):
    global out, frame_duration
    # Get video properties
    fps = cap.get(cv2.CAP_PROP_FPS)  # Frames per second
    frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))  # Width of the video frames
    frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))  # Height of the video frames
    frame_duration = 1.0 / fps  # Time per frame in seconds

    # Define the codec and create a VideoWriter object to save the video
    output_video_path = os.path.join(datadir, "temp_video.avi")
    fourcc = cv2.VideoWriter_fourcc(*'XVID')  # Codec (e.g., 'XVID' for .avi files)
    out = cv2.VideoWriter(output_video_path, fourcc, fps, (frame_width, frame_height))

def write_frame(frame):

    global out, prevtime, start_time, previous_frame

    if not out.isOpened():
        return
    processing_time = 0
    if start_time != 0:
        processing_time = time.time() - start_time

    # If processing takes more than the frame duration, duplicate the last frame
    while processing_time > frame_duration * 1.5:
        # If the processing time exceeds one frame, we need to duplicate the last frame
        out.write(previous_frame)
        processing_time -= frame_duration
        
    start_time = time.time()
    previous_frame = frame

    out.write(frame)

def release_writer():
    out.release()
