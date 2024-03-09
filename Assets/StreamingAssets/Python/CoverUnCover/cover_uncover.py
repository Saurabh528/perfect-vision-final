#!/usr/bin/env python
# coding: utf-8

# ### diagnosis.py
# 

# In[9]:


"""Diagnose summary statistics of the cover/uncover eye test"""
from argparse import ArgumentParser
import json
import os
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

with open('./Python/DPI.txt', 'r') as f:
    lines = f.readlines()
patientName = lines[0].strip()
print('patientName: ' + patientName)

def diagnoseleft_to_json(before, after):
    """
    Diagnose the summary statistics before and after cover
    in cover/uncover eye test and save result to json file
    Parameters:
        before: Path to JSON of summary statistics before cover
        after: Path to JSON of summary statistics after cover
    Returns:
        diagnosis: String of diagnosis
    """
    with open(before, "r") as f:
        before = json.load(f)
    with open(after, "r") as f:
        after = json.load(f)
    diagnosis = diagnose(before, after)
    diagnosis_json = json.dumps({"diagnosisleft": diagnosis})
    if not os.path.exists("./Python/" + patientName):
        os.mkdir("./Python/" + patientName)
    with open("./Python/" + patientName + "/diagnosisleft.json", "w") as f:
        f.write(diagnosis_json)
    pass

def diagnoseright_to_json(before, after):
    """
    Diagnose the summary statistics before and after cover
    in cover/uncover eye test and save result to json file
    Parameters:
        before: Path to JSON of summary statistics before cover
        after: Path to JSON of summary statistics after cover
    Returns:
        diagnosis: String of diagnosis
    """
    with open(before, "r") as f:
        before = json.load(f)
    with open(after, "r") as f:
        after = json.load(f)
    diagnosis = diagnose(before, after)
    diagnosis_json = json.dumps({"diagnosisright": diagnosis})
    if not os.path.exists("./Python/" + patientName):
        os.mkdir("./Python/" + patientName)
    with open("./Python/" + patientName + "/diagnosisright.json", "w") as f:
        f.write(diagnosis_json)
    pass

def diagnose(before, after):
    """
    Diagnose the summary statistics before and after cover
    in cover/uncover eye test
    Parameters:
        before: Dictionary of summary statistics before cover
        after: Dictionary of summary statistics after cover
    Returns:
        diagnosis: String of diagnosis
    """
    TOLERANCE = 4 # mm
    N = 180 # number of samples
    T_CRIT = 1.9734 # critical t-value at 95% confidence with DOF=178

    expo_estotropia_test = (before["cornea10Dx"], after["cornea10Dx"], "Expotrophia", "Esotrophia") 
    hyper_hypotropia_test = (before["cornea10Dy"], after["cornea10Dy"], "Hypertropia", "Hypotrophia")
    test1_var =  cauchy_schwartz_variance(expo_estotropia_test[0]["std"], expo_estotropia_test[1]["std"], N)
    test1 = confidence_interval(expo_estotropia_test[0]["mean"], expo_estotropia_test[1]["mean"],test1_var, N, T_CRIT)
    test2_var = cauchy_schwartz_variance(hyper_hypotropia_test[0]["std"], hyper_hypotropia_test[1]["std"], N)
    test2 = confidence_interval(hyper_hypotropia_test[0]["mean"], hyper_hypotropia_test[1]["mean"],test2_var, N, T_CRIT)
    if test1[0] > TOLERANCE:
        return expo_estotropia_test[2]
    if test1[1] < -1 * TOLERANCE:
        return expo_estotropia_test[3]
    if test2[0] > TOLERANCE:
        return hyper_hypotropia_test[2]
    if test2[1] < -1 * TOLERANCE:
        return hyper_hypotropia_test[3]
    else:
        return "Normal"

def cauchy_schwartz_variance(std1, std2, N):
    """Calculate the variance of the difference of two samples
    using the Cauchy Schwartz Variance/Inequality
    Parameters:
        std1: standard deviation of sample 1
        std2: standard deviation of sample 2
        N: number of samples in total
    """
    return (std1 + std2)**2 / N

def confidence_interval(mu1, mu2, var, N, T_CRIT):
    """Calculate 95% confidence interval
    Parameters:
        mu1: mean of sample 1
        mu2: mean of sample 2
        var: variance of the difference of 
            means of sample 1 and 2
        N: number of samples in total
        T_CRIT: critical t-value at 95% confidence with DOF=N-2
    """
    return (mu1 - mu2 + (T_CRIT * var / N), mu1 - mu2 - (T_CRIT * var / N))


