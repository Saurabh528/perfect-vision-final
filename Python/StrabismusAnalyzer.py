import mediapipe as mp
import cv2
import numpy as np
import pandas as pd
from datetime import datetime
class StrabismusAnalyzer:
    def __init__(self):
        self.mp_face_mesh = mp.solutions.face_mesh
        self.face_mesh = mp.solutions.face_mesh.FaceMesh(
            max_num_faces=1,
            refine_landmarks=True,
            min_detection_confidence=0.5
        )
        # Correct landmark indices
        self.LEFT_EYE = {
            'iris': 473,
            'left_pt': 263,
            'right_pt': 398,
            'outer': 263,  # lateral canthus
            'inner': 398   # medial canthus
        }
        self.RIGHT_EYE = {
            'iris': 468,
            'left_pt': 133,
            'right_pt': 33,
            'outer': 33,   # lateral canthus
            'inner': 133   # medial canthus
        }
        # Constants for measurement
        self.CORNEAL_DIAMETER_MM = 11.7
        self.HIRSCHBERG_RATIO = 22/1.0
        self.MARGIN_ERROR_MM = 0.3  # 0.3mm margin of error
        self.NOSE_TIP_LANDMARKS = [1,2]
        # Time tracking
        self.start_time = None
        self.measurements = []
    def calc_distance(self, p1, p2):
        return np.sqrt((p1[0] - p2[0])**2 + (p1[1] - p2[1])**2)
    def get_mm_scale(self, eye_points):
        """Calculate mm per pixel using corneal diameter"""
        corneal_width_px = self.calc_distance(eye_points['inner'], eye_points['outer'])
        return self.CORNEAL_DIAMETER_MM / corneal_width_px
    def calculate_deviation_mm(self, iris_pos, eye_center, mm_per_pixel):
        """Calculate actual deviation in millimeters with error margin"""
        displacement_px = iris_pos[0] - eye_center[0]
        deviation_mm = displacement_px * mm_per_pixel
        if deviation_mm !=0:
            sign = abs(deviation_mm)/deviation_mm
            deviation_mm = abs(deviation_mm)- self.MARGIN_ERROR_MM
            deviation_mm = deviation_mm * sign
            return deviation_mm
        else:
            return 0
    def calculate_nose_tip(self, landmarks, h, w):
        x_coords = [landmarks[idx].x * w for idx in self.NOSE_TIP_LANDMARKS]
        y_coords = [landmarks[idx].y * h for idx in self.NOSE_TIP_LANDMARKS]
        avg_x = sum(x_coords) / len(x_coords)
        avg_y = sum(y_coords) / len(y_coords)
        return int(avg_x), int(avg_y)
    def analyze_eye(self, points, mm_per_pixel):
        """Analyze single eye deviation in mm"""
        center_x = (points['inner'][0] + points['outer'][0]) / 2
        center_y = (points['inner'][1] + points['outer'][1]) / 2
        center = (int(center_x), int(center_y))
        deviation_mm = self.calculate_deviation_mm(points['iris'], center, mm_per_pixel)
        pd = deviation_mm * self.HIRSCHBERG_RATIO
        return pd, center, deviation_mm
    def process_frame(self, frame):
        if self.start_time is None:
            self.start_time = datetime.now()
        current_time = (datetime.now() - self.start_time).total_seconds()
        h, w = frame.shape[:2]
        results = self.face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        if not results.multi_face_landmarks:
            return None
        landmarks = results.multi_face_landmarks[0]
        vis_frame = frame.copy()
        def get_point(idx):
            return (int(landmarks.landmark[idx].x * w),
                   int(landmarks.landmark[idx].y * h))
        # Get all required points
        left_points = {
            'iris': get_point(self.LEFT_EYE['iris']),
            'inner': get_point(self.LEFT_EYE['inner']),
            'outer': get_point(self.LEFT_EYE['outer'])
        }
        right_points = {
            'iris': get_point(self.RIGHT_EYE['iris']),
            'inner': get_point(self.RIGHT_EYE['inner']),
            'outer': get_point(self.RIGHT_EYE['outer'])
        }
        # Calculate mm scale for each eye
        left_mm_per_pixel = self.get_mm_scale(left_points)
        right_mm_per_pixel = self.get_mm_scale(right_points)
        # Analyze each eye
        left_pd, left_center, left_mm = self.analyze_eye(left_points, left_mm_per_pixel)
        right_pd, right_center, right_mm = self.analyze_eye(right_points, right_mm_per_pixel)
        # Calculate total deviation
        total_pd = (left_pd + right_pd) / 2
        # Determine strabismus type
        if abs(total_pd) < 4:  # Clinical significance threshold
            deviation_type = "No significant deviation"
        else:
            deviation_type = "Esotropia" if total_pd < 0 else "Exotropia"
        # Draw visualization
        # Left eye (blue)
        cv2.circle(vis_frame, left_points['outer'], 3, (255, 0, 0), -1)
        cv2.circle(vis_frame, left_points['inner'], 3, (255, 0, 0), -1)
        cv2.circle(vis_frame, left_points['iris'], 3, (0, 255, 0), -1)
        cv2.circle(vis_frame, left_center, 2, (0, 255, 255), -1)
        # Right eye (red)
        cv2.circle(vis_frame, right_points['outer'], 3, (0, 0, 255), -1)
        cv2.circle(vis_frame, right_points['inner'], 3, (0, 0, 255), -1)
        cv2.circle(vis_frame, right_points['iris'], 3, (0, 255, 0), -1)
        cv2.circle(vis_frame, right_center, 2, (0, 255, 255), -1)
        # Nose Tip
        nose_tip = self.calculate_nose_tip(landmarks.landmark, h, w)
        cv2.circle(vis_frame, nose_tip, 2, (0, 255, 255), 5)
        # Display timer and measurements
        font = cv2.FONT_HERSHEY_SIMPLEX
        # Timer display
        cv2.putText(vis_frame, f"Time: {current_time:.1f}s",
                   (w - 150, 30), font, 0.7, (255, 255, 255), 2)
        info_lines = [
            f"Status: {deviation_type}",
            f"Left Eye: {left_pd:.1f}PD ({left_mm:.2f}mm)",
            f"Right Eye: {right_pd:.1f}PD ({right_mm:.2f}mm)",
            f"Total Deviation: {abs(total_pd):.1f}PD"
        ]
        for i, line in enumerate(info_lines):
            cv2.putText(vis_frame, line, (10, 60 + i*25),
                       font, 0.7, (0, 255, 0), 2)
        # Store measurements with timestamp
        self.measurements.append({
            'timestamp': current_time,
            'left_mm': left_mm,
            'right_mm': right_mm,
            'left_pd': left_pd,
            'right_pd': right_pd,
            'total_pd': total_pd,
            'deviation_type': deviation_type
        })
        return vis_frame
    def get_phase_statistics(self, df):
        """Calculate statistics for a specific time phase"""
        return {
            'mm_deviation': {
                'left_mean': df['left_mm'].mean(),
                'left_std': df['left_mm'].std(),
                'right_mean': df['right_mm'].mean(),
                'right_std': df['right_mm'].std()
            },
            'pd_measurements': {
                'left_mean': df['left_pd'].mean(),
                'left_std': df['left_pd'].std(),
                'right_mean': df['right_pd'].mean(),
                'right_std': df['right_pd'].std(),
                'total_mean': df['total_pd'].mean(),
                'total_std': df['total_pd'].std()
            },
            'analysis': {
                'frames': len(df),
                'significant_deviation_pct': (df['total_pd'].abs() > 4).mean() * 100
            }
        }
    def get_statistics(self):
        """Calculate phase-wise statistics"""
        if not self.measurements:
            return None
        df = pd.DataFrame(self.measurements)
        # Split into phases
        first_phase = df[df['timestamp'] <= 10]
        second_phase = df[(df['timestamp'] > 10) & (df['timestamp'] <= 20)]
        return {
            'first_phase': self.get_phase_statistics(first_phase),
            'second_phase': self.get_phase_statistics(second_phase)
        }
