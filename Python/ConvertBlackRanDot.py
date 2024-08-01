
import cv2
import numpy as np
import os

def process_image(input_image_path):
    # Load the image with OpenCV
    img = cv2.imread(input_image_path, cv2.IMREAD_COLOR)  # Loads the image in BGR format

    if img is None:
        raise FileNotFoundError(f"Image not found: {input_image_path}")

    # Convert the image from BGR to RGB (OpenCV uses BGR by default)
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    # Compute the alpha channel based on the maximum value among R, G, and B
    max_rgb = np.max(img_rgb, axis=2)  # Get maximum RGB value for each pixel
    alpha_channel = max_rgb  # Use this as the alpha channel

    # Stack the RGB image and alpha channel to create an RGBA image
    img_rgba = np.dstack((img_rgb, alpha_channel))

    # Generate the output file name by appending '_converted' before the file extension
    base, ext = os.path.splitext(input_image_path)
    output_image_path = f"{base}_converted{ext}"

    # Save the new image (converting RGBA to BGRA for OpenCV saving)
    cv2.imwrite(output_image_path, cv2.cvtColor(img_rgba, cv2.COLOR_RGBA2BGRA))

    print(f"Image saved as {output_image_path}")

# Example usage
input_image_path = 'f:/temp/RandomDotTest/VisualSymbols/Screenshot_27.png'  # Replace with your image path
process_image(input_image_path)