# ### get_measurment

# In[10]:


def get_measurements(face0, measurement_name="measurement"): 
    """Takes in face landmark and original face 
    and returns raw measurements of the face.
    Parameters:
        face: the face
    Returns:
        measurements: Measurement object
    """
    boxOD =  {
                "height": distance((face0[119].x,face0[119].y), (face0[52].x,face0[52].y)),
                "width": distance([face0[245].x,face0[245].y], [face0[35].x,face0[35].y]),
                "xMax": face0[245].x,
                "xMin": face0[35].x,
                "yMax": face0[119].y,
                "yMin": face0[52].y,
                    }
    boxOS = {
        "height": distance([face0[348].x,face0[348].y], [face0[282].x,face0[282].y]),
        "width": distance([face0[446].x,face0[446].y], [face0[465].x,face0[465].y]),
        "xMax": face0[446].x,
        "xMin": face0[465].x,
        "yMax": face0[348].y,
        "yMin": face0[282].y,
    }
    # define cornea
    corneaOD = {
        "x": face0[468].x,
        "y": face0[468].y,
        "h1": [face0[469].x,face0[469].y],
        "h2": [face0[471].x,face0[471].y],
        "radius": ( distance([face0[469].x,face0[469].y],
                            [face0[471].x,face0[471].y])  )/2
    }
    corneaOS = {
        "x": face0[473].x,
        "y": face0[473].y,
        "h1": [face0[474].x,face0[474].y],
        "h2": [face0[476].x,face0[476].y],
        "radius": ( distance([face0[474].x,face0[474].y],
                            [face0[476].x,face0[476].y])  )/2
    }
    # define palpebral fissure
    palpebralOD = {
        # points: [
        #     {x:, y:}
        # ],
        "medialCanthus": {"x":face0[133].x, "y":face0[133].y},
        "lateralCanthus": {"x":face0[33].x, "y":face0[33].y},
        "infPoint": {"x":face0[145].x, "y":face0[145].y},
        "supPoint": {"x":face0[159].x, "y":face0[159].y},
        "outline": [ 
            [face0[33].x,face0[33].y],
            [face0[246].x,face0[246].y],
            [face0[161].x,face0[161].y],
            [face0[160].x,face0[160].y],
            [face0[159].x,face0[159].y],
            [face0[158].x,face0[158].y],
            [face0[157].x,face0[157].y],
            [face0[173].x,face0[173].y],
            [face0[133].x,face0[133].y],
            [face0[155].x,face0[155].y],
            [face0[154].x,face0[154].y],
            [face0[153].x,face0[153].y],
            [face0[145].x,face0[145].y],
            [face0[144].x,face0[144].y],
            [face0[163].x,face0[163].y],
            [face0[7].x,face0[7].y]
        ]
    }
    palpebralOS = {
        # points: [
        #     {x:, y:}
        # ],
        "medialCanthus": {"x":face0[362].x, "y":face0[362].y},
        "lateralCanthus": {"x":face0[263].x, "y":face0[263].y},
        "infPoint": {"x":face0[374].x, "y":face0[374].y},
        "supPoint": {"x":face0[386].x, "y":face0[386].y},
        "outline": [ 
            [face0[263].x,face0[263].y],
            [face0[466].x,face0[466].y],
            [face0[388].x,face0[388].y],
            [face0[387].x,face0[387].y],
            [face0[386].x,face0[386].y],
            [face0[385].x,face0[385].y],
            [face0[384].x,face0[384].y],
            [face0[398].x,face0[398].y],
            [face0[362].x,face0[362].y],
            [face0[382].x,face0[382].y],
            [face0[381].x,face0[381].y],
            [face0[380].x,face0[380].y],
            [face0[374].x,face0[374].y],
            [face0[373].x,face0[373].y],
            [face0[390].x,face0[390].y],
            [face0[249].x,face0[249].y]
        ]
    }

    # scaleFactor: au to mm, scaleFactor2 au^2 to mm^2. Average OD and OS cornea for 2 samples
    corneaRadius = 5.855 # white to white = 11.71mm doi: 10.1097/01.ico.0000148312.01805.53
    scaleFactor = ((corneaOD["radius"] / corneaRadius) + (corneaOS["radius"] / corneaRadius))/2
    scaleFactor2 = ((square(corneaOD["radius"]) / square(corneaRadius)) + (square(corneaOS["radius"]) / square(corneaRadius)))/2

    # Calculated measures
    # Use distance from intersect between mid corneal horizontal line and sup-inf PF line and sup or inf PF point for MRD1/MRD2
    MidCornealIntersectOD = intersect(corneaOD["h1"], corneaOD["h2"], [palpebralOD["supPoint"]["x"], palpebralOD["supPoint"]["y"]], [palpebralOD["infPoint"]["x"], palpebralOD["infPoint"]["y"]])
    mrd1OD = distance(MidCornealIntersectOD,
                    [palpebralOD["supPoint"]["x"], palpebralOD["supPoint"]["y"]])
    mrd2OD = distance(MidCornealIntersectOD,
                    [palpebralOD["infPoint"]["x"],palpebralOD["infPoint"]["y"]])
    pfhOD = distance([palpebralOD["infPoint"]["x"],palpebralOD["infPoint"]["y"]],
                    [palpebralOD["supPoint"]["x"],palpebralOD["supPoint"]["y"]])
    pfwOD = distance([palpebralOD["medialCanthus"]["x"],palpebralOD["medialCanthus"]["y"]],
                    [palpebralOD["lateralCanthus"]["x"],palpebralOD["lateralCanthus"]["y"]])
    pfaOD = polygon_area(palpebralOD["outline"])

    MidCornealIntersectOS = intersect(corneaOS["h1"], corneaOS["h2"], [palpebralOS["supPoint"]["x"], palpebralOS["supPoint"]["y"]], [palpebralOS["infPoint"]["x"], palpebralOS["infPoint"]["y"]])
    mrd1OS = distance(MidCornealIntersectOS,
                    [palpebralOS["supPoint"]["x"], palpebralOS["supPoint"]["y"]])
    mrd2OS = distance(MidCornealIntersectOS,
                    [palpebralOS["infPoint"]["x"],palpebralOS["infPoint"]["y"]])
    pfhOS = distance([palpebralOS["infPoint"]["x"],palpebralOS["infPoint"]["y"]],
                    [palpebralOS["supPoint"]["x"],palpebralOS["supPoint"]["y"]])
    pfwOS = distance([palpebralOS["medialCanthus"]["x"],palpebralOS["medialCanthus"]["y"]],
                    [palpebralOS["lateralCanthus"]["x"],palpebralOS["lateralCanthus"]["y"]])
    pfaOS = polygon_area(palpebralOS["outline"])

    ipd = distance(MidCornealIntersectOD, MidCornealIntersectOS)
    innerCanthal = distance([palpebralOS["medialCanthus"]["x"], palpebralOS["medialCanthus"]["y"]],
                            [palpebralOD["medialCanthus"]["x"], palpebralOD["medialCanthus"]["y"]])
    outerCanthal = distance([palpebralOS["lateralCanthus"]["x"], palpebralOS["lateralCanthus"]["y"]],
                            [palpebralOD["lateralCanthus"]["x"], palpebralOD["lateralCanthus"]["y"]])
    morphometryResults = Measurement({
    # "scaleFactor": scaleFactor,
    # calculated values
    "cornea10Dx": corneaOD["x"],
    "cornea10Dy": corneaOD["y"],
    "cornea20Sx": corneaOD["x"],
    "cornea20Sy": corneaOD["y"],
    "mrd1OD": mrd1OD/scaleFactor,
    "mrd2OD": mrd2OD/scaleFactor,
    "pfhOD": pfhOD/scaleFactor,
    "pfwOD": pfwOD/scaleFactor,
    "pfaOD": pfaOD/scaleFactor2,

    "mrd1OS": mrd1OS/scaleFactor,
    "mrd2OS": mrd2OS/scaleFactor,
    "pfhOS": pfhOS/scaleFactor,
    "pfwOS": pfwOS/scaleFactor,
    "pfaOS": pfaOS/scaleFactor2,

    "ipd": ipd/scaleFactor,
    "innerCanthal": innerCanthal/scaleFactor,
    "outerCanthal": outerCanthal/scaleFactor,
    # points
    # "faceMesh": face0_original,
    # "boxOD": boxOD,
    # "boxOS": boxOS,
    # "corneaOD": corneaOD,
    # "corneaOS": corneaOS,
    # "palpebralOD": palpebralOD,
    # "palpebralOS": palpebralOS
    }, measurement_name)
    return morphometryResults