def main():
    analyzer = StrabismusAnalyzer()
    cap = cv2.VideoCapture(0)
    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
            result = analyzer.process_frame(frame)
            if result is not None:
                cv2.imshow('Strabismus Analysis', result)
            # Stop after 20 seconds
            if analyzer.start_time is not None:
                elapsed_time = (datetime.now() - analyzer.start_time).total_seconds()
                if elapsed_time > 20:
                    break
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    finally:
        cap.release()
        cv2.destroyAllWindows()
        # Print phase-wise statistics
        stats = analyzer.get_statistics()
        if stats:
            for phase in ['first_phase', 'second_phase']:
                phase_stats = stats[phase]
                print(f"\n{phase.replace('_', ' ').title()} (0-10s)" if phase == 'first_phase' else "\nSecond Phase (11-20s)")
                print("-" * 40)
                print("\nPrism Diopter Measurements:")
                print(f"Left Eye: {phase_stats['pd_measurements']['left_mean']:.1f} ± {phase_stats['pd_measurements']['left_std']:.1f} PD")
                print(f"Right Eye: {phase_stats['pd_measurements']['right_mean']:.1f} ± {phase_stats['pd_measurements']['right_std']:.1f} PD")
                print(f"Total Deviation: {abs(phase_stats['pd_measurements']['total_mean']):.1f} ± {phase_stats['pd_measurements']['total_std']:.1f} PD")
                print(f"\nSignificant Deviation in {phase_stats['analysis']['significant_deviation_pct']:.1f}% of frames")
                print(f"Frames Analyzed: {phase_stats['analysis']['frames']}")
if __name__ == "__main__":
    main()