# ### helper

# In[11]:


"""Helper functions"""
import numpy as np

def distance(p1, p2):
    """Takes two points in R^2 and returns the distance between 
    them using Pythagorean theorem.
    Parameters:
        p1: first point in R^2 as a list
        p2: second point in R^2 as a list
    Returns:
        distance: distance between the two points
    """
    return ((p1[0] - p2[0])**2 + (p1[1] - p2[1])**2)**0.5

def square(x):
    """Takes a number and returns the square of that number.
    Parameters:
        x: number
    Returns:
        x**2: square of the number
    """
    return x**2

def polygon_area(points):
    """Takes a list of points in R^2 and returns the 
    area of the polygon using shoelace formula
    Parameters:
        points: list of points in R^2 as a list
    Returns:
        area: area of the polygon
    """
    x = []
    y = []
    for i in points:
        x.append(i[0])
        y.append(i[1])
    correction = x[-1] * y[0] - y[-1]* x[0]
    main_area = np.dot(x[:-1], y[1:]) - np.dot(y[:-1], x[1:])
    return 0.5*np.abs(main_area + correction)

def intersect(p1, p2, p3, p4):
    """Takes four points in R^2 and returns the intersection
    of the two lines defined by the points.
    Parameters:
        p1: first point in R^2 as a list
        p2: second point in R^2 as a list
        p3: third point in R^2 as a list
        p4: fourth point in R^2 as a list
    Returns:
        intersection: intersection of the two lines made by
        (p1, p2) and (p3, p4)
    """
    line1 = [p1, p2]
    line2 = [p3, p4]
    xdiff = (line1[0][0] - line1[1][0], line2[0][0] - line2[1][0])
    ydiff = (line1[0][1] - line1[1][1], line2[0][1] - line2[1][1])

    def det(a, b):
        """Returns determinant of 2x2 matrix made of vectors a and b
        Parameters:
            a: first vector as a list
            b: second vector as a list
        Returns:
            det: determinant of the matrix [a|b]
        """
        return a[0] * b[1] - a[1] * b[0]

    div = det(xdiff, ydiff)
    if div == 0:
        raise Exception('lines do not intersect')

    d = (det(*line1), det(*line2))
    x = det(d, xdiff) / div
    y = det(d, ydiff) / div
    return [x, y]


# ### measurement 

# In[12]:



class Measurement():
    """Measurements"""
    
    def __init__(self, measurement_dict, measurement_name="measurement"):
        self.measurement_dict = measurement_dict
        self.measurement_name = measurement_name
        for measurement in self.measurement_dict.keys():
            setattr(self, measurement, self.measurement_dict[measurement])
    
    def to_json(self):
        """Convert to json"""
        measurement_json = json.dumps(self.measurement_dict)
        if not os.path.exists("./Python/" + patientName):
            os.mkdir("./Python/" + patientName)
        with open("./Python/" + patientName + "/" + ".".join([self.measurement_name, "json"]), "w") as f:
            f.write(measurement_json)
        pass

class MeasurementStatistics(Measurement):
    """Measurement Statistics"""

    def __init__(self, measurement_dict, measurement_name="measurement"):
        super().__init__(measurement_dict, measurement_name)


# ### statistics

# In[13]:


import numpy as np

def summary_statistics(measurements):
    """Takes a list of measurements and returns measurement summary statistics
    Parameters:
        measurements: list of values
    Returns:
        summary_statistics: dictionary of summary statistics
            - key: statistic name (e.g. "mean")
            - value: statistic value
    """
    try:
        summary_statistics = {
            "mean": np.mean(measurements),
            "std": np.std(measurements),
            "min": np.min(measurements),
            "max": np.max(measurements),
            "median": np.median(measurements),
            "range": np.max(measurements) - np.min(measurements)
        }
        return summary_statistics
    except TypeError:
        return {}


# ### web cam measurement

# In[14]:


import math
import cv2
import mediapipe as mp


def webcam_measurement(time, side):
    #side: 0: none, 1: left, 2- right
  """Takes in amount of time to run webcam and returns measurement 
  summary statitics
  Parameters:
    time: amount of time in seconds to run the webcam for
  Returns:
    summary_statistics: dictionary of dictionaries of 
      measurement summary statistics
      - key: measurement name (e.g. "cornea10Dx")
      - value: dictionary of measurement summary statistics
        - key: summary statistic name (mean, std, min, max, median, range)
        - value: summary statistic value
  """
  sample_max = math.floor(time * 30)
  measurements = dict()
  sample_num = 0
  mp_drawing = mp.solutions.drawing_utils
  mp_drawing_styles = mp.solutions.drawing_styles
  mp_face_mesh = mp.solutions.face_mesh
  # drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
  cap = cv2.VideoCapture(args.cameraindex)
  with mp_face_mesh.FaceMesh(
      max_num_faces=1,
      refine_landmarks=True,
      min_detection_confidence=0.5,
      min_tracking_confidence=0.5) as face_mesh:
    while cap.isOpened():
      success, image = cap.read()
      if not success:
        print("Ignoring empty camera frame.")
        # If loading a video, use 'break' instead of 'continue'.
        continue

      # To improve performance, optionally mark the image as not writeable to
      # pass by reference.
      image.flags.writeable = False
      image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
      results = face_mesh.process(image)

      # Draw the face mesh annotations on the image.
      image.flags.writeable = True
      image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
      if results.multi_face_landmarks:
          for face_landmarks in results.multi_face_landmarks:
              mp_drawing.draw_landmarks(
                  image=image,
                  landmark_list=face_landmarks,
                  connections=mp_face_mesh.FACEMESH_TESSELATION,
                  landmark_drawing_spec=None,
                  connection_drawing_spec=mp_drawing_styles
                  .get_default_face_mesh_tesselation_style())
              mp_drawing.draw_landmarks(
                  image=image,
                  landmark_list=face_landmarks,
                  connections=mp_face_mesh.FACEMESH_CONTOURS,
                  landmark_drawing_spec=None,
                  connection_drawing_spec=mp_drawing_styles
                  .get_default_face_mesh_contours_style())
              mp_drawing.draw_landmarks(
                  image=image,
                  landmark_list=face_landmarks,
                  connections=mp_face_mesh.FACEMESH_IRISES,
                  landmark_drawing_spec=None,
                  connection_drawing_spec=mp_drawing_styles
                  .get_default_face_mesh_iris_connections_style())
          
          face0_original = results.multi_face_landmarks[0]
          face0 = face0_original.landmark
          curr_measurent = get_measurements(face0).measurement_dict
          # print(curr_measurent)
          if sample_num == 0:
             for key in curr_measurent.keys():
              measurements[key] = [curr_measurent[key]]
          else:
            for key in curr_measurent.keys():
              measurements[key].append(curr_measurent[key])
          sample_num += 1
          if sample_num == sample_max:
            break
      # Flip the image horizontally for a selfie-view display.
          if not args.quiet:  
            cv2.imshow('MediaPipe Face Mesh', cv2.flip(image, 1))
          
      if cv2.waitKey(5) & 0xFF == 27:
        break
    
    if side == 1:
      measurement_statistics = dict()
      for measurement in measurements.keys():
        measurement_statistics[measurement] = summary_statistics(measurements[measurement])
      # print(measurement_statistics)
      cap.release()
      if not os.path.exists("./Python/" + patientName + "/beforeleft.json"):
        measurement_statistics = MeasurementStatistics(measurement_statistics, "beforeleft")
        measurement_statistics.to_json()
      else:
        measurement_statistics = MeasurementStatistics(measurement_statistics, "afterleft")
        measurement_statistics.to_json()
        diagnoseleft_to_json("Python/" + patientName + "/beforeleft.json", "Python/" + patientName + "/afterleft.json")
    elif side == 2:
      measurement_statistics = dict()
      for measurement in measurements.keys():
          measurement_statistics[measurement] = summary_statistics(measurements[measurement])
      if not os.path.exists("./Python/" + patientName + "/beforeright.json"):
        measurement_statistics = MeasurementStatistics(measurement_statistics, "beforeright")
        measurement_statistics.to_json()
      else:
        measurement_statistics = MeasurementStatistics(measurement_statistics, "afterright")
        measurement_statistics.to_json()
        diagnoseright_to_json("Python/" + patientName + "/beforeright.json", "Python/" + patientName + "/afterright.json")
      return measurement_statistics.measurement_dict


# In[15]:


import sys
import time
# from webcam_measurement import *
def main():
    # Initialize TCP connection
    if args.connect:
        socket = init_TCP()
    # 3 second delay
    time.sleep(0.5)
    
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
    webcam_measurement(3, 0)
    # Cover
    if args.connect:
        send_message_to_unity(socket, 'Cover left eye')
    else:
        print("Cover left eye")
    time.sleep(3)
    
    # After measurement
    if args.connect:
        send_message_to_unity(socket, 'Remove the Cover')
    else:
        print("Remove the Cover")
    webcam_measurement(3, 1)
    
    # Cover
    if args.connect:
        send_message_to_unity(socket, 'Cover right eye')
    else:
        print("Cover right eye")
    time.sleep(3)
    
    # After measurement
    if args.connect:
        send_message_to_unity(socket, 'Remove the Cover')
    else:
        print("Remove the Cover")
    webcam_measurement(3, 2)
    
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





# In[ ]:




